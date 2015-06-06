using Storage.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Microsoft.SqlServer.Server;

namespace FileStorage
{
    public class FileStorageWithIndex :DisposableObject, IFileStorageIndex, IFileStorageWriter
    {
        private bool _isNewFile;
        private readonly IFileStorageReaderAndWriter _fileStorageReaderAndWriter;
        private readonly object _syncObject = new object();
        private DateTime _startRange;
        private DateTime _finishRange;
        private long? _fileSize;

        readonly Dictionary<ushort, byte> _sourceIds = new Dictionary<ushort, byte>();
        readonly Dictionary<byte, byte> _dataTypeIds = new Dictionary<byte, byte>();
        readonly List<RecordInfo> _records = new List<RecordInfo>(); 
        
        public FileStorageWithIndex(bool isNewFile, IFileStorageReaderAndWriter fileStorageReaderAndWriter)//создаем индекс по уже существующиму файлу
        {
            if (fileStorageReaderAndWriter == null)
                throw new ArgumentNullException("fileStorageReaderAndWriter");
            _isNewFile = isNewFile;
            _fileStorageReaderAndWriter = fileStorageReaderAndWriter;

            if (isNewFile == false)//открываем индекс уже существующего файла
            {
                _fileSize = fileStorageReaderAndWriter.FileSize;
                if (File.Exists(_getIndexFileName())) //восстанавливаем информацию об индексе
                {
                    _rectoreIndex();
                }
                else //отсутвует файл с индексом, сканируем файл и восстанавливаем
                {
                    _fileStorageReaderAndWriter.ScanFileAndFillIndex(this);
                    _saveIndexToFile();
                }

            }
            else //создаем пустой индекс, для нового файла
            {
                _startRange=DateTime.MinValue;
                _finishRange=DateTime.MinValue;
                fileStorageReaderAndWriter.OpenStream(); //открываем файл для записи
            }
        }

        
        class RecordInfo
        {
            public long FilePosition;
            public DateTime DateTime;
            public ushort SourceId;
            public byte DataTypeId;
        }
        

        public void AddDataToIndex(long recordFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId)
        {
            lock (_syncObject)
            {
                _addDataToIndex(recordFilePosition, dateTime, sourceId, dataTypeId);
            }
        }

        private void _addDataToIndex(long recordFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId)
        {
            if (_finishRange > dateTime)
            {
                throw new Exception(String.Format("Time problem _finishRange[{0}] > dateTime[{1}]", _finishRange, dateTime));
            }
            if (_startRange == DateTime.MinValue)
            {
                _startRange = dateTime;
                _finishRange = dateTime;
            }
            else
            {
                _finishRange = dateTime;
            }

            if (_sourceIds.ContainsKey(sourceId) == false) //заполняем список источников, которые присутвуют в данном индексе
            {
                _sourceIds.Add(sourceId, 0);
            }
            if (_dataTypeIds.ContainsKey(dataTypeId) == false)
                //заполняем список типов данных, которые присутвуют в данном индексе
            {
                _dataTypeIds.Add(dataTypeId, 0);
            }
            _records.Add(new RecordInfo()
            {
                FilePosition = recordFilePosition,
                DataTypeId = dataTypeId,
                DateTime = dateTime,
                SourceId = sourceId
            });
        }

        public void StopWritingDataToFile()
        {
            lock (_syncObject)
            {
                if(!_isNewFile)throw new InvalidOperationException("File is readonly");
                if (_records.Count == 0)
                {
                    throw new Exception("Index without data");
                }
                _saveIndexToFile();

                _isNewFile = false;
                _fileStorageReaderAndWriter.CloseStream();
                
            }
        }

        private void _saveIndexToFile()//сохраняем индекс в файл
        {
            var indexFileName = _getIndexFileName();
            using (FileStream file = File.Create(indexFileName))
            {
                var buffer = BitConverter.GetBytes(_records.Count);
                file.Write(buffer, 0, buffer.Length);//записываем количество записей в файле

                foreach (RecordInfo record in _records)
                {
                    file.WriteByte(Byte.MaxValue);

                    buffer = BitConverter.GetBytes(record.DateTime.ToBinary());
                    file.Write(buffer, 0, buffer.Length);//записываем дату записи
                    
                    buffer = BitConverter.GetBytes(record.SourceId);
                    file.Write(buffer, 0, buffer.Length);//записываем идентификатор источника

                    file.WriteByte(record.DataTypeId); //записываем тип данных

                    buffer = BitConverter.GetBytes(record.FilePosition);
                    file.Write(buffer, 0, buffer.Length);//записываем позицию записи в основном файле
                }
                file.Flush();
            }
        }


        private void _rectoreIndex()//сохраняем индекс в файл
        {
            using (FileStream file = File.OpenRead(_getIndexFileName()))
            {
                byte[] buffer = new byte[8];

                file.Read(buffer, 0, 4);
                int size = BitConverter.ToInt32(buffer, 0);//считываем количество записей в файле
                if (size < 0)
                {
                    throw new InvalidDataException("Файл индекса поврежден! Отрицательное количество записей");
                }
                for (int i = 0; i < size; i++)
                {
                    if (file.ReadByte() != Byte.MaxValue)
                    {
                        throw new InvalidDataException(String.Format("Файл индекса поврежден! Не валидное начало записи[{0}]",i));
                    }
                    file.Read(buffer, 0, 8); //считываем дату записи
                    long dateTimeBinary = BitConverter.ToInt64(buffer, 0);
                    
                    var recordTime = DateTime.FromBinary(dateTimeBinary);

                    file.Read(buffer, 0, 2); //считываем идентификатор источника
                    ushort sourceId = BitConverter.ToUInt16(buffer, 0);


                    file.Read(buffer, 0, 1);
                    byte dataTypeId = buffer[0];

                    file.Read(buffer, 0, 8); //считываем позицию записи в данном файле
                    long recordPosition = BitConverter.ToInt64(buffer, 0);

                    _addDataToIndex(recordPosition, recordTime, sourceId, dataTypeId);
                }
            }
        }

        private string _getIndexFileName()
        {
            FileInfo sourceFileInfo = new FileInfo(_fileStorageReaderAndWriter.FileName);
            var file =string.Concat(Path.GetFileNameWithoutExtension(sourceFileInfo.Name),".idx");
            file = Path.Combine(sourceFileInfo.Directory.FullName, file);
            return file;
        }

        public string FileName { get; set; }

        public DateTime StartRange
        {
            get
            {
                lock (_syncObject)
                {
                    return _startRange;
                }
            }
        }

        public DateTime FinishRange
        {
            get
            {
                lock (_syncObject)
                {
                    return _finishRange;
                }
            }
        }

        public FileStorageInfo GetWorkInfo()
        {
            lock (_syncObject)
            {
                FileStorageInfo ret = new FileStorageInfo();
                ret.IsNewFile = _isNewFile;
                ret.SavedTimeRangeUTC = new DateTimeRange() { StartTime = _startRange, FinishTime = _finishRange };
                ret.CountRecords = _records.Count;

                if (_fileSize.HasValue)
                {
                    ret.SizeInBytes = _fileSize.Value;
                }
                else
                {
                    ret.SizeInBytes = _fileStorageReaderAndWriter.FileSize;    
                }
                ret.FileName = _fileStorageReaderAndWriter.FileName;
                return ret;
            }
        }

        public void ProcessSearchRequest(ISearchRequestData request)
        {
            if (request == null) throw new ArgumentNullException("request");

            
            lock (_syncObject)
            {

                if (_records.Count > 0 && request.FinishSearchRange>=_startRange && request.StartSearchRange<=_finishRange)
                {
                    bool filterBySourceIds = request.SearchSourceIds.Count > 0;
                    bool filterByTypeIds = request.TypeDataIds.Count > 0;

                    
                    if (filterBySourceIds && request.SearchSourceIds.Keys.Any(x => _sourceIds.ContainsKey(x)) == false)
                    {//запрашиваемые идентификаторы не присутвуют в данном файле
                        return;
                    }
                    if (filterByTypeIds && request.TypeDataIds.Keys.Any(x => _dataTypeIds.ContainsKey(x)) == false)
                    {
                        //запрашиваемые типы данных не присутвуют в данном файле
                        return;
                    }

                    bool streamOpened = _isNewFile;
                    try
                    {

                        foreach (var info in _records)
                        {
                            if(IsDisposed)break;
                            if (info.DateTime > request.FinishSearchRange)
                            {
                                break; //дальше данных в запрашиваемом диапазоне нет
                            }
                            if (info.DateTime >= request.StartSearchRange)
                            {
                                if (filterBySourceIds && request.SearchSourceIds.ContainsKey(info.SourceId) == false)
                                {
                                    //текущий идентификатор не запрашивается
                                    continue;
                                }
                                if (filterByTypeIds && request.TypeDataIds.ContainsKey(info.DataTypeId) == false)
                                {
                                    //текущий тип данных не запрашивается
                                    continue;
                                }
                                if (streamOpened==false)
                                {
                                    _fileStorageReaderAndWriter.OpenStream();
                                    streamOpened = true;
                                }

                                //текущая запись подходит загружаем данные
                                request.Add(_fileStorageReaderAndWriter.GetDbDataRecord(info.FilePosition));
                            }
                        }
                    }
                    finally
                    {
                        if (_isNewFile == false && streamOpened)
                        {
                            _fileStorageReaderAndWriter.CloseStream();
                        }
                    }
                }
            }
        }

        public bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data)
        {
            return _fileStorageReaderAndWriter.WriteRecord(sourceId, dataTypeId, data, this);
        }

        protected override void OnDisposed()
        {
            lock (_syncObject)
            {

                if (_isNewFile && _records.Count>0)
                {
                    _saveIndexToFile();
                }
                _fileStorageReaderAndWriter.Dispose();
            }
        }
    }
}
