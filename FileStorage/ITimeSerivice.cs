using System;

namespace FileStorage
{
    /// <summary>
    /// Сервис текущего времени (Используется для тестов)
    /// </summary>
    public interface ITimeSerivice
    {
        /// <summary>
        /// Текущая дата в UTC формате
        /// </summary>
        DateTime UTCNow { get; }
    }
}