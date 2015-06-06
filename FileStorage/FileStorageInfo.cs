namespace FileStorage
{
    /// <summary>
    /// Информация о файле
    /// </summary>
    public class FileStorageInfo
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Размер файла в файтах
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Количество записей
        /// </summary>
        public int CountRecords { get; set; }

        /// <summary>
        /// Является ли файл текущим
        /// </summary>
        public bool IsNewFile { get; set; }

        /// <summary>
        /// Сохраненный временной диапазон в файле
        /// </summary>
        public DateTimeRange SavedTimeRangeUTC { get; set; }

    }
}