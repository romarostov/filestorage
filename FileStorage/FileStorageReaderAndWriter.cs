using System;
using System.IO;
using Storage.Interfaces;

namespace FileStorage
{
    public class FileStorageReaderAndWriter :DisposableObject, IFileStorageReaderAndWriter
    {

        private readonly ITimeSerivice _timeSerivice;
        private readonly long _maximumFileSizeInBytes;
        readonly long _startingDataFilePosition;
        FileStream _fileStream;


        /// <summary>
        /// Открываем существующий файл с данными
        /// </summary>
        /// <param name="fileName">полный путь к файлу</param>
        public FileStorageReaderAndWriter(string fileName)
        {
            FileName = fileName;
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException("fileName");
            if (File.Exists(fileName) == false)
            {
                throw new FileNotFoundException(fileName);
            }

            using (FileStream file = File.OpenRead(fileName))
            {
                byte[] file_prefix = GetFileDBPrefix();

                //byte[] d=new byte[8];
                //int readed = file.Read(d, 0, 8);

                for (int i = 0; i < file_prefix.Length; i++)
                {
                    int current_byte = file.ReadByte();
                    if (file_prefix[i] != current_byte)
                    {
                        throw new InvalidDataException("Not current database file");
                    }
                }
                _startingDataFilePosition = file.Position;
            }
        }

        /// <summary>
        /// Создаем новый файл
        /// </summary>
        /// <param name="directory">директория в котором надо добавить файл</param>
        /// <param name="timeSerivice">сервис времени</param>
        /// <param name="maximumFileSizeInBytes">максимальный размер файла</param>
        public FileStorageReaderAndWriter(string directory, ITimeSerivice timeSerivice, long maximumFileSizeInBytes)
        {
            if (maximumFileSizeInBytes < 1)
            {
                throw new InvalidDataException(String.Format("_maximumFileSizeInBytes[{0}] < 1", maximumFileSizeInBytes));
            }
            _timeSerivice = timeSerivice;
            _maximumFileSizeInBytes = maximumFileSizeInBytes;

            FileName = Path.Combine(directory, GetFileNameByTime(timeSerivice.UTCNow));

            _fileStream = File.Create(FileName);

            //записываем префикс идентификатор базы данных
            byte[] filePrefix = GetFileDBPrefix();
            _fileStream.Write(filePrefix, 0, filePrefix.Length);
            _startingDataFilePosition = _fileStream.Position;
            _fileStream.Flush();
        }

        public long FileSize
        {
            get
            {
                if (_fileStream != null)
                {
                    return _fileStream.Length;
                }
                return new FileInfo(FileName).Length;
            }
        }


        public string FileName { get; protected set; }


        public void OpenStream()
        {
            if (_fileStream == null)
            {
                _fileStream = File.OpenRead(FileName);
            }
        }

        public void CloseStream()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }
        }

        public RecordDataItem GetDbDataRecord(long recordPotion)
        {
            if (_fileStream == null)
            {
                throw new InvalidOperationException("First open stream");
            }
            if (recordPotion < 0)
            {
                throw new InvalidDataException(String.Format("record_potion[{0}] < 0", recordPotion));
            }
            if (recordPotion >= _fileStream.Length)
            {
                throw new InvalidDataException(String.Format("record_potion[{0}] >=_fileStream.Length[{1}]", recordPotion, _fileStream.Length));
            }

            if (_fileStream.Position != recordPotion)
            {
                _fileStream.Position = recordPotion;
            }
            if (_fileStream.ReadByte() != Byte.MaxValue)
            {
                throw new InvalidDataException(String.Format("In position [{0}] not started record", recordPotion));
            }
            RecordDataItem ret = new RecordDataItem();
            byte[] buffer = new byte[8];

            _fileStream.Read(buffer, 0, 4);
            int size = BitConverter.ToInt32(buffer, 0);//размер пакета

            _fileStream.Read(buffer, 0, 8); //считываем дату записи
            long date_time_binary = BitConverter.ToInt64(buffer, 0);
            ret.Time = DateTime.FromBinary(date_time_binary);

            _fileStream.Read(buffer, 0, 2); //считываем идентификатор источника
            ret.SourceId = BitConverter.ToUInt16(buffer, 0);

            _fileStream.Read(buffer, 0, 1);
            ret.DataTypeId = buffer[0];

            ret.Data = new byte[size - 1 - 2 - 8 - 4];
            _fileStream.Read(ret.Data, 0, ret.Data.Length);
            return ret;
        }

        public void ScanFileAndFillIndex(IFileStorageIndex index)
        {
            if (index == null) throw new ArgumentNullException("index");

            OpenStream();
            try
            {
                _fileStream.Position = _startingDataFilePosition;
                byte[] buffer = new byte[8];
                while (_fileStream.Position < _fileStream.Length)
                {
                    long current_file_position = _fileStream.Position;
                    if (_fileStream.ReadByte() != Byte.MaxValue)
                    {
                        throw new InvalidDataException("Error file format");
                    }

                    _fileStream.Read(buffer, 0, 4);
                    int size = BitConverter.ToInt32(buffer, 0);//размер пакета

                    _fileStream.Read(buffer, 0, 8); //считываем дату записи
                    long date_time_binary = BitConverter.ToInt64(buffer, 0);
                    var time = DateTime.FromBinary(date_time_binary);

                    _fileStream.Read(buffer, 0, 2); //считываем идентификатор источника
                    var sourceId = BitConverter.ToUInt16(buffer, 0);

                    _fileStream.Read(buffer, 0, 1);
                    var dataTypeId = buffer[0];

                    index.AddDataToIndex(current_file_position, time, sourceId, dataTypeId);

                    int data_size = size - 1 - 2 - 8 - 4;
                    _fileStream.Seek(data_size, SeekOrigin.Current);
                }
            }
            finally
            {
                CloseStream();
            }
        }

        protected override void OnDisposed()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }
        }
        

        public bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data, IFileStorageIndex index)
        {
            if (_fileStream.Length + data.Length > _maximumFileSizeInBytes)
            {
                return false;
            }

            if (_fileStream.Length != _fileStream.Position)
            {
                _fileStream.Seek(0, SeekOrigin.End);
            }

            long current_file_position = _fileStream.Position;

            int record_size = 4/*размер записи*/+ 8 /*время записи*/ + 2 /*идентификатор источнка*/+ 1 /*тип данных*/+ data.Length;


            _fileStream.WriteByte(Byte.MaxValue);

            byte[] buffer = BitConverter.GetBytes(record_size);
            _fileStream.Write(buffer, 0, buffer.Length);//записываем размер пакета

            var dateTime = _timeSerivice.UTCNow;
            buffer = BitConverter.GetBytes(dateTime.ToBinary());
            _fileStream.Write(buffer, 0, buffer.Length);//записываем дату записи

            buffer = BitConverter.GetBytes(sourceId);
            _fileStream.Write(buffer, 0, buffer.Length);//записываем идентификатор источника

            _fileStream.WriteByte(dataTypeId); //записываем тип данных

            _fileStream.Write(data, 0, data.Length); //записываем данные
            _fileStream.Flush();
            index.AddDataToIndex(current_file_position, dateTime, sourceId, dataTypeId);
            return true;
        }



        /// <summary>
        /// Возвращает имя файла с данными по дате создания
        /// </summary>
        public static string GetFileNameByTime(DateTime createtionTime)
        {
            return String.Format("{0}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}{6}DB.dat",
                createtionTime.Year,
                createtionTime.Month,
                createtionTime.Day,
                createtionTime.Hour,
                createtionTime.Minute,
                createtionTime.Second, createtionTime.Millisecond);
        }

        /// <summary>
        /// "Уникальный" префик к файлам базы данных.
        /// 
        /// </summary>
        /// <returns></returns>
        public static byte[] GetFileDBPrefix()
        {
            return new byte[] { 20, 30, 2, 1, 4, 6, 3, 1 };
        }

    }
}