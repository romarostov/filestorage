using System;
using System.Collections.Generic;
using System.IO;
using Storage.Interfaces;

namespace FileStorage
{
    public class SearchRequestData : ISearchRequestData
    {
        private readonly long _maximumResultSize;
        private long _currentResultDataSize;
        public DateTime StartSearchRange { get; private set; }

        public DateTime FinishSearchRange { get; private set; }

        public List<RecordDataItem> Results { get; private set; }

        public IDictionary<ushort, byte> SearchSourceIds { get; private set; }

        public IDictionary<byte, byte> TypeDataIds { get; private set; }

        public void Add(RecordDataItem record)
        {
            if (record == null) throw new ArgumentNullException("record");
            if (record.Time < StartSearchRange || record.Time > FinishSearchRange)
            {
                throw new InvalidOperationException(String.Format("record.Time[{0}] < StartSearchRange[{1}] || record.Time > FinishSearchRange[{2}]", record.Time , StartSearchRange , FinishSearchRange));
            }
            if (record.Data == null || record.Data.Length == 0)
            {
                throw new InvalidOperationException(String.Format("Try add new record with empty result data SourceId[{0}] TypeId[{1}] Time:[{2}]", record.SourceId, record.DataTypeId, record.Time));
            }
            _currentResultDataSize += record.Data.Length;
            if (_currentResultDataSize > _maximumResultSize)
            {
                throw new InvalidDataException("Exceeds the maximum size of data returned");
            }

            Results.Add(record);
        }

        public SearchRequestData(DateTime start_range, DateTime finish_range, List<ushort> source_ids, List<byte> data_type_ids, long maximumResultSize)
        {
            if (maximumResultSize <= 0)
            {
                throw new InvalidDataException(String.Format("maximumResultSize[{0}] <= 0", maximumResultSize));
            }
            if (finish_range < start_range)
            {
                throw new InvalidDataException(String.Format("finish_range[{0}] < start_range[{1}]", finish_range , start_range));
            }
            StartSearchRange = start_range;
            FinishSearchRange = finish_range;
            _maximumResultSize = maximumResultSize;
            SearchSourceIds=new Dictionary<ushort, byte>();
            if (source_ids != null)
            {
                foreach (var sourceId in source_ids)
                {
                    SearchSourceIds.Add(sourceId,0);
                }
            }
            
            TypeDataIds=new Dictionary<byte, byte>();
            if (data_type_ids != null)
            {
                foreach (var type_id in data_type_ids)
                {
                    TypeDataIds.Add(type_id, type_id);
                }
            }

            Results = new List<RecordDataItem>();
        }

    }
}