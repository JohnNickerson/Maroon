using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MergeDiskLedgerTests
    {
        [TestMethod]
        public void Resolve_By_Edit()
        {
            var filename = @"D:\Temp\BudgetTracker Testing\spending_new.csv";
            var mapper = new LedgerCsvMapper(filename);
            var path = Path.GetDirectoryName(filename);
            var repo = new MergeDiskRepository<AccountTransfer>(mapper, path);
        }
    }
}
