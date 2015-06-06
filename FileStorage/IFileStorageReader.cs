using System;

namespace FileStorage
{
    /// <summary>
    /// ����������� ������ �� �����
    /// </summary>
    public interface IFileStorageReader:IDisposable
    {
        /// <summary>
        /// ������ ���������� �������, ����������� ������
        /// </summary>
        DateTime StartRange { get; }
        /// <summary>
        /// ���������� ���������� �������, ����������� ������
        /// </summary>
        DateTime FinishRange { get; }
        /// <summary>
        /// ���������� ������
        /// </summary>
        void ProcessSearchRequest(ISearchRequestData request);

        /// <summary>
        /// ���������� ����� ���������� � �������
        /// </summary>
        FileStorageInfo GetWorkInfo();
    }
}