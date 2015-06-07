using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Interfaces
{
    
    /// <summary>
    /// Запись в базе данных
    /// </summary>
    public class RecordDataItem
    {
        /// <summary>
        /// Время записи в UTC
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Идентификатор источника данных
        /// </summary>
        public ushort SourceId { get; set; }

        /// <summary>
        /// Тип данных
        /// </summary>
        public byte DataTypeId { get; set; }

        /// <summary>
        /// Данные
        /// </summary>
        public byte[] Data { get; set; }
    }


}
