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
        long MaximumMegabytesInFile { get; }

        /// <summary>
        /// Минимальный размер данных в записи
        /// </summary>
        long MinimumRecordDataSizeInBytes { get; }

        /// <summary>
        /// Максимальный размер данных в записи
        /// </summary>
        long MaximumRecordDataSizeInKilobytes { get; }

        /// <summary>
        /// Максимальный размер данных в ответе на запрос.
        /// Защита от большого запроса
        /// </summary>
        long MaximumResultDataSizeInMegabytes { get;  }
    }
}