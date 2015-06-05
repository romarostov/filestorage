using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMock;

namespace FileStorage.Tests
{
    [TestClass]
    public class DirectoryStorageTest
    {

        public static string  GetTestDirectory()
        {
            var ams_dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            string test_dir = Path.Combine(ams_dir.FullName, "TestDir");
            return test_dir;
        }

        public static void ClearTestDirectory()
        {
            string test_dir = GetTestDirectory();
            if (Directory.Exists(test_dir))
            {
                Directory.Delete(test_dir, true);
            }
            Directory.CreateDirectory(test_dir);
        }

        [TestInitialize]
        public void TestInit()
        {
            ClearTestDirectory();
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


        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation5()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mockFactory.CreateMock<IFileStorageFactory>();

            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1024);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(200);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumMegabytesInFile).WillReturn(1);
            new DirectoryStorage(GetTestDirectory(), config.MockObject,fileStorageFactory.MockObject);
        }


        
        [TestMethod]
        public void SimpleProcessWithMinimumConfigurationAndOneRecord()
        {
            MockFactory mock_factory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mock_factory.CreateMock<IDirectoryStorageConfiguration>();
            Mock<IFileStorageFactory> fileStorageFactory = mock_factory.CreateMock<IFileStorageFactory>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumMegabytesInFile).WillReturn(1);
            Mock<ITimeSerivice> time_serivice = mock_factory.CreateMock<ITimeSerivice>();
            using (var target = new DirectoryStorage(GetTestDirectory(), config.MockObject,fileStorageFactory.MockObject, time_serivice.MockObject))
            {
                Assert.AreEqual(0, target.GetFilesInfos().Count);

                bool bl = false;
                try
                {
                    target.SaveData(0,2,new byte[]{20,30});
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                bl = false;
                try
                {
                    target.SaveData(1, 2, null);
                }
                catch (ArgumentNullException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                Assert.AreEqual(0, target.GetFilesInfos().Count);


                DateTime t1=DateTime.Now.AddDays(-1);

                time_serivice.Expects.One.GetProperty(x => x.UTCNow).WillReturn(t1);
                target.SaveData(10,4,new byte[]{2});

                mock_factory.ClearException();
                mock_factory.VerifyAllExpectationsHaveBeenMet();

                Assert.AreEqual(1, target.GetFilesInfos().Count);
                //var file_info = target.GetFilesInfos().Single(x => x.FileName == DirectoryStorage.GetFileNameByTime(t1));
                //Assert.AreEqual(1,file_info.CountRecords);
                //Assert.AreEqual(true,file_info.IsCurrent);
                //Assert.AreEqual(t1,file_info.SavedTimeRangeUTC.StartTime);
                //Assert.AreEqual(t1,file_info.SavedTimeRangeUTC.FinishTime);
                //Assert.AreEqual(3+1,file_info.SizeInBytes);


                bl = false;
                try
                {
                    target.GetData(t1.AddDays(-1), t1.AddDays(-1), null, null);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);


                bl = false;
                try
                {
                    target.GetData(t1.AddDays(-1), t1.AddDays(-2), null, null);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);


                Assert.AreEqual(0,target.GetData(t1.AddDays(-1), t1.AddDays(-2), null, null).Count);

                {
                    var resuls = target.GetData(t1.AddDays(-1), t1, null, null);
                    Assert.AreEqual(1,resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1,item.Time);
                    Assert.AreEqual(10,item.SourceId);
                    Assert.AreEqual(4,item.DataTypeId);
                    Assert.AreEqual(1,item.Data.Length);
                    Assert.AreEqual(2,item.Data[0]);
                }


                {
                    var resuls = target.GetData(t1.AddDays(-1), t1.AddDays(1), null, null);
                    Assert.AreEqual(1, resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(4, item.DataTypeId);
                    Assert.AreEqual(1, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);
                }


                {
                    var resuls = target.GetData(t1.AddDays(-1), t1.AddDays(1), new List<int>(){10}, null);
                    Assert.AreEqual(1, resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(4, item.DataTypeId);
                    Assert.AreEqual(1, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);

                    Assert.AreEqual(0,target.GetData(t1.AddDays(-1), t1.AddDays(1), new List<int>() { 11 }, null).Count);
                }


                {
                    var resuls = target.GetData(t1.AddDays(-1), t1.AddDays(1), new List<int>() { 11,10 }, null);
                    Assert.AreEqual(1, resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(4, item.DataTypeId);
                    Assert.AreEqual(1, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);
                }


                
                {
                    var resuls = target.GetData(t1.AddDays(-1), t1.AddDays(1), new List<int>() { 11, 10 }, new List<byte>(){4});
                    Assert.AreEqual(1, resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(4, item.DataTypeId);
                    Assert.AreEqual(1, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);

                    Assert.AreEqual(0,target.GetData(t1.AddDays(-1), t1.AddDays(1), new List<int>() { 11, 10 }, new List<byte>(){5}).Count);
                    
                }

                {
                    var resuls = target.GetData(t1.AddDays(-1), t1.AddDays(1), null, new List<byte>() { 4 });
                    Assert.AreEqual(1, resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(4, item.DataTypeId);
                    Assert.AreEqual(1, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);

                    Assert.AreEqual(0, target.GetData(t1.AddDays(-1), t1.AddDays(1), null, new List<byte>() { 5 }).Count);

                }

                {
                    var resuls = target.GetData(t1.AddDays(-1), t1.AddDays(1), null, new List<byte>() { 2,4,5 });
                    Assert.AreEqual(1, resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(4, item.DataTypeId);
                    Assert.AreEqual(1, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);

                    Assert.AreEqual(0, target.GetData(t1.AddDays(-1), t1.AddDays(1), null, new List<byte>() { 5 }).Count);

                }



                DateTime t2 = t1.AddMilliseconds(1);

                time_serivice.Expects.One.GetProperty(x => x.UTCNow).WillReturn(t2);
                target.SaveData(10, 6, new byte[] { 2,10,11,12 });

                mock_factory.ClearException();
                mock_factory.VerifyAllExpectationsHaveBeenMet();


                //long old_file_size = file_info.SizeInBytes;

                //Assert.AreEqual(t1, target.GetSavedRangeInUTC().StartTime);
                //Assert.AreEqual(t2, target.GetSavedRangeInUTC().FinishTime);
                //Assert.AreEqual(1, target.GetFilesInfos().Count);
                //file_info = target.GetFilesInfos().Single(x => x.FileName == DirectoryStorage.GetFileNameByTime(t1));
                //Assert.AreEqual(2, file_info.CountRecords);
                //Assert.AreEqual(true, file_info.IsCurrent);
                //Assert.AreEqual(t1, file_info.SavedTimeRangeUTC.StartTime);
                //Assert.AreEqual(t2, file_info.SavedTimeRangeUTC.FinishTime);
                //Assert.AreEqual(old_file_size+8+3 + 1, file_info.SizeInBytes);


                {
                    var resuls = target.GetData(t1.AddDays(-1), t1, null, null);
                    Assert.AreEqual(1, resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(4, item.DataTypeId);
                    Assert.AreEqual(1, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);
                }

                {
                    var resuls = target.GetData(t1.AddDays(-1), t2, null, null);
                    Assert.AreEqual(2, resuls.Count);
                    var item = resuls[0];
                    Assert.AreEqual(t1, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(4, item.DataTypeId);
                    Assert.AreEqual(1, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);

                    item = resuls[1];
                    Assert.AreEqual(t2, item.Time);
                    Assert.AreEqual(10, item.SourceId);
                    Assert.AreEqual(6, item.DataTypeId);
                    Assert.AreEqual(4, item.Data.Length);
                    Assert.AreEqual(2, item.Data[0]);
                    Assert.AreEqual(10, item.Data[1]);
                    Assert.AreEqual(11, item.Data[2]);
                    Assert.AreEqual(12, item.Data[3]);
                }


                
            }

        }


        [TestMethod]
        public void GetFileNameByTime()
        {
            Assert.AreEqual("20101110090807DB.dat", FileStorageReaderAndWriter.GetFileNameByTime(new DateTime(2010, 11, 10, 9, 8, 7)));
        }




    }
}
