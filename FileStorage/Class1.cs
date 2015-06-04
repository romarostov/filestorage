using Storage.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

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



    public interface IFileWritingIndex 
    {
        void AddDataToIndex(long currentFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId);
    }


    public class FileWritingIndex : IFileWritingIndex, IFileStorage
    {

        public FileWritingIndex(IFileStorageReader fileStorageReader)//создаем индекс по уже существующиму файлу
        {

        }

        public FileWritingIndex()//создаем пустой индекс, для нового файла
        {
            
        }

        public void InitFile(IFileStorageReader fileStorageReader)
        {
            throw new NotImplementedException();
        }
        
        public List<long> GetRecordPositions(DateTime start_range, DateTime finish_range,Dictionary<int, int> source_ids, Dictionary<byte, byte> data_type_ids)
        {
            throw new NotImplementedException();
        }


        public void AddDataToIndex(long currentFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId)
        {
            throw new NotImplementedException();
        }

        public void FlushIndexToFile()
        {
            throw new NotImplementedException();
        }

        public DateTime StartRange { get; private set; }

        public DateTime FinishRange { get; private set; }

        public void ProcessSearchRequest(ISearchProcessData request)
        {
            throw new NotImplementedException();
        }

        public DirectoryStorageFileInfo GetWorkInfo()
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
        void Add(DbDataRecord record);
    }

    public class SearchProcessData : ISearchProcessData
    {
        public DateTime StartSearchRange { get;private set; }

        public DateTime FinishSearchRange { get; private set; }

        public List<DataItem> Results { get; private set; }

        public IDictionary<int, int> SearchSourceIds { get; private set; }

        public IDictionary<int, int> TypeDataIds { get; private set; }
        
        public void Add(DbDataRecord record)
        {
            throw new NotImplementedException();
        }
        
        public SearchProcessData(DateTime start_range, DateTime finish_range, List<int> source_ids, List<byte> data_type_ids,long maximum_result_size)
        {
            Results = new List<DataItem>();
        }
        
    }

    public interface IFileStorageReader
    {
        string FileName { get; }
        
        void OpenStream();
        
        void CloseStream();
        
        DataItem GetDbDataRecord(long record_potion);
        
        void ScanFileAndFillIndex(IFileWritingIndex index);

        void Dispose();
    }

    public abstract class FileStorageReaderBase :IDisposable, IFileStorageReader
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
            if (_fileStream == null)
            {
                throw new InvalidOperationException("First open stream");
            }
            if (record_potion < 0)
            {
                throw new InvalidDataException(String.Format("record_potion[{0}] < 0", record_potion));
            }
            if (record_potion >= _fileStream.Length)
            {
                throw new InvalidDataException(String.Format("record_potion[{0}] >=_fileStream.Length[{1}]", record_potion, _fileStream.Length));
            }

            if (_fileStream.Position != record_potion)
            {
                _fileStream.Position = record_potion;
            }
            if (_fileStream.ReadByte() != Byte.MaxValue)
            {
                throw new InvalidDataException(String.Format("In position [{0}] not started record", record_potion));
            }
            DataItem ret = new DataItem();
            byte[] buffer = new byte[8];

            _fileStream.Read(buffer, 0, 4);
            int size = BitConverter.ToInt32(buffer, 0);//размер пакета

            _fileStream.Read(buffer, 0, 8); //считываем дату записи
            long date_time_binary = BitConverter.ToInt64(buffer, 0);
            ret.Time = DateTime.FromBinary(date_time_binary);

            _fileStream.Read(buffer, 0, 2); //считываем идентификатор источника
            ret.SourceId = BitConverter.ToUInt16(buffer, 0);

            _fileStream.Read(buffer, 0, 1);
            ret.DataTypeId = buffer[0];

            ret.Data = new byte[size - 1 - 2 - 8 - 4];
            _fileStream.Read(ret.Data, 0, ret.Data.Length);
            return ret;
        }

        public void ScanFileAndFillIndex(IFileWritingIndex index)
        {
            if (index == null) throw new ArgumentNullException("index");

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
                    
                    _fileStream.Read(buffer, 0, 4);
                    int size = BitConverter.ToInt32(buffer, 0);//размер пакета

                    _fileStream.Read(buffer, 0, 8); //считываем дату записи
                    long date_time_binary = BitConverter.ToInt64(buffer, 0);
                    var time = DateTime.FromBinary(date_time_binary);

                    _fileStream.Read(buffer, 0, 2); //считываем идентификатор источника
                    var sourceId = BitConverter.ToUInt16(buffer, 0);

                    _fileStream.Read(buffer, 0, 1);
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
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }
        }
    }

    public class FileStorageReader : FileStorageReaderBase
    {

        public FileStorageReader(string file_name)
        {
            FileName = file_name;
            if (string.IsNullOrWhiteSpace(file_name)) throw new ArgumentNullException("file_name");
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
                        throw new InvalidDataException("Not current database file");
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

            if (_fileStream.Length != _fileStream.Position)
            {
                _fileStream.Seek(0, SeekOrigin.End);
            }

            long current_file_position = _fileStream.Position;

            int record_size =  4/*размер записи*/+ 8 /*время записи*/ + 2 /*идентификатор источнка*/+ 1 /*тип данных*/+ data.Length;
            
            
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
