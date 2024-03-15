using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using KatalogowaniePlikow;

namespace TestyWPF
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestConvertToBytes()
        {
            string type = "KB";
            long size = 2048;

            Assert.AreEqual(size, MainWindow.ConvertToBytes(2, type));
        }

        [TestMethod]
        public void TestConvertToBytes2()
        {
            string type = "NANO";

            bool isArgumentExceptionThrown = false;
            try
            {
                MainWindow.ConvertToBytes(4, type);
            }
            catch (ArgumentException)
            {
                isArgumentExceptionThrown = true;
            }

            Assert.IsTrue(isArgumentExceptionThrown, "Metoda nie zgłosiła wyjątku ArgumentException.");
        }

    }
}
