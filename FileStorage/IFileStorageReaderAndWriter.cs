using Storage.Interfaces;

namespace FileStorage
{
    /// <summary>
    /// Работа с файлом базы данных непосредтвенно
    /// </summary>
    public interface IFileStorageReaderAndWriter
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Размера файла
        /// </summary>
        long FileSize { get;  }

        /// <summary>
        /// Открыть поток для чтения или записи
        /// </summary>
        void OpenStream();

        /// <summary>
        /// Закрыть поток чтения или записи
        /// </summary>
        void CloseStream();

        /// <summary>
        /// Возвращает запись по ее позиции в файле
        /// </summary>
        RecordDataItem GetDbDataRecord(long recordPotion);

        void ScanFileAndFillIndex(IFileStorageIndex index);

        bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data, IFileStorageIndex index);

        void Dispose();
    }
}