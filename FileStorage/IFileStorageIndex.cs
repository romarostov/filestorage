using System;

namespace FileStorage
{
    /// <summary>
    /// Интерфейс индекса файла
    /// </summary>
    public interface IFileStorageIndex
    {
        /// <summary>
        /// Добавить информация о новой записи в файле
        /// </summary>
        void AddDataToIndex(long recordFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId);
    }
}