using System;

namespace FileStorage
{
    /// <summary>
    /// ��������� ������� �����
    /// </summary>
    public interface IFileStorageIndex
    {
        /// <summary>
        /// �������� ���������� � ����� ������ � �����
        /// </summary>
        void AddDataToIndex(long recordFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId);
    }
}