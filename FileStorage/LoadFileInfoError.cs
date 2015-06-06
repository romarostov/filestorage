namespace FileStorage
{
    /// <summary>
    /// Ошибочный файл в базе данных
    /// </summary>
    public class LoadFileInfoError
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        /// Ошибка
        /// </summary>
        public string Error { get; set; }
    }
}