namespace FileStorage
{
    /// <summary>
    /// ������ ������ � ������� ����
    /// </summary>
    public interface IFileStorageWriter : IFileStorageReader
    {
        /// <summary>
        /// ������ ������
        /// </summary>
        bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data);

        /// <summary>
        /// ������� ���� � �������� ������ ������� � ����
        /// </summary>
        void StopWritingDataToFile();

        /// <summary>
        /// ��� �����
        /// </summary>
        string FileName { get; }
    }
}