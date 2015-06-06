using System;
using System.Collections.Generic;

namespace Storage.Interfaces
{
    /// <summary>
    /// Интерфейс хранилища данных
    /// </summary>
    public interface IDataItemStore:IDisposable
    {
        /// <summary>
        /// Возвращает данные из хранилища
        /// </summary>
        /// <param name="startRange">начало запрашиваемого периода (UTC)</param>
        /// <param name="finishRange">конец запрашиваемого периода (UTC)</param>
        /// <param name="sourceIds">список идентификатор источников (опционально)</param>
        /// <param name="dataTypeIds">список типов данных (опционально)</param>
        List<RecordDataItem> GetData(DateTime startRange, DateTime finishRange, List<ushort> sourceIds, List<byte> dataTypeIds);

        /// <summary>
        /// Сохранить данные
        /// </summary>
        /// <param name="sourceId">идентификатор источника</param>
        /// <param name="dataTypeId">тип записываемых данных</param>
        /// <param name="data">данные</param>
        void SaveData(UInt16 sourceId, byte dataTypeId, byte[] data);

    }
}