namespace FileStorage
{
    /// <summary>
    /// Фабрика работы с файлами
    /// </summary>
    public interface IFileStorageFactory
    {
        /// <summary>
        /// Создать считыватель файла
        /// </summary>
        IFileStorageReader GetFileStorageReader(string fileName);

        /// <summary>
        /// Создать новый файл
        /// </summary>
        IFileStorageWriter CreaNewFileStorage(string directory);
    }
}