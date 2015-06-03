﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMock;

namespace FileStorage.Tests
{

    class MockIndexRecord
    {
        public long currentFilePosition;
        public DateTime dateTime;
        public ushort sourceId;
        public byte dataTypeId;
    }

    class MockFileWritingIndex : IFileWritingIndex
    {
        public List<MockIndexRecord> Records = new List<MockIndexRecord>();

        public void AddDataToIndex(long currentFilePosition, DateTime dateTime, ushort sourceId, byte dataTypeId)
        {
            throw new NotImplementedException();
        }

        public void FlushIndexToFile(string data_base_file)
        {
            throw new NotSupportedException();
        }
    }
    [TestClass]
    public class CurrentFileStorageTest
    {

        [TestInitialize]
        public void TestInit()
        {
            DirectoryStorageTest.ClearTestDirectory();
        }

        [TestMethod]
        public void SimpleProcessWithEmptyRecords()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<ITimeSerivice> timeSerivice = mockFactory.CreateMock<ITimeSerivice>();
            DateTime t1 = DateTime.Now.AddDays(-1);


            Mock<IFileWritingIndex> index = mockFactory.CreateMock<IFileWritingIndex>();
            timeSerivice.Expects.One.Method(x => x.UTCNow).WillReturn(t1);
            using (CurrentFileStorage target = new CurrentFileStorage(GetTestDirectory(), timeSerivice.MockObject, 100,
                    index.MockObject))
            {
                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                Assert.AreEqual(DirectoryStorage.GetFileNameByTime(t1), target.FileName);

            }
            bool bl = false;
            try
            {
                var target = new FileStorageReader(DirectoryStorage.GetFileNameByTime(t1.AddMilliseconds(1)));
            }
            catch (FileNotFoundException)
            {
                bl = true;
            }
            Assert.IsTrue(bl);

            using (var stream = File.Create(DirectoryStorage.GetFileNameByTime(t1.AddMilliseconds(1))))
            {
                byte[] data = DirectoryStorage.GetFileDBPrefix();
                data[2] = 0;
                stream.Write(data, 0, data.Length);
            }

            bl = false;
            try
            {
                var target = new FileStorageReader(DirectoryStorage.GetFileNameByTime(t1.AddMilliseconds(1)));
            }
            catch (InvalidDataException)
            {
                bl = true;
            }
            Assert.IsTrue(bl);

            {
                var target = new FileStorageReader(DirectoryStorage.GetFileNameByTime(t1));
                Mock<IFileWritingIndex> new_index = mockFactory.CreateMock<IFileWritingIndex>();
                target.ScanFileAndFillIndex(new_index.MockObject);
                mockFactory.VerifyAllExpectationsHaveBeenMet();
            }
        }


        [TestMethod]
        public void SimpleProcessWithOneRecord()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<ITimeSerivice> timeSerivice = mockFactory.CreateMock<ITimeSerivice>();
            DateTime t1 = DateTime.Now.AddDays(-1);
            DateTime t2 = t1.AddSeconds(1);


            long record_position = 0;
            MockFileWritingIndex index = new MockFileWritingIndex();
            timeSerivice.Expects.One.Method(x => x.UTCNow).WillReturn(t1);
            using (CurrentFileStorage target = new CurrentFileStorage(GetTestDirectory(), timeSerivice.MockObject, 100, index))
            {
                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                Assert.AreEqual(DirectoryStorage.GetFileNameByTime(t1), target.FileName);

                timeSerivice.Expects.One.Method(x => x.UTCNow).WillReturn(t2);
                target.WriteRecord(10, 20, new byte[] { 30 });

                Assert.AreEqual(1, index.Records.Count);
                var index_item = index.Records[0];
                Assert.AreEqual(10, index_item.sourceId);
                Assert.AreEqual(20, index_item.dataTypeId);
                Assert.AreEqual(t2, index_item.dateTime);
                Assert.IsTrue(index_item.currentFilePosition > 0);

                record_position = index_item.currentFilePosition;
                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();



                var record = target.GetDbDataRecord(index_item.currentFilePosition);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t1, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);

                bool bl = false;
                try
                {
                    target.GetDbDataRecord(index_item.currentFilePosition - 1);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);


                bl = false;
                try
                {
                    target.GetDbDataRecord(index_item.currentFilePosition + 1);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);
            }

            long file_size = new FileInfo(DirectoryStorage.GetFileNameByTime(t1)).Length;

            using (var target = new FileStorageReader(DirectoryStorage.GetFileNameByTime(t1)))
            {

                index = new MockFileWritingIndex();
                target.ScanFileAndFillIndex(index);

                Assert.AreEqual(1, index.Records.Count);
                var index_item = index.Records[0];
                Assert.AreEqual(10, index_item.sourceId);
                Assert.AreEqual(20, index_item.dataTypeId);
                Assert.AreEqual(t2, index_item.dateTime);
                Assert.AreEqual(record_position, index_item.currentFilePosition);
                Assert.IsTrue(index_item.currentFilePosition > 0);

                target.OpenStream();
                var record = target.GetDbDataRecord(index_item.currentFilePosition);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t1, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);

                bool bl = false;
                try
                {
                    target.GetDbDataRecord(index_item.currentFilePosition - 1);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);


                bl = false;
                try
                {
                    target.GetDbDataRecord(index_item.currentFilePosition + 1);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                bl = false;
                try
                {
                    target.GetDbDataRecord(index_item.currentFilePosition + 1);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                bl = false;
                try
                {
                    target.GetDbDataRecord(file_size - 1);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);


                bl = false;
                try
                {
                    target.GetDbDataRecord(file_size);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                bl = false;
                try
                {
                    target.GetDbDataRecord(file_size + 1);
                }
                catch (InvalidDataException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                record = target.GetDbDataRecord(index_item.currentFilePosition);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t1, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);
            }


            using (var target = new FileStorageReader(DirectoryStorage.GetFileNameByTime(t1)))
            {
                bool bl = false;
                try
                {
                    target.GetDbDataRecord(record_position);
                }
                catch (InvalidOperationException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                target.OpenStream();
                var record = target.GetDbDataRecord(record_position);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t1, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);
                target.CloseStream();

                bl = false;
                try
                {
                    target.GetDbDataRecord(record_position);
                }
                catch (InvalidOperationException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);
                target.OpenStream();
                record = target.GetDbDataRecord(record_position);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t1, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);
            }
        }


        [TestMethod]
        public void SimpleProcessWithTwoRecord()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<ITimeSerivice> timeSerivice = mockFactory.CreateMock<ITimeSerivice>();
            DateTime t1 = DateTime.Now.AddDays(-1);
            DateTime t2 = t1.AddSeconds(1);
            DateTime t3 = t1.AddSeconds(2);


            long first_record_position = 0;
            long second_record_position = 0;
            MockFileWritingIndex index = new MockFileWritingIndex();
            timeSerivice.Expects.One.Method(x => x.UTCNow).WillReturn(t1);
            using (
                CurrentFileStorage target = new CurrentFileStorage(GetTestDirectory(), timeSerivice.MockObject, 100,
                    index))
            {
                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                Assert.AreEqual(DirectoryStorage.GetFileNameByTime(t1), target.FileName);

                timeSerivice.Expects.One.Method(x => x.UTCNow).WillReturn(t2);
                target.WriteRecord(10, 20, new byte[] { 30 });

                Assert.AreEqual(1, index.Records.Count);
                var index_item = index.Records[0];
                Assert.AreEqual(10, index_item.sourceId);
                Assert.AreEqual(20, index_item.dataTypeId);
                Assert.AreEqual(t2, index_item.dateTime);
                Assert.IsTrue(index_item.currentFilePosition > 0);

                first_record_position = index_item.currentFilePosition;
                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                var record = target.GetDbDataRecord(index_item.currentFilePosition);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t1, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);

                timeSerivice.Expects.One.Method(x => x.UTCNow).WillReturn(t3);
                target.WriteRecord(101, 201, new byte[] { 31, 41, 51 });

                Assert.AreEqual(2, index.Records.Count);
                index_item = index.Records[0];
                Assert.AreEqual(10, index_item.sourceId);
                Assert.AreEqual(20, index_item.dataTypeId);
                Assert.AreEqual(t2, index_item.dateTime);
                Assert.IsTrue(index_item.currentFilePosition > 0);


                index_item = index.Records[1];
                Assert.AreEqual(101, index_item.sourceId);
                Assert.AreEqual(201, index_item.dataTypeId);
                Assert.AreEqual(t3, index_item.dateTime);
                Assert.IsTrue(index_item.currentFilePosition > 0);
                second_record_position = index_item.currentFilePosition;

                record = target.GetDbDataRecord(second_record_position);
                Assert.AreEqual(101, record.SourceId);
                Assert.AreEqual(201, record.DataTypeId);
                Assert.AreEqual(t3, record.Time);
                Assert.AreEqual(3, record.Data.Length);
                Assert.AreEqual(31, record.Data[0]);
                Assert.AreEqual(41, record.Data[1]);
                Assert.AreEqual(51, record.Data[2]);

                record = target.GetDbDataRecord(first_record_position);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t2, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);

            }

            using (var target = new FileStorageReader(DirectoryStorage.GetFileNameByTime(t1)))
            {

                index = new MockFileWritingIndex();
                target.ScanFileAndFillIndex(index);


                Assert.AreEqual(2, index.Records.Count);
                var index_item = index.Records[0];
                Assert.AreEqual(10, index_item.sourceId);
                Assert.AreEqual(20, index_item.dataTypeId);
                Assert.AreEqual(t2, index_item.dateTime);
                Assert.AreEqual(first_record_position, index_item.currentFilePosition);
                Assert.IsTrue(index_item.currentFilePosition > 0);


                index_item = index.Records[1];
                Assert.AreEqual(101, index_item.sourceId);
                Assert.AreEqual(201, index_item.dataTypeId);
                Assert.AreEqual(t3, index_item.dateTime);
                Assert.AreEqual(second_record_position, index_item.currentFilePosition);

                target.OpenStream();

                var record = target.GetDbDataRecord(second_record_position);
                Assert.AreEqual(101, record.SourceId);
                Assert.AreEqual(201, record.DataTypeId);
                Assert.AreEqual(t3, record.Time);
                Assert.AreEqual(3, record.Data.Length);
                Assert.AreEqual(31, record.Data[0]);
                Assert.AreEqual(41, record.Data[1]);
                Assert.AreEqual(51, record.Data[2]);

                record = target.GetDbDataRecord(first_record_position);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t2, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);

                target.CloseStream();


                target.OpenStream();


                record = target.GetDbDataRecord(first_record_position);
                Assert.AreEqual(10, record.SourceId);
                Assert.AreEqual(20, record.DataTypeId);
                Assert.AreEqual(t2, record.Time);
                Assert.AreEqual(1, record.Data.Length);
                Assert.AreEqual(30, record.Data[0]);

                record = target.GetDbDataRecord(second_record_position);
                Assert.AreEqual(101, record.SourceId);
                Assert.AreEqual(201, record.DataTypeId);
                Assert.AreEqual(t3, record.Time);
                Assert.AreEqual(3, record.Data.Length);
                Assert.AreEqual(31, record.Data[0]);
                Assert.AreEqual(41, record.Data[1]);
                Assert.AreEqual(51, record.Data[2]);


                target.CloseStream();

            }
        }

        [TestMethod]
        public void SimpleProcessBigTest()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<ITimeSerivice> timeSerivice = mockFactory.CreateMock<ITimeSerivice>();
            DateTime t1 = DateTime.Now.AddDays(-1);



            List<string> record_data = new List<string>();
            List<long> positions =new List<long>();  

            MockFileWritingIndex index = new MockFileWritingIndex();
            timeSerivice.Expects.One.Method(x => x.UTCNow).WillReturn(t1);
            using (CurrentFileStorage target = new CurrentFileStorage(GetTestDirectory(), timeSerivice.MockObject, 100, index))
            {
                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                Assert.AreEqual(DirectoryStorage.GetFileNameByTime(t1), target.FileName);

                for (ushort i = 1; i < 20; i++)
                {
                    string item_data = Guid.NewGuid().ToString();
                    record_data.Add(item_data);
                    timeSerivice.Expects.One.Method(x => x.UTCNow).WillReturn(t1.AddSeconds(i));
                    Assert.IsTrue(target.WriteRecord(i, (byte)(i + 1), Encoding.UTF8.GetBytes(item_data)));
                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                long file_size = target.FileSize;
                Assert.IsFalse(target.WriteRecord(10,10, new byte[] { 10})); //больше чем 100 байт
                
                Assert.AreEqual(20,index.Records.Count);
                for (ushort i = 1; i < 20; i++)
                {
                    var index_item = index.Records[i - 1];
                    Assert.AreEqual(i, index_item.sourceId);
                    Assert.AreEqual(i+1, index_item.dataTypeId);
                    Assert.AreEqual(t1.AddSeconds(i), index_item.dateTime);
                    Assert.IsTrue(index_item.currentFilePosition>0);
                    positions.Add(index_item.currentFilePosition);
                    var record = target.GetDbDataRecord(index_item.currentFilePosition);
                    Assert.AreEqual(i, record.SourceId);
                    Assert.AreEqual(i + 1, record.DataTypeId);
                    Assert.AreEqual(t1.AddSeconds(i), record.Time);
                    Assert.AreEqual(record_data[i - 1], Encoding.UTF8.GetString(record.Data));
                }

            }

            using (var target = new FileStorageReader(DirectoryStorage.GetFileNameByTime(t1)))
            {

                index = new MockFileWritingIndex();
                target.ScanFileAndFillIndex(index);

                Assert.AreEqual(20, index.Records.Count);
                for (ushort i = 1; i < 20; i++)
                {
                    var index_item = index.Records[i - 1];
                    Assert.AreEqual(i, index_item.sourceId);
                    Assert.AreEqual(i + 1, index_item.dataTypeId);
                    Assert.AreEqual(t1.AddSeconds(i), index_item.dateTime);
                    Assert.IsTrue(index_item.currentFilePosition > 0);
                    var record = target.GetDbDataRecord(index_item.currentFilePosition);
                    Assert.AreEqual(i, record.SourceId);
                    Assert.AreEqual(i + 1, record.DataTypeId);
                    Assert.AreEqual(t1.AddSeconds(i), record.Time);
                    Assert.AreEqual(record_data[i - 1], Encoding.UTF8.GetString(record.Data));
                }
            }

            using (var target = new FileStorageReader(DirectoryStorage.GetFileNameByTime(t1)))
            {
                target.OpenStream();
                Assert.AreEqual(20, index.Records.Count);
                for (int i = 1; i < positions.Count; i++)
                {
                    long position = positions[i-1];
                    var record = target.GetDbDataRecord(position);
                    Assert.AreEqual(i, record.SourceId);
                    Assert.AreEqual(i + 1, record.DataTypeId);
                    Assert.AreEqual(t1.AddSeconds(i), record.Time);
                    Assert.AreEqual(record_data[i - 1], Encoding.UTF8.GetString(record.Data));
                }
                target.CloseStream();
            }

        }



        private string GetTestDirectory()
        {
            return DirectoryStorageTest.GetTestDirectory();
        }
    }
}