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

        public float SizeInMegaBytes { get; set; }

        public int CountRecords { get; set; }

        public bool IsCurrent { get; set; }

        public DateTimeRange SavedTimeRangeUTC { get; set; }

    }

    public interface ITimeSerivice
    {
        DateTime UTCNow { get; }
    }

    class TimeSerivice : ITimeSerivice
    {
        public DateTime GetUTCTime()
        {
            return DateTime.UtcNow;
        }
    }

    public class DirectoryStorage : IDataItemStore, IDisposable
    {
        public DirectoryStorage(string directory, IDirectoryStorageConfiguration configuration):this(directory,configuration,new TimeSerivice())
        {
            
        }

        public DirectoryStorage(string directory,IDirectoryStorageConfiguration configuration, ITimeSerivice time_serivice)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException("directory");
            if(Directory.Exists(directory)==false)throw new DirectoryNotFoundException(directory);
            if (configuration.MinimumRecordDataSizeInBytes < 1)
            {
                throw new InvalidDataException(string.Format("configuration.MinimumRecordDataSizeInBytes[{0}] < 1", configuration.MinimumRecordDataSizeInBytes));
            }
            if (configuration.MinimumRecordDataSizeInBytes*1014 > configuration.MaximumRecordDataSizeInKilobytes)
            {
                throw new InvalidDataException(String.Format("configuration.MinimumRecordDataSizeInBytes*1014 [{0}]> configuration.MaximumRecordDataSizeInKilobytes[{1}]", configuration.MinimumRecordDataSizeInBytes * 1014 , configuration.MaximumRecordDataSizeInKilobytes));
            }
            if (configuration.MaximumRecordDataSizeInKilobytes*5 > configuration.MaximumMegabytesInFile*1024)//проверяем что можно в файл записать хотя бы 5 максимальных записей
            {
                throw new InvalidDataException(string.Format("configuration.MaximumRecordDataSizeInKilobytes[{0}]*5 > configuration.MaximumMegabytesInFile[{1}]*1024", configuration.MaximumRecordDataSizeInKilobytes ,configuration.MaximumMegabytesInFile ));
            }

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
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public static string GetFileNameByTime(DateTime createtion_time)
        {
            throw new NotImplementedException();
        }
    }
}
