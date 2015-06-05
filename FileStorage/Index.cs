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


    public class FileStorageInfo
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
        void AddDataToIndex(long current_file_position, DateTime dateTime, ushort sourceId, byte dataTypeId);
    }

    public interface IFileStorageFactory
    {
        IFileStorageReader GetFileStorageReader(string file_name);

        IFileStorageWriter CreaNewFileStorage(string directory);
    }

    public class FileStorageFactory : IFileStorageFactory
    {
        private readonly ITimeSerivice _timeService;
        private readonly int _maximumFileSizeInBytes;

        public FileStorageFactory(ITimeSerivice time_service, IDirectoryStorageConfiguration configuration)
        {
            _timeService = time_service;
            _maximumFileSizeInBytes = configuration.MaximumMegabytesInFile*1024*1024;
        }

        public IFileStorageReader GetFileStorageReader(string file_name)
        {
            IFileStorageReaderAndWriter file_storage=new FileStorageReaderAndWriter(file_name);
            return new FileStorageWithIndex(false,file_storage);
        }

        public IFileStorageWriter CreaNewFileStorage(string directory)
        {
            IFileStorageReaderAndWriter file_storage = new FileStorageReaderAndWriter(directory,_timeService,_maximumFileSizeInBytes);
            return new FileStorageWithIndex(true,file_storage); 
        }

    }


    public interface IFileStorageReader
    {
        DateTime StartRange { get; }
        DateTime FinishRange { get; }
        void ProcessSearchRequest(ISearchProcessData request);
        FileStorageInfo GetWorkInfo();
    }


    public interface IFileStorageWriter: IFileStorageReader
    {
        bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data);

        void StopWritingDataToFile();
    }

    public class FileStorageWithIndex : IFileWritingIndex,  IFileStorageWriter
    {
        private readonly bool _isNewFile;
        private readonly IFileStorageReaderAndWriter _fileStorageReaderAndWriter;

        public FileStorageWithIndex(bool is_new_file, IFileStorageReaderAndWriter file_storage_reader_and_writer)//создаем индекс по уже существующиму файлу
        {
            _isNewFile = is_new_file;
            _fileStorageReaderAndWriter = file_storage_reader_and_writer;

            if (is_new_file == false)//открываем индекс уже существующего файла
            {

            }
            else //создаем пустой индекс, для нового файла
            {
                file_storage_reader_and_writer.OpenStream(); //открываем файл для записи
            }
        }


        public void AddDataToIndex(long current_file_position, DateTime dateTime, ushort sourceId, byte dataTypeId)
        {
            throw new NotImplementedException();
        }

        public void StopWritingDataToFile()
        {
            throw new NotImplementedException();
        }

        public DateTime StartRange { get; private set; }

        public DateTime FinishRange { get; private set; }

        public List<long> GetRecordPositions(ISearchProcessData request)
        {
            throw new NotImplementedException();
        }

        public void ProcessSearchRequest(ISearchProcessData request)
        {
            List<long> positions = GetRecordPositions(request);
            if (positions != null)
            {
                try
                {
                    if(_isNewFile==false) _fileStorageReaderAndWriter.OpenStream();

                    foreach (long position in positions)
                    {
                        request.Add(_fileStorageReaderAndWriter.GetDbDataRecord(position));
                    }
                }
                finally
                {
                    if (_isNewFile==false)
                    {
                        _fileStorageReaderAndWriter.CloseStream();
                    }
                }
            }
        }

        public bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data)
        {

            throw new NotImplementedException();
        }

        public FileStorageInfo GetWorkInfo()
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
        void Add(DataItem record);
    }

    public class SearchProcessData : ISearchProcessData
    {
        public DateTime StartSearchRange { get; private set; }

        public DateTime FinishSearchRange { get; private set; }

        public List<DataItem> Results { get; private set; }

        public IDictionary<int, int> SearchSourceIds { get; private set; }

        public IDictionary<int, int> TypeDataIds { get; private set; }

        public void Add(DataItem record)
        {
            throw new NotImplementedException();
        }

        public SearchProcessData(DateTime start_range, DateTime finish_range, List<int> source_ids, List<byte> data_type_ids, long maximum_result_size)
        {
            Results = new List<DataItem>();
        }

    }


}
