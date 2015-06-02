using Storage.Interfaces;
using System;
using System.Collections.Generic;
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
                    _currentFileStorage = new CurrentFileStorage(_directory, _timeSerivice, _maximumBytesInFile);
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

                    var oldFileStorage = _currentFileStorage;
                    _currentFileStorage = null;

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            //записываем индекс в файл
                            oldFileStorage.CreateFileIndexAndGetInfo();
                        }
                        catch (Exception)
                        {
                            
                        }
                    });

                    var newCurrentFile = new CurrentFileStorage(_directory, _timeSerivice, _maximumBytesInFile);
                    newCurrentFile.WriteRecord(source_id, data_type_id, data);

                    _currentFileStorage = newCurrentFile;
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
        
        void AddDataToIndex(long currentFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId, int dataLength);

        void FlushIndexToFile(string data_base_file);
    }

    public class FileWritingIndex : IFileWritingIndex
    {
        public void AddDataToIndex(long currentFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId, int dataLength)
        {
            throw new NotImplementedException();
        }

        public void FlushIndexToFile(string data_base_file)
        {
            throw new NotImplementedException();
        }
    }

    public class CurrentFileStorage : IDisposable
    {
        private readonly ITimeSerivice _timeSerivice;
        private readonly int _maximumFileSize;
        private readonly IFileWritingIndex _fileIndex;
        private readonly FileStream _fileStream;
        private readonly string _fullFile;

        public CurrentFileStorage(string directory, ITimeSerivice timeSerivice, int maximumFileSize):this(directory,timeSerivice,maximumFileSize,new FileWritingIndex())
        {
            
        }

        public CurrentFileStorage(string directory, ITimeSerivice timeSerivice, int maximumFileSize,IFileWritingIndex fileIndex)
        {
            _timeSerivice = timeSerivice;
            _maximumFileSize = maximumFileSize;
            _fileIndex = fileIndex;
            
            _fullFile = Path.Combine(directory, DirectoryStorage.GetFileNameByTime(timeSerivice.UTCNow));

            _fileStream = File.Create(_fullFile);

            //записываем префикс идентификатор базы данных
            byte[] file_prefix = DirectoryStorage.GetFileDBPrefix();
            _fileStream.Write(file_prefix, 0, file_prefix.Length);
        }



        public void Dispose()
        {
            
            _fileStream.Close();
        }

        ~CurrentFileStorage()
        {
            Dispose();
        }

        public bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data)
        {
            if (_fileStream.Length + data.Length > _maximumFileSize)
            {
                return false;
            }

            long currentFilePosition = _fileStream.Position;

            int record_size = 4/*размер записи*/+ 8 /*время записи*/ + 4 /*идентификатор источнка*/+ 1 /*тип данных*/+ data.Length;
            
            byte[] buffer = BitConverter.GetBytes(record_size);
            _fileStream.Write(buffer, 0, buffer.Length);//записываем размер пакета

            var dateTime = _timeSerivice.UTCNow;
            buffer = BitConverter.GetBytes(dateTime.ToBinary());
            _fileStream.Write(buffer,0,buffer.Length);//записываем дату записи
            
            buffer = BitConverter.GetBytes(sourceId);
            _fileStream.Write(buffer, 0, buffer.Length);//записываем идентификатор источника
            
            _fileStream.WriteByte(dataTypeId); //записываем тип данных
            
            _fileStream.Write(data,0,data.Length); //записываем данные

            _fileIndex.AddDataToIndex(currentFilePosition, dateTime, sourceId, dataTypeId, data.Length);
            return true;
        }

        public void CreateFileIndexAndGetInfo()
        {
            _fileIndex.FlushIndexToFile(_fullFile);
        }
    }
}
