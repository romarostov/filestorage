namespace FileStorage
{
    /// <summary>
    /// ������� ������ � �������
    /// </summary>
    public interface IFileStorageFactory
    {
        /// <summary>
        /// ������� ����������� �����
        /// </summary>
        IFileStorageReader GetFileStorageReader(string fileName);

        /// <summary>
        /// ������� ����� ����
        /// </summary>
        IFileStorageWriter CreaNewFileStorage(string directory);
    }
}