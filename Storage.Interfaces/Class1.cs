using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Interfaces
{
    public class DateTimeRange
    {
        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

    }

    public interface IDataItemStore
    {

        List<DataItem> GetData(DateTime start_range, DateTime finish_range, List<int> source_ids, List<byte> data_type_ids);

        void SaveData(UInt16 source_id, byte data_type_id, byte[] data);

    }

    public class DataItem
    {
        public DateTime Time { get; set; }

        public ushort SourceId { get; set; }

        public byte DataTypeId { get; set; }

        public byte[] Data { get; set; }
    }


}
