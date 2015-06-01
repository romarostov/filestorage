using Storage.Interfaces;
using System;
using System.Collections.Generic;
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

        public DateTimeRange SavedTimeRange { get; set; }

    }

    public class DirectoryStorage : IDataItemStore, IDisposable
    {
        public DirectoryStorage(string directory,IDirectoryStorageConfiguration configuration)
        {

        }

        public List<DirectoryStorageFileInfo> GetFilesInfos()
        {
            throw new NotImplementedException();
        }

        public DateTimeRange SavedRange()
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
    }
}
