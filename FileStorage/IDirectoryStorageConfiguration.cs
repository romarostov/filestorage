namespace FileStorage
{
    /// <summary>
    /// Конфигурация хранилища
    /// </summary>
    public interface IDirectoryStorageConfiguration
    {
        /// <summary>
        /// Максимальный размер одного файла 
        /// </summary>
        int MaximumMegabytesInFile { get; }

        /// <summary>
        /// Минимальный размер данных в записи
        /// </summary>
        int MinimumRecordDataSizeInBytes { get; }

        /// <summary>
        /// Максимальный размер данных в записи
        /// </summary>
        int MaximumRecordDataSizeInKilobytes { get; }

        /// <summary>
        /// Максимальный размер данных в ответе на запрос.
        /// Защита от большого запроса
        /// </summary>
        int MaximumResultDataSizeInMegabytes { get;  }
    }
}