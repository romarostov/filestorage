using System;

namespace FileStorage
{
    /// <summary>
    /// ������ �������� ������� (������������ ��� ������)
    /// </summary>
    public interface ITimeSerivice
    {
        /// <summary>
        /// ������� ���� � UTC �������
        /// </summary>
        DateTime UTCNow { get; }
    }
}