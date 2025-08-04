using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using Xunit;

namespace UnitTests
{
    
    public class LedgerMapperTests
    {
        [Obsolete("Use file system abstraction")]
        private void Cleanup()
        {
            foreach (var updateFile in Directory.GetFiles(".", "*.csv"))
            {
                File.Delete(updateFile);
            }
        }

        [Fact]
        public void Round_Trip_Test()
        {
            Cleanup();
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
            var mapper = new LedgerCsvMapper();
            mapper.Write(ledger, filename);
            var fromDisk = mapper.Read(filename);

            Assert.NotNull(fromDisk);
            Assert.Equal(ledger.Count, fromDisk.Count());
            Cleanup();
        }
    }
}
