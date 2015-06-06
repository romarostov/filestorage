using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Interfaces;

namespace FileStorage.Tests
{
    [TestClass]
    public class SearchRequestDataTest
    {
        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void ErrorDataRange()
        {
            var t1 = DateTime.Now;
            new SearchRequestData(t1, t1.AddMilliseconds(-1), null, null, 20);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void ErrorMaximumDataRange()
        {
            var t1 = DateTime.Now;
            new SearchRequestData(t1, t1.AddMilliseconds(1), null, null,0);
        }

        [TestMethod]
        public void SimpleProcess()
        {
            var t1 = DateTime.Now;
            var target=new SearchRequestData(t1, t1, null, null, 10);
            Assert.AreEqual(t1, target.StartSearchRange);
            Assert.AreEqual(t1, target.FinishSearchRange);
            Assert.AreEqual(0, target.SearchSourceIds.Count);
            Assert.AreEqual(0, target.TypeDataIds.Count);

            bool bl = false;
            try
            {
                target.Add(null);
            }
            catch (ArgumentNullException)
            {
                bl = true;
            }
            Assert.IsTrue(bl);


            bl = false;
            try
            {
                target.Add(new RecordDataItem(){Time = t1.AddMilliseconds(-1),Data = new byte[]{20},DataTypeId = 20,SourceId = 30});
            }
            catch (InvalidOperationException)
            {
                bl = true;
            }
            Assert.IsTrue(bl);


            
            bl = false;
            try
            {
                target.Add(new RecordDataItem() { Time = t1, Data = null, DataTypeId = 20, SourceId = 30 });
            }
            catch (InvalidOperationException)
            {
                bl = true;
            }
            Assert.IsTrue(bl);


            bl = false;
            try
            {
                target.Add(new RecordDataItem() { Time = t1, Data = new byte[]{}, DataTypeId = 20, SourceId = 30 });
            }
            catch (InvalidOperationException)
            {
                bl = true;
            }
            Assert.IsTrue(bl);

            Assert.AreEqual(0, target.Results.Count);
            var item = new RecordDataItem() {Time = t1, Data = new byte[] {10}, DataTypeId = 20, SourceId = 30};
            target.Add(item);
            
            Assert.AreEqual(1,target.Results.Count);
            Assert.AreEqual(item,target.Results[0]);

        }


        [TestMethod]
        public void SimpleProcess2()
        {
            var t1 = DateTime.Now;
            var t2 = DateTime.Now.AddHours(1);
            var target = new SearchRequestData(t1, t2, new List<ushort>(){20,30}, new List<byte>(){2,4}, 10);
            Assert.AreEqual(t1, target.StartSearchRange);
            Assert.AreEqual(t2, target.FinishSearchRange);
            Assert.AreEqual(2, target.SearchSourceIds.Count);
            Assert.IsTrue(target.SearchSourceIds.ContainsKey(20));
            Assert.IsTrue(target.SearchSourceIds.ContainsKey(30));
            Assert.AreEqual(2, target.TypeDataIds.Count);
            Assert.IsTrue(target.TypeDataIds.ContainsKey(2));
            Assert.IsTrue(target.TypeDataIds.ContainsKey(4));
            

            

            Assert.AreEqual(0, target.Results.Count);
            var item1 = new RecordDataItem() { Time = t1, Data = new byte[] {1,2,3,4 }, DataTypeId = 20, SourceId = 30 };
            target.Add(item1);
            Assert.AreEqual(1, target.Results.Count);
            Assert.AreEqual(item1, target.Results[0]);

            var item2 = new RecordDataItem() { Time = t1.AddSeconds(1), Data = new byte[] { 5,6,7,8,9,10 }, DataTypeId = 20, SourceId = 30 };
            target.Add(item2);
            Assert.AreEqual(2, target.Results.Count);
            Assert.AreEqual(item1, target.Results[0]);
            Assert.AreEqual(item2, target.Results[1]);


            bool bl = false;//запрос превышаем максимальное количество байт в ответе
            try
            {
                item1 = new RecordDataItem() { Time = t1.AddSeconds(1), Data = new byte[] { 11 }, DataTypeId = 20, SourceId = 30 };
                target.Add(item1);
            }
            catch (InvalidDataException)
            {
                bl = true;
            }
            Assert.IsTrue(bl);

        }
    }
}