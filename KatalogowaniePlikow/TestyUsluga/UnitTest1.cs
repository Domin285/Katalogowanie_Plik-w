using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using Usluga;

namespace TestyUsluga
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCreateLog()
        {
            //wyjątek, bo na ścieżce istnieje już dziennik
            Assert.ThrowsException<ArgumentException>(() =>
            {
                EventLog.CreateEventSource("C:/Users/dyplo/source/repos/KatalogowaniePlikow/Usluga", "Zasoby");
            });
        }
    }
}
