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
        int MaximumMegabytesInFile { get; }

        /// <summary>
        /// ����������� ������ ������ � ������
        /// </summary>
        int MinimumRecordDataSizeInBytes { get; }

        /// <summary>
        /// ������������ ������ ������ � ������
        /// </summary>
        int MaximumRecordDataSizeInKilobytes { get; }

        /// <summary>
        /// ������������ ������ ������ � ������ �� ������.
        /// ������ �� �������� �������
        /// </summary>
        int MaximumResultDataSizeInMegabytes { get;  }
    }
}