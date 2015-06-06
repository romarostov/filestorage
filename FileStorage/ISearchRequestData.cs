using System;
using System.Collections.Generic;
using Storage.Interfaces;

namespace FileStorage
{
    /// <summary>
    /// Запрос
    /// </summary>
    public interface ISearchRequestData
    {
        /// <summary>
        /// Начало запрашиваемого временного периода
        /// </summary>
        DateTime StartSearchRange { get; }

        /// <summary>
        /// Завершение запрашиваемого временного периода
        /// </summary>
        DateTime FinishSearchRange { get; }

        /// <summary>
        /// Список идентификаторов источников данных
        /// </summary>
        IDictionary<ushort, byte> SearchSourceIds { get; }

        /// <summary>
        /// Список запрашиваемых типов данных
        /// </summary>
        IDictionary<byte, byte> TypeDataIds { get; }

        /// <summary>
        /// Добавить запись к результату
        /// </summary>
        /// <param name="record"></param>
        void Add(RecordDataItem record);
    }
}