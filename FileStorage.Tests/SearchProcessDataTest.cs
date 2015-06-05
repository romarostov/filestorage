using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileStorage.Tests
{
    [TestClass]
    public class SearchProcessDataTest
    {
        [ExpectedException(typeof (InvalidDataException))]
        [TestMethod]
        public void ErrorDataRange()
        {
            var t1 = DateTime.Now;
            new SearchProcessData(t1, t1.AddMilliseconds(-1), null, null, 20);
        }

        [ExpectedException(typeof(InvalidDataException))]
        [TestMethod]
        public void EmptyArgument1()
        {
            var t1 = DateTime.Now;
            new SearchProcessData(t1, t1.AddMilliseconds(-1), null, null, 20);
        }
    }
}