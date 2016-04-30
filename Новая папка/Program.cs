using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FizkultFixFiles.UnidecodeSharpFork;

namespace FizkultFixFiles
{

    class FizkultClinet
    {


        public static Dictionary<string, int> GetUlugaType()
        {
            Dictionary<string, int> Payd = new Dictionary<string, int>();
            Payd.Add("Trenazhernyy zal", 0);
            Payd.Add("Vodnye programmy", 0);
            Payd.Add("Solyariy", 0);
            Payd.Add("Aerobika", 0);
            Payd.Add("Stoyanka", 0);
            Payd.Add("Manikyur", 0);
            Payd.Add("Mind Body", 0);
            Payd.Add("Boevye iskusstva", 0);
            Payd.Add("Detskiy klub", 0);
            Payd.Add("Parikmakherskie uslugi", 0);
            Payd.Add("Lechebnyy massazh", 0);
            Payd.Add("Tantseval'nye programmy", 0);
            Payd.Add("Massazh_SF", 0);
            Payd.Add("Terapevticheskie protsedury", 0);
            Payd.Add("Kosmeticheskie uslugi", 0);
            Payd.Add("Igrovye programmy", 0);
            Payd.Add("Yoga", 0);
            Payd.Add("Fizioterapevticheskie protsedury", 0);
            Payd.Add("Apparatnaya kosmetologiya", 0);
            Payd.Add("Okrashivanie", 0);
            Payd.Add("Gruppovye programmy", 0);
            Payd.Add("Vrachi", 0);
            Payd.Add("Apparatnaya kosmetologiya_SF", 0);
            Payd.Add("Programmy vne kluba", 0);
            Payd.Add("Prochie uslugi kluba", 0);
            Payd.Add("Vrachi_SF", 0);
            Payd.Add("Prochie uslugi", 0);
            Payd.Add("Prodazha kosmeticheskikh tovarov", 0);
            Payd.Add("Prochie uslugi po prodazhe chlenstva", 0);
            Payd.Add("In\"ektsii", 0);
            Payd.Add("Lechebnoe pitanie", 0);
            Payd.Add("In\"ektsii_SF", 0);
            Payd.Add("Prochie uslugi SPA", 0);
            Payd.Add("Prochie vidy deyatel'nosti", 0);
            Payd.Add("Sportivno-ozdorovitel'nye uslugi", 0);
            Payd.Add("Meditsinskie uslugi", 0);
            Payd.Add("Okazanie uslug po chlenstvu", 0);
            return Payd;
        }


        public static Dictionary<string, int> GetKategoryType()
        {
            Dictionary<string, int> Payd = new Dictionary<string, int>();

            Payd.Add("Sotrudnik SPA", 0);
            Payd.Add("Trener master", 0);
            Payd.Add("Trener personal'nyy", 0);
            Payd.Add("Trener elit", 0);
            Payd.Add("Trener fitnes", 0);
            Payd.Add("Vrach", 0);
            return Payd;
        }

        public static Dictionary<string, int> GetCommunicationType()
        {
            Dictionary<string, int> Payd = new Dictionary<string, int>();

            Payd.Add("Telefonnyy zvonok iskhodyashchiy", 0);
            Payd.Add("Vstrecha", 0);
            Payd.Add("Telefonnyy zvonok vkhodyashchiy", 0);
            Payd.Add("Prodazha", 0);
            Payd.Add("Elektronnoe pis'mo iskhodyashchee", 0);
            Payd.Add("Obratnaya svyaz'", 0);
            Payd.Add("Vizit", 0);
            return Payd;
        }

        public int TotalVisits = 0;
        public int TotalVisitsInLastMonth = 0;
        public int TotalVisitsInPreviousMonth = 0;



        public FizkultClinet(int kodKlientaId)
        {
            KategoryType = GetKategoryType();
            LastKategoryType = GetKategoryType();
            //UslugaType = GetUlugaType();
            CommunicationType = GetCommunicationType();
        }

        public Dictionary<string, int> KategoryType { get; set; }

        public Dictionary<string, int> LastKategoryType { get; set; }

        //public Dictionary<string, int> UslugaType { get; set; }

        public Dictionary<string, int> CommunicationType { get; set; }
        public int TotalCommunicationsInPreviousMonth = 0;

        public int TotalCommunicationsInLastMonth = 0;

        public int TotalCommunications = 0;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string fileName;
            //string fileName = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\Communications.csv";
            //string fileName = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\DSUPoseweniya.csv";
            //string targetFile = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\DSUPoseweniya2.csv";
            ////string targetFile = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\Communications2.csv";
            //using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
            //{
            //    using (StreamWriter writer = new StreamWriter(File.Create(targetFile)))
            //    {
            //        while (reader.EndOfStream == false)
            //        {
            //            var line = reader.ReadLine();
            //            var new_line = line.Unidecode();

            //            writer.WriteLine(new_line);
            //        }
            //        writer.Flush();
            //    }
            //}

            Dictionary<int, FizkultClinet> clients = new Dictionary<int, FizkultClinet>();

            //DateTime lastMonthStart = new DateTime(2015, 11, 1);
            //DateTime previousMonthStart = new DateTime(2015, 10, 1);

            DateTime lastMonthStart = new DateTime(2016, 2, 1);
            DateTime previousMonthStart = new DateTime(2016, 1, 1);

            fileName = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\DSUPoseweniya2.csv";
            fillDSU(fileName, clients, lastMonthStart, previousMonthStart);

            fileName = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\Communications2.csv";
            fillCommunications(fileName, clients, lastMonthStart, previousMonthStart);


            processMainDocument(clients);
        }

        private static void fillDSU(string fileName, Dictionary<int, FizkultClinet> clients, DateTime lastMonthStart, DateTime previousMonthStart)
        {
            using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
            {
                int lineIndex = 0;
                {
                    Dictionary<string, int> columns_index = new Dictionary<string, int>();

                    int KodKlienta = 0;
                    int Data = 0;
                    int NachaloPoseshcheniya = 0;
                    int OkonchaniePoseshcheniya = 0;
                    int KategoriyaTrenera = 0;
                    int Dlitelnost = 0;
                    int NapravlenieUslugi = 0;

                    while (reader.EndOfStream == false)
                    {
                        lineIndex++;

                        var line = reader.ReadLine();
                        if (lineIndex == 1)
                        {
                            string[] strs = line.Split(',');
                            int i = 0;
                            for (; i < strs.Length; i++)
                            {
                                columns_index.Add(strs[i], i);
                            }
                            KodKlienta = columns_index["KodKlienta"];
                            Data = columns_index["Data"];
                            NachaloPoseshcheniya = columns_index["NachaloPoseshcheniya"];
                            OkonchaniePoseshcheniya = columns_index["OkonchaniePoseshcheniya"];
                            KategoriyaTrenera = columns_index["KategoriyaTrenera"];
                            Dlitelnost = columns_index["Dlitel'nost'"];
                            NapravlenieUslugi = columns_index["NapravlenieUslugi"];
                        }
                        else
                        {
                            string[] strs = line.Split(',');
                            if (strs.Length > 5)
                            {
                                int kodKlientaId = int.Parse(strs[KodKlienta]);

                                FizkultClinet currentClinet;
                                if (clients.TryGetValue(kodKlientaId, out currentClinet) == false)
                                {
                                    currentClinet = new FizkultClinet(kodKlientaId);
                                    clients.Add(kodKlientaId, currentClinet);
                                }

                                currentClinet.TotalVisits++;

                                DateTime visitTime = DateTime.Parse(strs[Data]);


                                if (visitTime > lastMonthStart)
                                {
                                    currentClinet.TotalVisitsInLastMonth++;
                                }
                                else if (visitTime > previousMonthStart)
                                {
                                    currentClinet.TotalVisitsInPreviousMonth++;
                                }

                                if (string.IsNullOrEmpty(strs[Data]))
                                {
                                }

                                var trener = strs[KategoriyaTrenera];
                                if (string.IsNullOrEmpty(trener) == false)
                                {
                                    currentClinet.KategoryType[trener]++;
                                    if (visitTime > previousMonthStart)
                                    {
                                        currentClinet.LastKategoryType[trener]++;
                                    }
                                }


                                //var usluga = strs[NapravlenieUslugi];
                                //if (string.IsNullOrEmpty(usluga) == false)
                                //{
                                //    usluga = usluga.Trim('"');
                                //    currentClinet.UslugaType[usluga]++;
                                //}
                            }
                        }
                    }
                }
            }
        }

        private static void fillCommunications(string fileName, Dictionary<int, FizkultClinet> clients, DateTime lastMonthStart, DateTime previousMonthStart)
        {
            using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
            {
                int lineIndex = 0;
                {
                    Dictionary<string, int> columns_index = new Dictionary<string, int>();

                    int KodKlienta = 0;
                    int Data = 0;
                    int VidSobytiya = 0;
                    int SostoyanieVzaimodeystviya = 0;

                    while (reader.EndOfStream == false)
                    {
                        lineIndex++;

                        var line = reader.ReadLine();
                        if (lineIndex == 1)
                        {
                            string[] strs = line.Split(',');
                            int i = 0;
                            for (; i < strs.Length; i++)
                            {
                                columns_index.Add(strs[i], i);
                            }
                            KodKlienta = columns_index["KodKlienta"];
                            Data = columns_index["DataVzaimodeystviya"];
                            VidSobytiya = columns_index["VidSobytiya"];
                            SostoyanieVzaimodeystviya = columns_index["SostoyanieVzaimodeystviya"];
                        }
                        else
                        {
                            string[] strs = line.Split(',');
                            if (strs.Length > 3)
                            {
                                if(string.IsNullOrEmpty(strs[KodKlienta]))continue;

                                int kodKlientaId = int.Parse(strs[KodKlienta]);

                                FizkultClinet currentClinet;
                                if (clients.TryGetValue(kodKlientaId, out currentClinet) == false)
                                {
                                    currentClinet = new FizkultClinet(kodKlientaId);
                                    clients.Add(kodKlientaId, currentClinet);
                                }

                                currentClinet.TotalCommunications++;

                                DateTime visitTime = DateTime.Parse(strs[Data]);

                                if (visitTime > lastMonthStart)
                                {
                                    currentClinet.TotalCommunicationsInLastMonth++;
                                }
                                else if (visitTime > previousMonthStart)
                                {
                                    currentClinet.TotalCommunicationsInPreviousMonth++;
                                }

                                string state = strs[SostoyanieVzaimodeystviya];

                                if (string.IsNullOrWhiteSpace(state))
                                {

                                }

                                var trener = strs[VidSobytiya];
                                if (string.IsNullOrEmpty(trener) == false)
                                {
                                    if (trener == "Vstrecha na oplatu")
                                    {
                                        trener = "Vstrecha";
                                    }
                                    if (trener == "Sms")
                                    {
                                        trener = "Telefonnyy zvonok iskhodyashchiy";
                                    }
                                    


                                    currentClinet.CommunicationType[trener]++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void processMainDocument(Dictionary<int, FizkultClinet> clients)
        {
            //string name = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\contracs.csv";
            string name = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\contracs.csv";
            //string newFileName = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\contracs2.csv";
            string newFileName = @"D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\contracs2.csv";
            int lineIndex = 0;

            //var uslugi = FizkultClinet.GetUlugaType();
            //var categories = FizkultClinet.GetKategoryType();
            var communicationType = FizkultClinet.GetCommunicationType();

            using (StreamReader reader = new StreamReader(File.OpenRead(name)))
            {
                using (StreamWriter writer = new StreamWriter(File.Create(newFileName)))
                {
                    Dictionary<string, int> columns_index = new Dictionary<string, int>();

                    int vozrastnayakategoriya = 0;
                    int datanachalasrokadejstviya = 0;
                    int dataokonchaniyasrokadejstviya = 0;
                    int vidstazhananachalomesyaca = 0;
                    int vidstazhavmomentpriobreteniya = 0;
                    int viddogovora = 0;
                    int bonusyostatok = 0;
                    int segmentkluba = 0;
                    //int prodlilsya = 0;
                    int kodklienta = 0;

                    while (reader.EndOfStream == false)
                    {
                        lineIndex++;

                        var line = reader.ReadLine();
                        if (lineIndex == 1)
                        {
                            writer.WriteLine(line);
                            continue;
                        }
                        else if (lineIndex == 2)
                        {
                            string[] strs = line.Split(',');
                            int i = 0;
                            for (; i < strs.Length; i++)
                            {
                                columns_index.Add(strs[i], i);
                            }
                            kodklienta = columns_index["kodklienta"];
                            vozrastnayakategoriya = columns_index["vozrastnayakategoriya"];
                            datanachalasrokadejstviya = columns_index["datanachalasrokadejstviya"];
                            dataokonchaniyasrokadejstviya = columns_index["dataokonchaniyasrokadejstviya"];
                            vidstazhananachalomesyaca = columns_index["vidstazhananachalomesyaca"];
                            vidstazhavmomentpriobreteniya = columns_index["vidstazhavmomentpriobreteniya"];
                            viddogovora = columns_index["viddogovora"];
                            bonusyostatok = columns_index["bonusyostatok"];
                            segmentkluba = columns_index["segmentkluba"];
                            //prodlilsya = columns_index["prodlilsya"];

                            string new_line = line;
                            new_line = new_line + ",TotalVisits,TotalVisitsInLastMonth,TotalVisitsInPreviousMonth";

                            //foreach (var usluga in uslugi.Keys)
                            //{
                            //    new_line = new_line + "," + usluga;
                            //}


                            //foreach (var usluga in categories.Keys)
                            //{
                            //    new_line = new_line + "," + usluga;
                            //}

                            //foreach (var usluga in categories.Keys)
                            //{
                            //    new_line = new_line + ",Last" + usluga;
                            //}

                            new_line = new_line + ",TotalCommunications,TotalCommunicationsInLastMonth,TotalCommunicationsInPreviousMonth";


                            foreach (var usluga in communicationType.Keys)
                            {
                                new_line = new_line + "," + usluga;
                            }

                            writer.WriteLine(new_line);
                        }
                        else
                        {
                            string[] strs = line.Split(',');

                            fixVozrastGroup(strs, vozrastnayakategoriya);
                            VidStaga(strs, vidstazhananachalomesyaca);
                            VidStaga(strs, vidstazhavmomentpriobreteniya);
                            Fixviddogovora(strs, viddogovora);
                            Fixbonusyostatok(strs, bonusyostatok);
                            Fixvsegmentkluba(strs, segmentkluba);
                            //Fixvprodlilsya(strs, prodlilsya);

                            StringBuilder new_line = new StringBuilder(string.Join(",", strs));

                            if(string.IsNullOrEmpty(strs[kodklienta]))continue;

                            int clientId = int.Parse(strs[kodklienta]);

                            FizkultClinet client;
                            if (clients.TryGetValue(clientId, out client) == false)
                            {
                                client = new FizkultClinet(clientId);
                            }

                            new_line.Append(",");
                            new_line.Append(client.TotalVisits);
                            new_line.Append(",");
                            new_line.Append(client.TotalVisitsInLastMonth);
                            new_line.Append(",");
                            new_line.Append(client.TotalVisitsInPreviousMonth);


                            //foreach (var usluga in uslugi.Keys)
                            //{
                            //    new_line.Append(",");

                            //    if (client.UslugaType[usluga] > 0)
                            //    {
                            //        new_line.Append("1");
                            //    }
                            //    else
                            //    {
                            //        new_line.Append("0");
                            //    }
                            //}

                            //foreach (var usluga in categories.Keys)
                            //{
                            //    new_line.Append(",");

                            //    new_line.Append(client.KategoryType[usluga]);
                            //    //if (client.KategoryType[usluga] > 0)
                            //    //{
                            //    //    new_line.Append("1");
                            //    //}
                            //    //else
                            //    //{
                            //    //    new_line.Append("0");
                            //    //}
                            //}


                            //foreach (var usluga in categories.Keys)
                            //{
                            //    new_line.Append(",");


                            //    new_line.Append(client.LastKategoryType[usluga]);

                            //    //if (client.LastKategoryType[usluga] > 0)
                            //    //{
                            //    //    new_line.Append("1");
                            //    //}
                            //    //else
                            //    //{
                            //    //    new_line.Append("0");
                            //    //}
                            //}


                            new_line.Append(",");
                            new_line.Append(client.TotalCommunications);
                            new_line.Append(",");
                            new_line.Append(client.TotalCommunicationsInLastMonth);
                            new_line.Append(",");
                            new_line.Append(client.TotalCommunicationsInPreviousMonth);


                            foreach (var usluga in communicationType.Keys)
                            {
                                new_line.Append(",");
                                new_line.Append(client.CommunicationType[usluga]);

                                //if (client.LastKategoryType[usluga] > 0)
                                //{
                                //    new_line.Append("1");
                                //}
                                //else
                                //{
                                //    new_line.Append("0");
                                //}
                            }

                            writer.WriteLine(new_line.ToString());
                        }
                    }

                    writer.Flush();
                }
            }
        }

        private static void totalDays(string[] strs, int datanachalasrokadejstviya, int dataokonchaniyasrokadejstviya,
            StringBuilder new_line)
        {

            DateTime endTime = DateTime.ParseExact(strs[dataokonchaniyasrokadejstviya], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            DateTime startTime;
            if (string.IsNullOrEmpty(strs[datanachalasrokadejstviya]))
            {
                startTime = endTime;
            }
            else
            {
                startTime = DateTime.ParseExact(strs[datanachalasrokadejstviya], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            new_line.Append(",");
            new_line.Append((int)(endTime - startTime).TotalDays);
        }

        private static void fixVozrastGroup(string[] strs, int vozrastnayakategoriya)
        {
            string vozrast = strs[vozrastnayakategoriya];
            if (vozrast == "Взрослые")
            {
                strs[vozrastnayakategoriya] = "1";
            }
            else if (vozrast == "Kids")
            {
                strs[vozrastnayakategoriya] = "2";
            }
            else if (vozrast == "Teens")
            {
                strs[vozrastnayakategoriya] = "3";
            }
            else throw new NotSupportedException();
        }

        private static void VidStaga(string[] strs, int columnIndex)
        {
            string vozrast = strs[columnIndex];
            if (vozrast == "New" || vozrast=="")
            {
                strs[columnIndex] = "1";
            }
            else if (vozrast == "Renew")
            {
                strs[columnIndex] = "2";
            }
            else if (vozrast == "Ex")
            {
                strs[columnIndex] = "3";
            }
            else throw new NotSupportedException();
        }

        private static void Fixviddogovora(string[] strs, int viddogovora)
        {
            string vozrast = strs[viddogovora];
            if (vozrast == "Корпоративный")
            {
                strs[viddogovora] = "1";
            }
            else if (vozrast == "Индивидуальный" || vozrast == "")
            {
                strs[viddogovora] = "2";
            }
            else if (vozrast == "Групповой" || vozrast== "Члены семей сотрудников")
            {
                strs[viddogovora] = "3";
            }
            else throw new NotSupportedException();
        }

        private static void Fixvsegmentkluba(string[] strs, int segmentkluba)
        {
            string vozrast = strs[segmentkluba];
            if (vozrast == "Lower")
            {
                strs[segmentkluba] = "1";
            }
            else if (vozrast == "Lite")
            {
                strs[segmentkluba] = "2";
            }
            else if (vozrast == "Upper" || vozrast == "Жуковка")
            {
                strs[segmentkluba] = "3";
            }
            else if (vozrast == "Lux")
            {
                strs[segmentkluba] = "4";
            }
            else throw new NotSupportedException();
        }
        private static void Fixvprodlilsya(string[] strs, int prodlilsya)
        {
            string vozrast = strs[prodlilsya];
            if (vozrast == "1")
            {
                strs[prodlilsya] = "0";
            }
            else if (vozrast == "0")
            {
                strs[prodlilsya] = "1";
            }
            else throw new NotSupportedException();
        }

        private static void Fixbonusyostatok(string[] strs, int bonusyostatok)
        {
            string vozrast = strs[bonusyostatok].Replace(" ", string.Empty);

            float sum = 0;
            if (!string.IsNullOrEmpty(vozrast)) sum = float.Parse(vozrast, CultureInfo.InvariantCulture);
            if (sum < 0)
            {
                strs[bonusyostatok] = "-1";
            }
            else if (sum < 50)
            {
                strs[bonusyostatok] = "0";
            }
            else if (sum < 500)
            {
                strs[bonusyostatok] = "1";
            }
            else if (sum < 1000)
            {
                strs[bonusyostatok] = "2";
            }
            else if (sum < 2000)
            {
                strs[bonusyostatok] = "3";
            }
            else
            {
                strs[bonusyostatok] = "4";
            }

        }


    }
}
