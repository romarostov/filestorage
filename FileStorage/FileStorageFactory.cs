using System;

namespace FileStorage
{
    public class FileStorageFactory : IFileStorageFactory
    {
        private readonly ITimeSerivice _timeService;
        private readonly long _maximumFileSizeInBytes;

        public FileStorageFactory(ITimeSerivice timeService, IDirectoryStorageConfiguration configuration)
        {
            if (configuration.MaximumMegabytesInFile <= 0)
            {
                throw new Exception(String.Format("configuration.MaximumMegabytesInFile[{0}] <= 0", configuration.MaximumMegabytesInFile));
            }
            _timeService = timeService;
            _maximumFileSizeInBytes = configuration.MaximumMegabytesInFile * 1024 * 1024;
        }

        public IFileStorageReader GetFileStorageReader(string fileName)
        {
            IFileStorageReaderAndWriter fileStorage = new FileStorageReaderAndWriter(fileName);
            return new FileStorageWithIndex(false, fileStorage);
        }

        public IFileStorageWriter CreaNewFileStorage(string directory)
        {
            IFileStorageReaderAndWriter fileStorage = new FileStorageReaderAndWriter(directory, _timeService, _maximumFileSizeInBytes);
            return new FileStorageWithIndex(true, fileStorage);
        }

    }
}