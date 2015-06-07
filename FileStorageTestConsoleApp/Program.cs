using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileStorage;

namespace FileStorageTestConsoleApp
{
    class Program
    {

        class DBConfig : IDirectoryStorageConfiguration
        {
            /// <summary>
            /// Максимальный размер одного файла 
            /// </summary>
            public long MaximumMegabytesInFile { get; set; }

            /// <summary>
            /// Минимальный размер данных в записи
            /// </summary>
            public long MinimumRecordDataSizeInBytes { get; set; }

            /// <summary>
            /// Максимальный размер данных в записи
            /// </summary>
            public long MaximumRecordDataSizeInKilobytes { get; set; }

            /// <summary>
            /// Максимальный размер данных в ответе на запрос.
            /// Защита от большого запроса
            /// </summary>
            public long MaximumResultDataSizeInMegabytes { get; set; }
        }
        static void Main(string[] args)
        {
            string directory = "D:\\TestDB";
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Создаем папку с базой");
                Directory.CreateDirectory(directory);
            }

            //конфигурация базы данных
            DBConfig configuration = new DBConfig();
            //Максимальный размер одного файла 
            configuration.MaximumMegabytesInFile = 2048;

            //Минимальный размер данных в записи
            configuration.MinimumRecordDataSizeInBytes = 128;

            // Максимальный размер данных в записи
            configuration.MaximumRecordDataSizeInKilobytes = 1024;

            //Максимальный размер данных в ответе на запрос. Защита от большого запроса
            configuration.MaximumResultDataSizeInMegabytes = 50;

            Console.WriteLine("Инициализация базы данных {0}", directory);
            Stopwatch sp = new Stopwatch();
            sp.Start();
            DirectoryStorage storage = new DirectoryStorage(directory, configuration, new FileStorageFactory(new TimeSerivice(), configuration));
            sp.Stop();
            writeFiles(storage);
            Console.WriteLine("Инициализация завершена {0}", sp.Elapsed);

            long isActive = 1;

            //Генерация списка идентификаторов источников
            List<ushort> source_ids = new List<ushort>();
            for (ushort i = 1; i < 100; i++)
            {
                source_ids.Add(i);
            }
            //Генерация типов данных
            List<byte> types_ids = new List<byte>() { 10, 20,30,40,50,60,70,80,90,100 };


            //Поток для непрерывной записи данных
            Task.Factory.StartNew(() =>
            {
                Random rnd = new Random();

                long min_data_size = configuration.MinimumRecordDataSizeInBytes;
                long max_data_size = configuration.MaximumRecordDataSizeInKilobytes * 1024;

                DateTime time = DateTime.Now;
                int count_writes = 0;

                int source_index = 0;
                while (Interlocked.Read(ref isActive) == 1)
                {
                    count_writes++;
                    try
                    {
                        ushort source_id = source_ids[source_index];

                        source_index++;
                        if (source_index >= source_ids.Count)
                        {
                            source_index = 0;
                        }

                        byte types_id = types_ids[rnd.Next(0, types_ids.Count - 1)];

                        long data_size = min_data_size + (long)((max_data_size - min_data_size) * rnd.NextDouble() * rnd.NextDouble());

                        byte[] data = new byte[data_size];

                        byte[] buffer = BitConverter.GetBytes(data_size);
                        Buffer.BlockCopy(buffer, 0, data, 0, buffer.Length);

                        buffer = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                        Buffer.BlockCopy(buffer, 0, data, data.Length - buffer.Length, buffer.Length);

                        storage.SaveData(source_id, types_id, data);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("ErrorWrite record:{0}", exception.Message);
                    }
                    TimeSpan span = DateTime.Now - time;
                    if (span.TotalSeconds > 1)
                    {
                        Console.WriteLine("Writes per second:{0}", count_writes);
                        count_writes = 0;
                        time = DateTime.Now;
                    }
                    Thread.Sleep(2);
                }
            });

            //Поток для проведения тестовых запросов
            Task.Factory.StartNew(() =>
            {
                Random rnd = new Random();



                while (Interlocked.Read(ref isActive) == 1)
                {
                    try
                    {

                        List<FileStorageInfo> list = storage.GetFilesInfos();

                        if (list.Count > 0)
                        {
                            int count = rnd.Next(0, list.Count - 1);
                            FileStorageInfo selected_file = list[count];

                            //DateTime start_time = selected_file.SavedTimeRangeUTC.StartTime;
                            //DateTime finishRange = start_time.AddSeconds(1);// selected_file.SavedTimeRangeUTC.FinishTime;

                            var total_duration = selected_file.SavedTimeRangeUTC.FinishTime - selected_file.SavedTimeRangeUTC.StartTime;

                            DateTime start_time = selected_file.SavedTimeRangeUTC.StartTime.AddSeconds(total_duration.TotalSeconds * rnd.NextDouble());

                            Debug.Assert(start_time >= selected_file.SavedTimeRangeUTC.StartTime && start_time < selected_file.SavedTimeRangeUTC.FinishTime);
                            DateTime finishRange = start_time.AddSeconds(1);


                            //Console.WriteLine("Request duration{0}", finishRange - start_time);
                            List<ushort> requested_source_ids = new List<ushort>();

                            count = rnd.Next(0, 5);

                            if (count > 0)
                            {
                                while (requested_source_ids.Count < count)
                                {
                                    var val = source_ids[rnd.Next(0, source_ids.Count - 1)];
                                    if (requested_source_ids.Contains(val) == false)
                                    {
                                        requested_source_ids.Add(val);
                                    }
                                }
                            }


                            List<byte> request_type_ids = new List<byte>();

                            if (rnd.NextDouble() > 0.5f)
                            {
                                count = rnd.Next(0, types_ids.Count);

                                if (count > 0)
                                {
                                    while (request_type_ids.Count < count)
                                    {
                                        var val = types_ids[rnd.Next(0, types_ids.Count - 1)];
                                        if (request_type_ids.Contains(val) == false)
                                        {
                                            request_type_ids.Add(val);
                                        }
                                    }
                                }
                            }

                            DateTime time = DateTime.Now;

                            var datas = storage.GetData(start_time, finishRange, requested_source_ids, request_type_ids);

                            long total_data_size = 0;
                            foreach (var record in datas) //проверяем записи
                            {
                                if (record.Time < start_time)
                                {
                                    throw new Exception(
                                        String.Format(
                                            "Error record data record.Time[{0}] < _requested_start_time[{1}]",
                                            record.Time, start_time));
                                }
                                if (record.Time > finishRange)
                                {
                                    throw new Exception(
                                        String.Format(
                                            "Error record data record.Time[{0}] > _requested_finishRange[{1}]",
                                            record.Time, finishRange));
                                }

                                var time_binary = BitConverter.ToInt64(record.Data, record.Data.Length - 8);
                                DateTime record_time = DateTime.FromBinary(time_binary);
                                if (Math.Abs((record.Time - record_time).TotalMilliseconds) > 500)
                                {
                                    throw new Exception(
                                        String.Format(
                                            "Error record data Math.Abs((record.Time[{0}]-record_time[{1}]).TotalMilliseconds[{2}])>100",
                                            record.Time, record_time,
                                            Math.Abs((record.Time - record_time).TotalMilliseconds)));
                                }

                                long size = BitConverter.ToInt64(record.Data, 0);
                                if (size != record.Data.Length)
                                {
                                    throw new Exception(
                                        String.Format(
                                            "Error record data expected size[{0}] != record.Data.Length[{1}]", size,
                                            record.Data.Length));
                                }
                                
                                total_data_size += record.Data.Length;
                            }

                            TimeSpan span = DateTime.Now - time;
                            Console.WriteLine("Request duration:{0} CountRecords:{1} TotalDataSize(Kb):{2}",
                                span.TotalMilliseconds, datas.Count, (float) total_data_size/1024);
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("ErrorRequest:{0}", exception.Message);
                    }
                    Thread.Sleep(1000);
                }

            });


            Console.ReadLine();


            Interlocked.Exchange(ref isActive, 0);

            writeFiles(storage);

            storage.Dispose();
            Console.WriteLine("Stop test app! Press asy key");
            Console.Read();
        }

        private static void writeFiles(DirectoryStorage storage)
        {
            foreach (var fileStorageInfo in storage.GetFilesInfos())
            {
                FileInfo info = new FileInfo(fileStorageInfo.FileName);
                Console.WriteLine("{0} Records:{1} Size(MB):{2} St:{3} Fin:{4} Current:{5}", info.Name,
                    fileStorageInfo.CountRecords, ((float)fileStorageInfo.SizeInBytes) / (1024 * 1024),
                    fileStorageInfo.SavedTimeRangeUTC.StartTime, fileStorageInfo.SavedTimeRangeUTC.FinishTime,
                    fileStorageInfo.IsNewFile);
            }
        }
    }
}
