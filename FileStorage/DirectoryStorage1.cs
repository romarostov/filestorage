using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Storage.Interfaces;

namespace FileStorage
{

    public class LoadFileInfoError
    {
        public string FileName { get; set; }
        public string Error { get; set; }
    }



    public class DirectoryStorage : IDataItemStore, IDisposable
    {
        private readonly string _directory;
        private readonly IFileStorageFactory _fileStorageFactory;
        private readonly ITimeSerivice _timeSerivice;

        private readonly int _minimumRecordDataSizeInBytes;
        private readonly int _maximumRecordDataSizeInBytes;

        private readonly object _writeSyncObject = new object();
        IFileStorageWriter _currentFileStorage;


        public List<LoadFileInfoError> FileWithErrors { get; private set; }

        private SortedDictionary<DateTime, IFileStorageReader> _storageItems = new SortedDictionary<DateTime, IFileStorageReader>();

        private long _mazimumResultDataSize; 
        private readonly object _readIndexesSyncObject = new object();
        



        public DirectoryStorage(string directory, IDirectoryStorageConfiguration configuration, IFileStorageFactory fileStorageFactory)
            : this(directory, configuration, fileStorageFactory, new TimeSerivice())
        {

        }

        public DirectoryStorage(string directory, IDirectoryStorageConfiguration configuration, IFileStorageFactory fileStorageFactory, ITimeSerivice timeSerivice)
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
            _minimumRecordDataSizeInBytes = configuration.MinimumRecordDataSizeInBytes;
            _maximumRecordDataSizeInBytes = configuration.MaximumRecordDataSizeInKilobytes * 1024;

            _directory = directory;
            _fileStorageFactory = fileStorageFactory;
            _timeSerivice = timeSerivice;

            //список с файлами, которые по разным причинам не прошли валидацию
            FileWithErrors = new List<LoadFileInfoError>();


            //сканируем папку на наличие файлов с данными
            var files = Directory.GetFiles(_directory, "*.dat");
            foreach (string file in files)
            {
                try
                {
                    var fileStorage = _fileStorageFactory.GetFileStorageReader(file);
                    _storageItems.Add(fileStorage.StartRange, fileStorage);
                }
                catch (Exception exception)
                {
                    FileWithErrors.Add(new LoadFileInfoError() { FileName = file, Error = exception.Message });
                }
            }
        }

        public List<FileStorageInfo> GetFilesInfos()
        {
            List<FileStorageInfo> ret = new List<FileStorageInfo>();
            lock (_readIndexesSyncObject)
            {
                foreach (var fileStorageIndex in _storageItems.Values)
                {
                    ret.Add(fileStorageIndex.GetWorkInfo());
                }
            }
            return ret;
        }
        
        public List<DataItem> GetData(DateTime start_range, DateTime finish_range, List<int> source_ids, List<byte> data_type_ids)
        {
            if (_timeSerivice.UTCNow <= start_range)
            {
                throw new InvalidDataException(String.Format("_timeSerivice.UTCNow[{0}] <= start_range[{1}]", _timeSerivice.UTCNow, start_range));
            }
            SearchProcessData request = new SearchProcessData(start_range, finish_range, source_ids, data_type_ids, _mazimumResultDataSize);

            lock (_readIndexesSyncObject)
            {
                foreach (KeyValuePair<DateTime, IFileStorageReader> fileStorageIndex in _storageItems)
                {
                    if (fileStorageIndex.Key >= start_range)
                    {
                        if (fileStorageIndex.Key < finish_range) break;
                        fileStorageIndex.Value.ProcessSearchRequest(request);
                    }
                }
            }
            return request.Results;
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
                var writer = _currentFileStorage;
                if (_currentFileStorage == null)
                {
                    writer = _fileStorageFactory.CreaNewFileStorage(_directory);
                }

                bool? is_write = null;

                try
                {
                    is_write = writer.WriteRecord(source_id, data_type_id, data);
                }
                catch (Exception)
                {
                    //ошибка записи данных в файл, чтобы избежать ошибки в следствии частичной записи, закрываем этот файл и начинаем новый
                }

                if (is_write == true)
                {
                    if (_currentFileStorage == null) //создали новый файл, добавляем его в общий список
                    {
                        _currentFileStorage = writer;
                        lock (_readIndexesSyncObject)
                        {
                            _storageItems.Add(_currentFileStorage.StartRange, _currentFileStorage);
                        }
                    }
                }
                else //False - файл переполнен, записываем в новый
                {
                    
                    try
                    {
                        if (_currentFileStorage != null)
                        {
                            _currentFileStorage.StopWritingDataToFile();
                        }
                    }
                    catch (Exception exception)
                    {
                        if (_currentFileStorage != null)
                            FileWithErrors.Add(new LoadFileInfoError() { FileName = _currentFileStorage.FileName, Error = exception.Message });
                    }

                    _currentFileStorage = null;

                    //создаем новый файл и записываем туда данные

                    var newCurentFile = _fileStorageFactory.CreaNewFileStorage(_directory);
                    newCurentFile.WriteRecord(source_id, data_type_id, data);

                    _currentFileStorage = newCurentFile;
                    lock (_readIndexesSyncObject)
                    {
                        _storageItems.Add(_currentFileStorage.StartRange, _currentFileStorage);
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_writeSyncObject)
            {
                if (_currentFileStorage != null)
                {
                    _currentFileStorage.StopWritingDataToFile();
                }
                _currentFileStorage = null;
            }
        }

        ///// <summary>
        ///// Возвращает имя файла с данными по дате создания
        ///// </summary>
        //public static string GetFileNameByTime(DateTime createtionTime)
        //{
        //    return String.Format("{0}{1:N2}{2:N2}{3:N2}{4:N2}{5:N2}{6}DB.dat",
        //        createtionTime.Year,
        //        createtionTime.Month,
        //        createtionTime.Day,
        //        createtionTime.Hour,
        //        createtionTime.Minute,
        //        createtionTime.Second, createtionTime.Millisecond);
        //}

        ///// <summary>
        ///// "Уникальный" префик к файлам базы данных.
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public static byte[] GetFileDBPrefix()
        //{
        //    return new byte[] { 20, 30, 2, 1, 4, 6, 3, 1 };
        //}
    }
}