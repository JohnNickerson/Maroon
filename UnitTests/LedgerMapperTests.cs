using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class LedgerMapperTests
    {
        [TestInitialize]
        public void Setup()
        {
            foreach (var updateFile in Directory.GetFiles(".", "update*.xml"))
            {
                File.Delete(updateFile);
            }
            foreach (var updateFile in Directory.GetFiles(".", "*.csv"))
            {
                File.Delete(updateFile);
            }
        }

        [TestMethod]
        public void Round_Trip_Test()
        {
            var ledger = new List<AccountTransfer>
            {
                new AccountTransfer
                {
                    ID = Guid.NewGuid(),
                    RevisionGuid = Guid.NewGuid(),
                    LastModified = DateTime.Now,
                    Amount = 69,
                    Category = "test",
                    Date = DateTime.Today,
                    Description = "A test record",
                    FromAccount = "FromTest",
                    ToAccount = "ToTest"
                }
            };
            var filename = "TestLedger.csv";
            var mapper = new LedgerCsvMapper(filename);
            mapper.SaveAll(ledger);
            var fromDisk = mapper.LoadAll();

            Assert.IsNotNull(fromDisk);
            Assert.AreEqual(ledger.Count, fromDisk.Count());
        }
    }
}
