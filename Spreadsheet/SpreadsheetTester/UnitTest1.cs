using Microsoft.VisualStudio.TestTools.UnitTesting;
using Formulas;
using SS;
using System;
using System.Collections.Generic;

namespace SpreadsheetTester
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            Spreadsheet sheet = new Spreadsheet();
            Assert.AreEqual(sheet.Changed, false);
        }
    }
}
