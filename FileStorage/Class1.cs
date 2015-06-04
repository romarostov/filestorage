﻿using Storage.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage
{
    public interface IDirectoryStorageConfiguration
    {
        /// <summary>
        /// Максимальный размер 
        /// </summary>
        int MaximumMegabytesInFile { get; }

        int MinimumRecordDataSizeInBytes { get; }

        int MaximumRecordDataSizeInKilobytes { get; }


    }

    public class DirectoryStorageFileInfo
    {
        public string FileName { get; set; }

        public long SizeInBytes { get; set; }

        public int CountRecords { get; set; }

        public bool IsCurrent { get; set; }

        public DateTimeRange SavedTimeRangeUTC { get; set; }

    }

    public interface ITimeSerivice
    {
        /// <summary>
        /// Текущая дата в UTC формате
        /// </summary>
        DateTime UTCNow { get; }
    }

    class TimeSerivice : ITimeSerivice
    {
        public DateTime UTCNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }

    public class DirectoryStorage : IDataItemStore, IDisposable
    {
        private readonly string _directory;
        private readonly ITimeSerivice _timeSerivice;
        private readonly int _minimumRecordDataSizeInBytes;
        private readonly int _maximumRecordDataSizeInBytes;
        private readonly int _maximumBytesInFile;
        private readonly object _writeSyncObject = new object();
        CurrentFileStorage _currentFileStorage;
        private IFileWritingIndex _current_file_index;


        public DirectoryStorage(string directory, IDirectoryStorageConfiguration configuration)
            : this(directory, configuration, new TimeSerivice())
        {

        }

        public DirectoryStorage(string directory, IDirectoryStorageConfiguration configuration, ITimeSerivice timeSerivice)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (timeSerivice == null) throw new ArgumentNullException("timeSerivice");
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException("directory");
            if (Directory.Exists(directory) == false) throw new DirectoryNotFoundException(directory);
            if (configuration.MinimumRecordDataSizeInBytes < 1)
            {
                throw new InvalidDataException(string.Format("configuration.MinimumRecordDataSizeInBytes[{0}] < 1", configuration.MinimumRecordDataSizeInBytes));
            }
            if (configuration.MinimumRecordDataSizeInBytes * 1014 > configuration.MaximumRecordDataSizeInKilobytes)
            {
                throw new InvalidDataException(String.Format("configuration.MinimumRecordDataSizeInBytes*1014 [{0}]> configuration.MaximumRecordDataSizeInKilobytes[{1}]", configuration.MinimumRecordDataSizeInBytes * 1014, configuration.MaximumRecordDataSizeInKilobytes));
            }
            if (configuration.MaximumRecordDataSizeInKilobytes * 5 > configuration.MaximumMegabytesInFile * 1024)//проверяем что можно в файл записать хотя бы 5 максимальных записей
            {
                throw new InvalidDataException(string.Format("configuration.MaximumRecordDataSizeInKilobytes[{0}]*5 > configuration.MaximumMegabytesInFile[{1}]*1024", configuration.MaximumRecordDataSizeInKilobytes, configuration.MaximumMegabytesInFile));
            }
            _minimumRecordDataSizeInBytes = configuration.MinimumRecordDataSizeInBytes;
            _maximumRecordDataSizeInBytes = configuration.MaximumRecordDataSizeInKilobytes * 1024;
            _maximumBytesInFile = configuration.MaximumMegabytesInFile * 1024 * 1024;
            _directory = directory;
            _timeSerivice = timeSerivice;
        }

        public List<DirectoryStorageFileInfo> GetFilesInfos()
        {
            throw new NotImplementedException();
        }

        public DateTimeRange GetSavedRangeInUTC()
        {
            throw new NotImplementedException();
        }

        public List<DataItem> GetData(DateTime start_range, DateTime finish_range, List<int> source_ids, List<byte> data_type_ids)
        {
            throw new NotImplementedException();
        }

        public void SaveData(UInt16 source_id, byte data_type_id, byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length < _minimumRecordDataSizeInBytes)
            {
                throw new InvalidDataException(string.Format("data.Length[{0}] < MinimumRecordDataSizeInBytes[{1}]", data.Length, _minimumRecordDataSizeInBytes));
            }
            if (data.Length > _maximumRecordDataSizeInBytes)
            {
                throw new InvalidDataException(string.Format("data.Length[{0}] > MaximumRecordDataSizeInBytes[{1}]", data.Length, _maximumRecordDataSizeInBytes));
            }

            lock (_writeSyncObject)
            {
                if (_currentFileStorage == null)
                {
                    _current_file_index=new FileWritingIndex();
                    _currentFileStorage = new CurrentFileStorage(_directory, _timeSerivice, _maximumBytesInFile, _current_file_index);
                }

                bool? is_write=null;

                try
                {
                    is_write = _currentFileStorage.WriteRecord(source_id, data_type_id, data);
                }
                catch (Exception)
                {
                    //ошибка записи данных в файл, чтобы избежать ошибки в следствии частичной записи, закрываем этот файл и начинаем новый
                }

                //False - файл переполнен, записываем в новый
                if (is_write.HasValue==false || is_write.Value == false)
                {
                    var old_index = _current_file_index;
                    var oldFileStorage = _currentFileStorage;
                    
                    _currentFileStorage = null;
                    _current_file_index = null;

                    

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            //записываем индекс в файл
                            old_index.FlushIndexToFile(oldFileStorage.FileName);
                        }
                        catch (Exception)
                        {
                            
                        }
                    });

                    IFileWritingIndex new_index=new FileWritingIndex();
                    var newCurrentFile = new CurrentFileStorage(_directory, _timeSerivice, _maximumBytesInFile,new_index);
                    newCurrentFile.WriteRecord(source_id, data_type_id, data);

                    _currentFileStorage = newCurrentFile;
                    _current_file_index = new_index;
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Возвращает имя файла с данными по дате создания
        /// </summary>
        public static string GetFileNameByTime(DateTime createtionTime)
        {
            return String.Format("{0}{1:N2}{2:N2}{3:N2}{4:N2}{5:N2}{6}DB.dat",
                createtionTime.Year,
                createtionTime.Month,
                createtionTime.Day,
                createtionTime.Hour,
                createtionTime.Minute,
                createtionTime.Second, createtionTime.Millisecond);
        }

        /// <summary>
        /// "Уникальный" префик к файлам базы данных.
        /// 
        /// </summary>
        /// <returns></returns>
        public static byte[] GetFileDBPrefix()
        {
            return new byte[] { 20, 30, 2, 1, 4, 6, 3, 1 };
        }
    }
    
    public interface IFileWritingIndex
    {
        


        void AddDataToIndex(long currentFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId);

        void FlushIndexToFile(string data_base_file);
    }

    public class FileWritingIndex : IFileWritingIndex
    {
        
        public List<long> GetRecordPositions(DateTime start_range, DateTime finish_range,Dictionary<int, int> source_ids, Dictionary<byte, byte> data_type_ids)
        {
            throw new NotImplementedException();
        }

        public void AddDataToIndex(long currentFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId)
        {
            throw new NotImplementedException();
        }

        public void FlushIndexToFile(string data_base_file)
        {
            throw new NotImplementedException();
        }
    }

    public interface ISearchProcessData
    {
        DateTime StartSearchRange { get; }
        DateTime FinishSearchRange { get; }
        IDictionary<int, int> SearchSourceIds { get; }
        IDictionary<int, int> TypeDataIds { get; }
        void Add(DateTime time, ushort sourceId, byte dataTypeId, byte[] data);
    }

    public class SearchProcessData : ISearchProcessData
    {
        public DateTime StartSearchRange { get;private set; }

        public DateTime FinishSearchRange { get; private set; }

        public List<DbDataRecord> Results { get; private set; }

        public IDictionary<int, int> SearchSourceIds { get; private set; }

        public IDictionary<int, int> TypeDataIds { get; private set; }

        public void Add(DateTime time, ushort sourceId, byte dataTypeId, byte[] data)
        {
            throw new NotImplementedException();
        }

        public SearchProcessData(DateTime start_range, DateTime finish_range, List<int> source_ids, List<byte> data_type_ids,long maximum_result_size)
        {
            Results=new List<DbDataRecord>();
        }
        
    }

    public abstract class FileStorageReaderBase :IDisposable
    {
        protected long _startingDataFilePosition;
        protected FileStream _fileStream;


        public string FileName { get; protected set; }

        public virtual void OpenStream()
        {
            if (_fileStream == null)
            {
                _fileStream = File.OpenRead(FileName);
            }
        }

        public virtual void CloseStream()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }
        }

        public DataItem GetDbDataRecord(long record_potion)
        {
            if (record_potion < 0)
            {
                throw new InvalidDataException(String.Format("record_potion[{0}] < 0", record_potion));
            }
            if (record_potion >= _fileStream.Length)
            {
                throw new InvalidDataException(String.Format("record_potion[{0}] >=_fileStream.Length[{1}]", record_potion, _fileStream.Length));
            }

            _fileStream.Position = record_potion;
            if (_fileStream.ReadByte() != Byte.MaxValue)
            {
                throw new Exception(String.Format("In position [{0}] not started record", record_potion));
            }
            DataItem ret = new DataItem();
            byte[] buffer = new byte[8];

            _fileStream.Write(buffer, 0, 4);
            int size = BitConverter.ToInt32(buffer, 0);//размер пакета

            _fileStream.Write(buffer, 0, 8); //считываем дату записи
            long date_time_binary = BitConverter.ToInt64(buffer, 0);
            ret.Time = DateTime.FromBinary(date_time_binary);

            _fileStream.Write(buffer, 0, 2); //считываем идентификатор источника
            ret.SourceId = BitConverter.ToUInt16(buffer, 0);

            _fileStream.Write(buffer, 0, 1);
            ret.DataTypeId = buffer[0];

            ret.Data = new byte[size - 1 - 2 - 8 - 4];
            _fileStream.Write(ret.Data, 0, ret.Data.Length);
            return ret;
        }

        public void ScanFileAndFillIndex(IFileWritingIndex index)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));

            OpenStream();
            try
            {
                _fileStream.Position = _startingDataFilePosition;
                byte[] buffer = new byte[8];
                while (_fileStream.Position<_fileStream.Length)
                {
                    long current_file_position=_fileStream.Position;
                    if (_fileStream.ReadByte() != Byte.MaxValue)
                    {
                        throw new InvalidDataException("Error file format");
                    }
                    
                    _fileStream.Write(buffer, 0, 4);
                    int size = BitConverter.ToInt32(buffer, 0);//размер пакета

                    _fileStream.Write(buffer, 0, 8); //считываем дату записи
                    long date_time_binary = BitConverter.ToInt64(buffer, 0);
                    var time = DateTime.FromBinary(date_time_binary);

                    _fileStream.Write(buffer, 0, 2); //считываем идентификатор источника
                    var sourceId = BitConverter.ToUInt16(buffer, 0);

                    _fileStream.Write(buffer, 0, 1);
                    var dataTypeId = buffer[0];
                    
                    index.AddDataToIndex(current_file_position,time,sourceId,dataTypeId);

                    int data_size = size - 1 - 2 - 8 - 4;
                    _fileStream.Seek(data_size,SeekOrigin.Current);
                }
            }
            finally
            {
                CloseStream();
            }
        }

        public void Dispose()
        {
            
            CloseStream();
        }
    }

    public class FileStorageReader : FileStorageReaderBase
    {

        public FileStorageReader(string file_name)
        {
            FileName = file_name;
            if (string.IsNullOrWhiteSpace(file_name)) throw new ArgumentNullException(nameof(file_name));
            if (File.Exists(file_name)==false)
            {
                throw new FileNotFoundException(file_name);
            }

            using (FileStream file=File.OpenRead(file_name))
            {
                byte[] file_prefix = DirectoryStorage.GetFileDBPrefix();

                //byte[] d=new byte[8];
                //int readed = file.Read(d, 0, 8);

                for (int i = 0; i < file_prefix.Length; i++)
                {
                    int current_byte = file.ReadByte();
                    if (file_prefix[i] != current_byte)
                    {
                        throw new Exception("Not current database file");
                    }
                }
                _startingDataFilePosition = file.Position;
            }

        }

    }
    

    public class CurrentFileStorage : FileStorageReaderBase
    {
        private readonly ITimeSerivice _timeSerivice;
        private readonly int _maximumFileSizeInBytes;
        private readonly IFileWritingIndex _fileIndex;
        
        
        public long FileSize { get { return _fileStream.Length; } }

        public CurrentFileStorage(string directory, ITimeSerivice timeSerivice, int maximumFileSizeInBytes,IFileWritingIndex fileIndex)
        {
            _timeSerivice = timeSerivice;
            _maximumFileSizeInBytes = maximumFileSizeInBytes;
            _fileIndex = fileIndex;
            
            FileName = Path.Combine(directory, DirectoryStorage.GetFileNameByTime(timeSerivice.UTCNow));

            _fileStream = File.Create(FileName);

            //записываем префикс идентификатор базы данных
            byte[] filePrefix = DirectoryStorage.GetFileDBPrefix();
            _fileStream.Write(filePrefix, 0, filePrefix.Length);
            _startingDataFilePosition = _fileStream.Position;
            _fileStream.Flush();
        }

        public bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data)
        {
            if (_fileStream.Length + data.Length > _maximumFileSizeInBytes)
            {
                return false;
            }

            long current_file_position = _fileStream.Position;

            int record_size =  4/*размер записи*/+ 8 /*время записи*/ + 4 /*идентификатор источнка*/+ 1 /*тип данных*/+ data.Length;
            
            
            _fileStream.WriteByte(Byte.MaxValue);

            byte[] buffer = BitConverter.GetBytes(record_size);
            _fileStream.Write(buffer, 0, buffer.Length);//записываем размер пакета

            var dateTime = _timeSerivice.UTCNow;
            buffer = BitConverter.GetBytes(dateTime.ToBinary());
            _fileStream.Write(buffer,0,buffer.Length);//записываем дату записи
            
            buffer = BitConverter.GetBytes(sourceId);
            _fileStream.Write(buffer, 0, buffer.Length);//записываем идентификатор источника
            
            _fileStream.WriteByte(dataTypeId); //записываем тип данных
            
            _fileStream.Write(data,0,data.Length); //записываем данные
            _fileStream.Flush();
            _fileIndex.AddDataToIndex(current_file_position, dateTime, sourceId, dataTypeId);
            return true;
        }

        public override void CloseStream()
        {
        }
    }
}
