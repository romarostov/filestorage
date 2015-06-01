using System; 
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMock;

namespace FileStorage.Tests
{
    [TestClass]
    public class DirectoryStorageTest
    {

        string GetTestDirectory()
        {
            var ams_dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            string test_dir = Path.Combine(ams_dir.FullName, "TestDir");
            return test_dir;
        }

        [TestInitialize]
        public void TestInit()
        {
            string test_dir = GetTestDirectory();
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
            new DirectoryStorage("", config.MockObject);

        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void EmptyConfiguration()
        {
            
            new DirectoryStorage(GetTestDirectory(), null);

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
            new DirectoryStorage(test_dir, config.MockObject);
        }



        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation1()
        {
            
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(0);
            new DirectoryStorage(GetTestDirectory(), config.MockObject);
        }


        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation2()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(0);
            new DirectoryStorage(GetTestDirectory(), config.MockObject);
        }
        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation3()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1025);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(1);
            new DirectoryStorage(GetTestDirectory(), config.MockObject);
        }


        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation4()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1024);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumMegabytesInFile).WillReturn(0);
            new DirectoryStorage(GetTestDirectory(), config.MockObject);
        }


        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ConfigurationValidation5()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1024);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(200);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumMegabytesInFile).WillReturn(1);
            new DirectoryStorage(GetTestDirectory(), config.MockObject);
        }


        
        [TestMethod]
        public void SimpleProcessWithMinimumConfigurationAndOneRecord()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IDirectoryStorageConfiguration> config = mockFactory.CreateMock<IDirectoryStorageConfiguration>();
            config.Expects.AtLeastOne.GetProperty(x => x.MinimumRecordDataSizeInBytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumRecordDataSizeInKilobytes).WillReturn(1);
            config.Expects.AtLeastOne.GetProperty(x => x.MaximumMegabytesInFile).WillReturn(1);
            using (var target = new DirectoryStorage(GetTestDirectory(), config.MockObject))
            {

                Assert.AreEqual(null, target.SavedRange()); 
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


                Assert.AreEqual(null, target.SavedRange());
                Assert.AreEqual(0, target.GetFilesInfos().Count);



            }

        }




    }
}
