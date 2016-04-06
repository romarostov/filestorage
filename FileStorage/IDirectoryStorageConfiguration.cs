namespace FileStorage
{
    /// <summary>
    /// ������������ ���������
    /// </summary>
    public interface IDirectoryStorageConfiguration
    {
        /// <summary>
        /// ������������ ������ ������ ����� 
        /// </summary>
        long MaximumMegabytesInFile { get; }

        /// <summary>
        /// ����������� ������ ������ � ������
        /// </summary>
        long MinimumRecordDataSizeInBytes { get; }

        /// <summary>
        /// ������������ ������ ������ � ������
        /// </summary>
        long MaximumRecordDataSizeInKilobytes { get; }

        /// <summary>
        /// ������������ ������ ������ � ������ �� ������.
        /// ������ �� �������� �������
        /// </summary>
        long MaximumResultDataSizeInMegabytes { get;  }
    }
}