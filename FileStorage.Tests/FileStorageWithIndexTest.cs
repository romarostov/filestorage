using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMock;
using Storage.Interfaces;

namespace FileStorage.Tests
{
    [TestClass]
    public class FileStorageWithIndexTest
    {
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void EmptyReader()
        {
            new FileStorageWithIndex(true, null);
        }

        [TestMethod]
        public void NewIndexWithoutRecords()
        {
            MockFactory mockFactory = new MockFactory();
            Mock<IFileStorageReaderAndWriter> file_reader = mockFactory.CreateMock<IFileStorageReaderAndWriter>();
            file_reader.Expects.One.MethodWith(x => x.OpenStream());
            using (var target = new FileStorageWithIndex(true, file_reader.MockObject))
            {


                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                {
                    file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn("data.data");
                    file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(10);

                    var info = target.GetWorkInfo();
                    Assert.IsTrue(info.IsNewFile);

                    Assert.AreEqual("data.data", info.FileName);
                    Assert.AreEqual(0, info.CountRecords);
                    Assert.AreEqual(10, info.SizeInBytes);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }



                bool bl = false;
                try
                {
                    target.ProcessSearchRequest(null);
                }
                catch (ArgumentNullException)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);

                {
                    Mock<ISearchRequestData> request = mockFactory.CreateMock<ISearchRequestData>();
                    target.ProcessSearchRequest(request.MockObject);
                }

                bl = false;
                try
                {
                    target.StopWritingDataToFile(); //нет записей
                }
                catch (Exception)
                {
                    bl = true;
                }
                Assert.IsTrue(bl);


                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();
                byte[] data = new byte[10];

                file_reader.Expects.One.Method(x => x.WriteRecord(20, 30, data, null)).WithAnyArguments().WillReturn(true);
                Assert.IsTrue(target.WriteRecord(20, 30, data));

                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                file_reader.Expects.One.MethodWith(x => x.CloseStream());

            }
            mockFactory.VerifyAllExpectationsHaveBeenMet();
            mockFactory.ClearException();

        }

        [TestMethod]
        public void NewIndexWithOneRecord()
        {
            DirectoryStorageTest.ClearTestDirectory("NewIndexWithOneRecord");
            string directory = DirectoryStorageTest.GetTestDirectory("NewIndexWithOneRecord");

            MockFactory mockFactory = new MockFactory();


            DateTime t1 = DateTime.Now.AddSeconds(-10);

            DateTime t2 = t1.AddSeconds(1);


            //time.Expects.One.GetProperty(x => x.UTCNow).WillReturn(t1);
            var file_reader = mockFactory.CreateMock<IFileStorageReaderAndWriter>();

            string full_file_name = Path.Combine(directory, "data.dat");
            string full_index_name = Path.Combine(directory, "data.idx");
            Assert.IsFalse(File.Exists(full_index_name));
            file_reader.Expects.One.MethodWith(x => x.OpenStream());

            using (var target = new FileStorageWithIndex(true, file_reader.MockObject))
            {

                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                {

                    file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
                    file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn(full_file_name);
                    var info = target.GetWorkInfo();
                    Assert.IsTrue(info.IsNewFile);
                    Assert.AreEqual(0, info.CountRecords);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
                target.AddDataToIndex(20, t2, 30, 40);

                {

                    file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
                    file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn(full_file_name);
                    var info = target.GetWorkInfo();
                    Assert.IsTrue(info.IsNewFile);
                    Assert.AreEqual(1, info.CountRecords);
                    Assert.AreEqual(full_file_name, info.FileName);
                    Assert.AreEqual(20, info.SizeInBytes);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.StartTime);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.FinishTime);



                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();

                    
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t1);
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();

                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    var source_ids = new Dictionary<ushort, byte>();
                    //source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2.AddSeconds(2));
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(50, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                file_reader.Expects.One.MethodWith(x => x.CloseStream());

                file_reader.Expects.AtLeastOne.GetProperty(x => x.FileName).WillReturn(full_file_name);

                Assert.IsFalse(File.Exists(full_index_name));
                target.StopWritingDataToFile();

                Assert.IsTrue(File.Exists(full_index_name));
                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();
                {

                    file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
                    //file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn(full_file_name);
                    var info = target.GetWorkInfo();
                    Assert.IsFalse(info.IsNewFile);
                    Assert.AreEqual(1, info.CountRecords);
                    Assert.AreEqual(full_file_name, info.FileName);
                    Assert.AreEqual(20, info.SizeInBytes);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.StartTime);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.FinishTime);
                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2.AddSeconds(2));
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    file_reader.Expects.One.MethodWith(x => x.OpenStream());
                    file_reader.Expects.One.MethodWith(x => x.CloseStream());
                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
                file_reader.Expects.One.MethodWith(x => x.Dispose());
            }

            //проверяем что берем все из индекса

            mockFactory.VerifyAllExpectationsHaveBeenMet();
            mockFactory.ClearException();

            file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);

            using (var target = new FileStorageWithIndex(false, file_reader.MockObject))
            {

                {
                    //file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
                    //file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn(full_file_name);
                    var info = target.GetWorkInfo();
                    Assert.IsFalse(info.IsNewFile);
                    Assert.AreEqual(1, info.CountRecords);
                    Assert.AreEqual(full_file_name, info.FileName);
                    Assert.AreEqual(20, info.SizeInBytes);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.StartTime);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.FinishTime);
                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2.AddSeconds(2));
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    file_reader.Expects.One.MethodWith(x => x.OpenStream());
                    file_reader.Expects.One.MethodWith(x => x.CloseStream());
                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2.AddSeconds(2));
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    file_reader.Expects.One.MethodWith(x => x.OpenStream());
                    file_reader.Expects.One.MethodWith(x => x.CloseStream());

                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).Will(Throw.Exception(new Exception()));

                    bool bl = false;
                    try
                    {
                        target.ProcessSearchRequest(request.MockObject);
                    }
                    catch (Exception)
                    {
                        bl = true;
                    }
                    Assert.IsTrue(bl);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2.AddSeconds(2));
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                file_reader.Expects.One.MethodWith(x => x.Dispose());
            }





        }


        [TestMethod]
        public void NewIndexWithTwoRecord()
        {
            DirectoryStorageTest.ClearTestDirectory("NewIndexWithTwoRecord");
            string directory = DirectoryStorageTest.GetTestDirectory("NewIndexWithTwoRecord");

            MockFactory mockFactory = new MockFactory();


            DateTime t1 = DateTime.Now.AddSeconds(-10);

            DateTime t2 = t1.AddSeconds(1);
            DateTime t3 = t2.AddSeconds(1);


            //time.Expects.One.GetProperty(x => x.UTCNow).WillReturn(t1);
            var file_reader = mockFactory.CreateMock<IFileStorageReaderAndWriter>();

            string full_file_name = Path.Combine(directory, "data.dat");
            string full_index_name = Path.Combine(directory, "data.idx");
            file_reader.Expects.One.MethodWith(x => x.OpenStream());
            var target = new FileStorageWithIndex(true, file_reader.MockObject);
            

                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();

                {

                    file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
                    file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn(full_file_name);
                    var info = target.GetWorkInfo();
                    Assert.IsTrue(info.IsNewFile);
                    Assert.AreEqual(0, info.CountRecords);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
                target.AddDataToIndex(20, t2, 30, 40);
                target.AddDataToIndex(22, t3, 32, 40);

                {

                    file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
                    file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn(full_file_name);
                    var info = target.GetWorkInfo();
                    Assert.IsTrue(info.IsNewFile);
                    Assert.AreEqual(2, info.CountRecords);
                    Assert.AreEqual(full_file_name, info.FileName);
                    Assert.AreEqual(20, info.SizeInBytes);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.StartTime);
                    Assert.AreEqual(t3, info.SavedTimeRangeUTC.FinishTime);
                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();

                    Assert.AreEqual(t2, target.StartRange);
                    Assert.AreEqual(t3, target.FinishRange);
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t1);
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();

                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));


                    RecordDataItem dt2 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(22)).WillReturn(dt2);

                    request.Expects.One.MethodWith(x => x.Add(dt2));

                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }




                
                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(32, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(22)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(32, 0);
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(22)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));


                    RecordDataItem dt2 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt2);

                    request.Expects.One.MethodWith(x => x.Add(dt2));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    var source_ids = new Dictionary<ushort, byte>();
                    //source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);
                    request.Expects.One.MethodWith(x => x.Add(dt1));

                    RecordDataItem dt2 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(22)).WillReturn(dt2);
                    request.Expects.One.MethodWith(x => x.Add(dt2));

                    target.ProcessSearchRequest(request.MockObject);
                    
                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    var source_ids = new Dictionary<ushort, byte>();
                    //source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(50, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                file_reader.Expects.One.MethodWith(x => x.CloseStream());
                file_reader.Expects.AtLeastOne.GetProperty(x => x.FileName).WillReturn(full_file_name);
                Assert.IsFalse(File.Exists(full_index_name));
                target.StopWritingDataToFile();
                Assert.IsTrue(File.Exists(full_index_name));
                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();
                {

                    file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
                    //file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn(full_file_name);
                    var info = target.GetWorkInfo();
                    Assert.IsFalse(info.IsNewFile);
                    Assert.AreEqual(2, info.CountRecords);
                    Assert.AreEqual(full_file_name, info.FileName);
                    Assert.AreEqual(20, info.SizeInBytes);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.StartTime);
                    Assert.AreEqual(t3, info.SavedTimeRangeUTC.FinishTime);
                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                mockFactory.VerifyAllExpectationsHaveBeenMet();
                mockFactory.ClearException();




                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3.AddSeconds(1));
                    var source_ids = new Dictionary<ushort, byte>();
                    //source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    var typeIds = new Dictionary<byte, byte>();
                    typeIds.Add(40, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(typeIds);

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);
                    request.Expects.One.MethodWith(x => x.Add(dt1));

                    RecordDataItem dt2 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(22)).WillReturn(dt2);
                    request.Expects.One.MethodWith(x => x.Add(dt2));

                    file_reader.Expects.One.MethodWith(x => x.OpenStream());
                    file_reader.Expects.One.MethodWith(x => x.CloseStream());

                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

            
            file_reader.Expects.One.MethodWith(x => x.Dispose());
            //проверяем что берем все из индекса
            target.Dispose();


            mockFactory.VerifyAllExpectationsHaveBeenMet();
            mockFactory.ClearException();

            file_reader = mockFactory.CreateMock<IFileStorageReaderAndWriter>();

            file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
            file_reader.Expects.AtLeastOne.GetProperty(x => x.FileName).WillReturn(full_file_name);
            target = new FileStorageWithIndex(false, file_reader.MockObject);

            mockFactory.VerifyAllExpectationsHaveBeenMet();
            mockFactory.ClearException();
            file_reader.ClearExpectations();
                {
                    //file_reader.Expects.One.GetProperty(x => x.FileSize).WillReturn(20);
                    file_reader.Expects.One.GetProperty(x => x.FileName).WillReturn(full_file_name);
                    var info = target.GetWorkInfo();
                    Assert.IsFalse(info.IsNewFile);
                    Assert.AreEqual(2, info.CountRecords);
                    Assert.AreEqual(full_file_name, info.FileName);
                    Assert.AreEqual(20, info.SizeInBytes);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.StartTime);
                    Assert.AreEqual(t3, info.SavedTimeRangeUTC.FinishTime);
                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));

                    file_reader.Expects.One.MethodWith(x => x.OpenStream());
                    file_reader.Expects.One.MethodWith(x => x.CloseStream());


                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));


                    RecordDataItem dt2 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(22)).WillReturn(dt2);

                    request.Expects.One.MethodWith(x => x.Add(dt2));

                    file_reader.Expects.One.MethodWith(x => x.OpenStream());
                    file_reader.Expects.One.MethodWith(x => x.CloseStream());


                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }





                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    var source_ids = new Dictionary<ushort, byte>();
                    source_ids.Add(30, 0);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds).WillReturn(source_ids);
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    file_reader.Expects.One.MethodWith(x => x.GetDbDataRecord(20)).WillReturn(dt1);

                    request.Expects.One.MethodWith(x => x.Add(dt1));


                    file_reader.Expects.One.MethodWith(x => x.OpenStream());
                    file_reader.Expects.One.MethodWith(x => x.CloseStream());


                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

                file_reader.Expects.One.MethodWith(x => x.Dispose());
            target.Dispose();





        }


        class MockFileStorageReaderAndWriter : IFileStorageReaderAndWriter
        {
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public void OpenStream()
            {

            }

            public void CloseStream()
            {

            }

            public RecordDataItem GetDbDataRecord(long recordPotion)
            {
                return GetDbDataRecordFunc(recordPotion);
            }

            public Func<long, RecordDataItem> GetDbDataRecordFunc { get; set; }


            public void ScanFileAndFillIndex(IFileStorageIndex index)
            {
                ScanFileAndFillIndexAction(index);
            }

            public Action<IFileStorageIndex> ScanFileAndFillIndexAction { get; set; }

            public bool WriteRecord(ushort sourceId, byte dataTypeId, byte[] data, IFileStorageIndex index)
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {

            }
        }

        [TestMethod]
        public void RestoreIndex()
        {
            DirectoryStorageTest.ClearTestDirectory("NewIndexWithTwoRecord");
            string directory = DirectoryStorageTest.GetTestDirectory("NewIndexWithTwoRecord");

            MockFactory mockFactory = new MockFactory();


            DateTime t1 = DateTime.Now.AddSeconds(-10);

            DateTime t2 = t1.AddSeconds(1);
            DateTime t3 = t2.AddSeconds(1);


            //time.Expects.One.GetProperty(x => x.UTCNow).WillReturn(t1);
            MockFileStorageReaderAndWriter file_reader = new MockFileStorageReaderAndWriter();

            string full_file_name = Path.Combine(directory, "data.dat");
            string full_index_name = Path.Combine(directory, "data.idx");

            file_reader.ScanFileAndFillIndexAction = (idx) =>
            {
                idx.AddDataToIndex(20, t2, 30, 40);
                idx.AddDataToIndex(22, t3, 32, 40);
            };
            file_reader.FileName = full_file_name;
            file_reader.FileSize = 10;

            Assert.IsFalse(File.Exists(full_index_name));

            using (var target = new FileStorageWithIndex(false, file_reader)) //восстанавливам индекс
            {
                Assert.IsTrue(File.Exists(full_index_name));

                {
                    var info = target.GetWorkInfo();
                    Assert.IsFalse(info.IsNewFile);
                    Assert.AreEqual(2, info.CountRecords);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.StartTime);
                    Assert.AreEqual(t3, info.SavedTimeRangeUTC.FinishTime);

                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t1);
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();

                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();

                    file_reader.GetDbDataRecordFunc = (pos) =>
                    {
                        if (pos == 20)
                        {
                            return dt1;
                        }
                        throw new Exception();
                    };
                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    RecordDataItem dt2 = new RecordDataItem();

                    file_reader.GetDbDataRecordFunc = (pos) =>
                    {
                        if (pos == 20)
                        {
                            return dt1;
                        }
                        if (pos == 22)
                        {
                            return dt2;
                        }
                        throw new Exception();
                    };


                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    request.Expects.One.MethodWith(x => x.Add(dt2));

                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
            }

            file_reader = new MockFileStorageReaderAndWriter();
            file_reader.FileName = full_file_name;
            Assert.IsTrue(File.Exists(full_index_name));

            //чтитывем данные из индекса
            using (var target = new FileStorageWithIndex(false, file_reader))
            {
                Assert.IsTrue(File.Exists(full_index_name));

                {
                    var info = target.GetWorkInfo();
                    Assert.IsFalse(info.IsNewFile);
                    Assert.AreEqual(2, info.CountRecords);
                    Assert.AreEqual(t2, info.SavedTimeRangeUTC.StartTime);
                    Assert.AreEqual(t3, info.SavedTimeRangeUTC.FinishTime);

                }


                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t1);
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();

                }

                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t1);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();

                    file_reader.GetDbDataRecordFunc = (pos) =>
                    {
                        if (pos == 20)
                        {
                            return dt1;
                        }
                        throw new Exception();
                    };
                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }
                {
                    var request = mockFactory.CreateMock<ISearchRequestData>();
                    request.Expects.AtLeastOne.GetProperty(x => x.StartSearchRange).WillReturn(t2);
                    request.Expects.AtLeastOne.GetProperty(x => x.FinishSearchRange).WillReturn(t3);
                    request.Expects.AtLeastOne.GetProperty(x => x.SearchSourceIds)
                        .WillReturn(new Dictionary<ushort, byte>());
                    request.Expects.AtLeastOne.GetProperty(x => x.TypeDataIds).WillReturn(new Dictionary<byte, byte>());

                    RecordDataItem dt1 = new RecordDataItem();
                    RecordDataItem dt2 = new RecordDataItem();

                    file_reader.GetDbDataRecordFunc = (pos) =>
                    {
                        if (pos == 20)
                        {
                            return dt1;
                        }
                        if (pos == 22)
                        {
                            return dt2;
                        }
                        throw new Exception();
                    };


                    request.Expects.One.MethodWith(x => x.Add(dt1));
                    request.Expects.One.MethodWith(x => x.Add(dt2));

                    target.ProcessSearchRequest(request.MockObject);

                    mockFactory.VerifyAllExpectationsHaveBeenMet();
                    mockFactory.ClearException();
                }

            }





        }

    }
}