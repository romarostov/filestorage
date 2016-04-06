using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMock;
using Storage.Interfaces;

namespace FileStorage.Tests
{
    [TestClass]
    public class DirectoryStorageTest
    {

        public static string GetTestDirectory(string test_dir_name = "TestDir")
        {
            var ams_dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            string test_dir = Path.Combine(ams_dir.FullName, test_dir_name);
            return test_dir;
        }

        public static void ClearTestDirectory(string test_dir_name = "TestDir")
        {
            string test_dir = GetTestDirectory(test_dir_name);
            if (Directory.Exists(test_dir))
            {
                Directory.Delete(test_dir, true);
            }
            Directory.CreateDirectory(test_dir);
        }
        
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void EmptyDirectoryName()
        {
            MockFactory mockFactory=new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mockFactory.CreateMock<IFileStorageFactory>();
            new DirectoryStorage("", config.MockObject,fileStorageFactory.MockObject);

        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void EmptyConfiguration()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mockFactory.CreateMock<IFileStorageFactory>();
            new DirectoryStorage(GetTestDirectory(), null,fileStorageFactory.MockObject);

        }

        [ExpectedException(typeof(DirectoryNotFoundException))]
        [TestMethod]
        public void DirectoryNoExists()
        {
            var ams_dir=new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            string test_dir = Path.Combine(ams_dir.FullName, "sdfsdfsdfsfsdfd");

            Assert.IsFalse(Directory.Exists(test_dir));

            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            
            Mock<IFileStorageFactory> fileStorageFactory = mockFactory.CreateMock<IFileStorageFactory>();

            new DirectoryStorage(test_dir, config.MockObject,fileStorageFactory.MockObject);
        }



        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation1()
        {

            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mockFactory.CreateMock<IFileStorageFactory>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(0);
            
            new DirectoryStorage(GetTestDirectory(), config.MockObject,fileStorageFactory.MockObject);
        }


        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation2()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mockFactory.CreateMock<IFileStorageFactory>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(0);
            new DirectoryStorage(GetTestDirectory(), config.MockObject,fileStorageFactory.MockObject);
        }
        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation3()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mockFactory.CreateMock<IFileStorageFactory>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1025);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(1);
            new DirectoryStorage(GetTestDirectory(), config.MockObject,fileStorageFactory.MockObject);
        }


        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation4()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mockFactory.CreateMock<IFileStorageFactory>();

            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1024);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumMegabytesInFile).WillReturn(0);
            new DirectoryStorage(GetTestDirectory(), config.MockObject,fileStorageFactory.MockObject);
        }
        

        
        [TestMethod]
        public void SimpleProcess()
        {
            MockFactory mock_factory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mock_factory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mock_factory.CreateMock<IFileStorageFactory>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumResultDataSizeInMegabytes).WillReturn(1);

            //Mock<ITimeSerivice> time_serivice = mock_factory.CreateMock<ITimeSerivice>();

            var directory = GetTestDirectory("DirectoryStorageSimpleProcee");
            ClearTestDirectory("DirectoryStorageSimpleProcee");

            string file1 = Path.Combine(directory, "file1.dat");
            string file2 = Path.Combine(directory, "file2.dat");
            using (var file = File.Create(file1))
            {
                
            }
            using (var file = File.Create(file2))
            {

            }

            var fileReader1 = mock_factory.CreateMock<IFileStorageReader>();
            fileStorageFactory.Expects.One.MethodWith(x => x.GetFileStorageReader(file1)).WillReturn(fileReader1.MockObject);

            fileStorageFactory.Expects.One.MethodWith(x => x.GetFileStorageReader(file2)).Will(Throw.Exception(new Exception("Error")));

            DateTime time1 = DateTime.UtcNow.AddHours(-1);
            DateTime time1_2 = time1.AddSeconds(1);
            DateTime time2 = time1_2.AddSeconds(1);
            DateTime time2_2 = time2.AddSeconds(1);
            DateTime time3 = time2_2.AddSeconds(1);
            DateTime time3_2 = time3.AddSeconds(1);

            fileReader1.Expects.One.GetProperty(x => x.StartRange).WillReturn(time1);

            DirectoryStorage target = new DirectoryStorage(directory, config.MockObject, fileStorageFactory.MockObject);

                mock_factory.VerifyAllExpectationsHaveBeenMet();
                mock_factory.ClearException();

                {
                    FileStorageInfo info1=new FileStorageInfo();
                    fileReader1.Expects.One.MethodWith(x => x.GetWorkInfo()).WillReturn(info1);

                    var infos=target.GetFilesInfos();
                    Assert.AreEqual(1,infos.Count);
                    Assert.AreEqual(true,infos.Contains(info1));
                }
                mock_factory.VerifyAllExpectationsHaveBeenMet();
                mock_factory.ClearException();

                Assert.AreEqual(0, target.GetData(time1.AddHours(-1), time1.AddSeconds(-1), null, null).Count);
                
                {

                    fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                    fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                    Assert.AreEqual(0,target.GetData(time1.AddHours(-1), time1, null, null).Count);
                }
                mock_factory.VerifyAllExpectationsHaveBeenMet();
                mock_factory.ClearException();

                //проверяем что при ошибки записи нового файла, не добавляли в общий список
                bool bl = false;
                try
                {
                    var reader_temp = mock_factory.CreateMock<IFileStorageWriter>();
                    fileStorageFactory.Expects.One.MethodWith(x => x.CreaNewFileStorage(directory))
                        .WillReturn(reader_temp.MockObject);
                    var bytes = new byte[] {10};
                    reader_temp.Expects.One.MethodWith(x => x.WriteRecord(10, 20, bytes))
                        .Will(Throw.Exception(new Exception()));

                    target.SaveData(10, 20, bytes);
                }
                catch
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                mock_factory.VerifyAllExpectationsHaveBeenMet(true);
                mock_factory.ClearException();

                {
                    FileStorageInfo info1 = new FileStorageInfo();
                    fileReader1.Expects.One.MethodWith(x => x.GetWorkInfo()).WillReturn(info1);

                    var infos = target.GetFilesInfos();
                    Assert.AreEqual(1, infos.Count);
                    Assert.AreEqual(true, infos.Contains(info1));
                }
                mock_factory.VerifyAllExpectationsHaveBeenMet();
                mock_factory.ClearException();

                var file_reader2 = mock_factory.CreateMock<IFileStorageWriter>();

            {
                fileStorageFactory.Expects.One.MethodWith(x => x.CreaNewFileStorage(directory))
                    .WillReturn(file_reader2.MockObject);
                var bytes = new byte[] {10};
                file_reader2.Expects.One.GetProperty(x => x.StartRange).WillReturn(time2);
                file_reader2.Expects.One.MethodWith(x => x.WriteRecord(10, 20, bytes)).WillReturn(true);

                target.SaveData(10, 20, bytes);
            }

            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            //проверяем что он добавился в общий список
            {
                FileStorageInfo info1 = new FileStorageInfo();
                fileReader1.Expects.One.MethodWith(x => x.GetWorkInfo()).WillReturn(info1);

                FileStorageInfo info2 = new FileStorageInfo();
                file_reader2.Expects.One.MethodWith(x => x.GetWorkInfo()).WillReturn(info2);

                var infos = target.GetFilesInfos();
                Assert.AreEqual(2, infos.Count);
                Assert.AreEqual(true, infos.Contains(info1));
                Assert.AreEqual(true, infos.Contains(info2));
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            {
                var bytes = new byte[] { 10 };
                file_reader2.Expects.One.MethodWith(x => x.WriteRecord(10, 20, bytes)).WillReturn(true);

                target.SaveData(10, 20, bytes);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                Assert.AreEqual(0, target.GetData(time1.AddHours(-1), time1.AddSeconds(-1), null, null).Count);
            }

            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();
            {

                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                
                fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();



                Assert.AreEqual(0, target.GetData(time1.AddHours(-1), time1, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);

                fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time1.AddHours(-1), time2, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();


            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);

                fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time2.AddSeconds(-1), time2, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            
            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);

                
                file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time2, time2, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);

                Assert.AreEqual(0, target.GetData(time2.AddSeconds(2), time2.AddHours(1), null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            var file_reader3 = mock_factory.CreateMock<IFileStorageWriter>();//проверяем переполнения файла
            {
                
                fileStorageFactory.Expects.One.MethodWith(x => x.CreaNewFileStorage(directory))
                    .WillReturn(file_reader3.MockObject);
                var bytes = new byte[] { 10 };
                file_reader3.Expects.One.GetProperty(x => x.StartRange).WillReturn(time3);
                file_reader2.Expects.One.MethodWith(x => x.WriteRecord(10, 20, bytes)).WillReturn(false);
                file_reader2.Expects.One.MethodWith(x => x.StopWritingDataToFile());
                file_reader3.Expects.One.MethodWith(x => x.WriteRecord(10, 20, bytes)).WillReturn(true);

                target.SaveData(10, 20, bytes);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            //проверяем что он добавился в общий список
            {
                FileStorageInfo info1 = new FileStorageInfo();
                fileReader1.Expects.One.MethodWith(x => x.GetWorkInfo()).WillReturn(info1);

                FileStorageInfo info2 = new FileStorageInfo();
                file_reader2.Expects.One.MethodWith(x => x.GetWorkInfo()).WillReturn(info2);

                FileStorageInfo info3 = new FileStorageInfo();
                file_reader3.Expects.One.MethodWith(x => x.GetWorkInfo()).WillReturn(info3);

                var infos = target.GetFilesInfos();
                Assert.AreEqual(3, infos.Count);
                Assert.AreEqual(true, infos.Contains(info1));
                Assert.AreEqual(true, infos.Contains(info2));
                Assert.AreEqual(true, infos.Contains(info3));
            }


            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                file_reader3.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time3_2);
                //file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time1.AddSeconds(-10), time1.AddMilliseconds(-1), null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();


            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                file_reader3.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time3_2);
                
                fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time1.AddSeconds(-10), time1, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();


            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                file_reader3.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time3_2);

                fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time1.AddSeconds(-10), time2_2, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();


            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                file_reader3.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time3_2);

                fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time1.AddSeconds(-10), time2_2, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();


            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                file_reader3.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time3_2);

                fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader3.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time1, time3_2, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                file_reader3.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time3_2);

                //fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader3.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time2_2, time3_2, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();


            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                file_reader3.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time3_2);

                //fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                //file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                file_reader3.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time3, time3_2, null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();


            {
                fileReader1.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time1_2);
                file_reader2.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time2_2);
                file_reader3.Stub.Out.GetProperty(x => x.FinishRange).WillReturn(time3_2);

                //fileReader1.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                //file_reader2.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                //file_reader3.Expects.One.Method(x => x.ProcessSearchRequest(null)).WithAnyArguments();
                Assert.AreEqual(0, target.GetData(time3_2.AddMilliseconds(1), time3_2.AddSeconds(1), null, null).Count);
            }
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();

            fileReader1.Expects.One.MethodWith(x => x.Dispose());
            target.Dispose();
            mock_factory.VerifyAllExpectationsHaveBeenMet();
            mock_factory.ClearException();



        }


        [TestMethod]
        public void GetFileNameByTime()
        {
            Assert.AreEqual("201011100908070DB.dat", FileStorageReaderAndWriter.GetFileNameByTime(new DateTime(2010, 11, 10, 9, 8, 7)));
        }




    }
}
