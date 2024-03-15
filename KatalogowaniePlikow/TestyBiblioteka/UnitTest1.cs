using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Biblioteka;

namespace TestyBiblioteka
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestDeleteDirectoryException()
        {
            Class1 class1 = new Class1();
            Assert.ThrowsException<DirectoryNotFoundException>(() => class1.DeleteDirectory("C:1234%#2"));
        }

        [TestMethod]
        public void TestDeleteDirectory() 
        { 
            Class1 class2 = new Class1();
            Assert.ThrowsException<IOException>(() => class2.AddFile("D:", "D:/pusty.txt"));
        }
    }
}
