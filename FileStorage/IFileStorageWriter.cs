namespace FileStorage
{
    /// <summary>
    /// Запись данных в текущий файл
    /// </summary>
    public interface IFileStorageWriter : IFileStorageReader
    {
        /// <summary>
        /// Запись данных
        /// </summary>
        bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data);

        /// <summary>
        /// Закрыть файл и записать данные индекса в файл
        /// </summary>
        void StopWritingDataToFile();

        /// <summary>
        /// Имя файла
        /// </summary>
        string FileName { get; }
    }
}