using System;

namespace FileStorage
{
    /// <summary>
    /// Считыватель данных из файла
    /// </summary>
    public interface IFileStorageReader:IDisposable
    {
        /// <summary>
        /// Начала временного периода, сохраненных данных
        /// </summary>
        DateTime StartRange { get; }
        /// <summary>
        /// Завершение временного периода, сохраненных данных
        /// </summary>
        DateTime FinishRange { get; }
        /// <summary>
        /// Обработать запрос
        /// </summary>
        void ProcessSearchRequest(ISearchRequestData request);

        /// <summary>
        /// Возвращает общую информацию о запроса
        /// </summary>
        FileStorageInfo GetWorkInfo();
    }
}