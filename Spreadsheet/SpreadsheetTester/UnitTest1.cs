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
        public void TestSetDouble()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetCellContents("A1", 55);
            ss.SetCellContents("A2", 55);
            Assert.AreEqual((double)55, ss.GetCellContents("A1"));
        }

        [TestMethod]
        public void TestSetFormula()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetCellContents("A1", new Formula("55+10"));
            Assert.AreEqual(new Formula("55+10").ToString(), ss.GetCellContents("A1").ToString());
        }

        [TestMethod]
        public void TestSetString()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetCellContents("A1", "A2");
            Assert.AreEqual("A2", ss.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullContent()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetCellContents("A1", null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestNullName()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetCellContents(null, 55);
        }

        [TestMethod]
        public void TestNonEmpty()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetCellContents("A1", new Formula("a+b"));
            ss.SetCellContents("A2", 55);
            HashSet<string> set = new HashSet<string>();
            foreach (string s in ss.GetNamesOfAllNonemptyCells())
                set.Add(s);
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidName()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetCellContents("1A", 55);
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircular()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ISet<string> set = ss.SetCellContents("A1", new Formula("A1+A2"));
        }
        /*
        [TestMethod]
        public void SetCellContentsDependence()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ISet<string> set = ss.SetCellContents("A1", new Formula("5+6"));
            ISet<string> set1 = ss.SetCellContents("A2", new Formula("8+A1"));
            Assert.AreEqual(0, set1.Count);
        }*/
    }
}
