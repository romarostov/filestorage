using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using Storage.Interfaces;

namespace FileStorage
{
    /// <summary>
    /// Общая логика работы с хранилищем
    /// </summary>
    public class DirectoryStorage : DisposableObject, IDataItemStore
    {
        private readonly string _directory;
        private readonly IFileStorageFactory _fileStorageFactory;
        private readonly ITimeSerivice _timeSerivice;

        private readonly int _minimumRecordDataSizeInBytes;
        private readonly int _maximumRecordDataSizeInBytes;

        private readonly object _writeSyncObject = new object();
        IFileStorageWriter _currentFileStorage;

        public List<LoadFileInfoError> FileWithErrors { get; private set; }

        private readonly SortedDictionary<DateTime, IFileStorageReader> _storageItems = new SortedDictionary<DateTime, IFileStorageReader>();

        private readonly long _maximumResultDataSize;
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
            if (configuration.MinimumRecordDataSizeInBytes > configuration.MaximumRecordDataSizeInKilobytes * 1014)
            {
                throw new InvalidDataException(String.Format("configuration.MinimumRecordDataSizeInBytes [{0}]> configuration.MaximumRecordDataSizeInKilobytes*1014[{1}]", configuration.MinimumRecordDataSizeInBytes * 1014, configuration.MaximumRecordDataSizeInKilobytes));
            }
            if (configuration.MaximumResultDataSizeInMegabytes < configuration.MinimumRecordDataSizeInBytes)
            {
                throw new InvalidDataException(String.Format("configuration.MaximumResultDataSizeInMegabytes[{0}] < configuration.MinimumRecordDataSizeInBytes[{1}}", configuration.MaximumResultDataSizeInMegabytes, configuration.MinimumRecordDataSizeInBytes));
            }

            _minimumRecordDataSizeInBytes = configuration.MinimumRecordDataSizeInBytes;
            _maximumRecordDataSizeInBytes = configuration.MaximumRecordDataSizeInKilobytes * 1024;
            _maximumResultDataSize = configuration.MaximumResultDataSizeInMegabytes * 1024 * 1024;

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

        public List<RecordDataItem> GetData(DateTime startRange, DateTime finishRange, List<ushort> sourceIds, List<byte> dataTypeIds)
        {
            if (_timeSerivice.UTCNow <= startRange)
            {
                throw new InvalidDataException(String.Format("_timeSerivice.UTCNow[{0}] <= start_range[{1}]", _timeSerivice.UTCNow, startRange));
            }
            SearchRequestData request = new SearchRequestData(startRange, finishRange, sourceIds, dataTypeIds, _maximumResultDataSize);

            lock (_readIndexesSyncObject)
            {
                foreach (KeyValuePair<DateTime, IFileStorageReader> fileStorageIndex in _storageItems)
                {
                    if (fileStorageIndex.Key >= startRange)
                    {
                        if (fileStorageIndex.Key > finishRange) break;
                        fileStorageIndex.Value.ProcessSearchRequest(request);
                    }
                }
            }
            return request.Results;
        }

        public void SaveData(UInt16 sourceId, byte dataTypeId, byte[] data)
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
            if (IsDisposed) throw new InvalidOperationException();

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
                    is_write = writer.WriteRecord(sourceId, dataTypeId, data);
                }
                catch (Exception exception)
                {
                    //ошибка записи данных в файл, чтобы избежать ошибки в следствии частичной записи, закрываем этот файл и начинаем новый
                    LogManager.GetCurrentClassLogger().ErrorException("WriteRecord", exception);
                    _stopCurrentFile();
                    throw;
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
                else if (is_write == false)//False - файл переполнен, записываем в новый
                {
                    _stopCurrentFile();

                    //создаем новый файл и записываем туда данные

                    var newCurentFile = _fileStorageFactory.CreaNewFileStorage(_directory);
                    newCurentFile.WriteRecord(sourceId, dataTypeId, data);

                    _currentFileStorage = newCurentFile;
                    lock (_readIndexesSyncObject)
                    {
                        _storageItems.Add(_currentFileStorage.StartRange, _currentFileStorage);
                    }

                }
            }
        }

        private void _stopCurrentFile()
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
                    FileWithErrors.Add(new LoadFileInfoError()
                    {
                        FileName = _currentFileStorage.FileName,
                        Error = exception.Message
                    });
            }
            finally
            {
                _currentFileStorage = null;
            }
        }

        protected override void OnDisposed()
        {
            lock (_readIndexesSyncObject)
            {
                foreach (IFileStorageReader item in _storageItems.Values)
                {
                    item.Dispose();
                }
                _storageItems.Clear();
            }

        }

    }
}