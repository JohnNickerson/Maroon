using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using Xunit;

namespace UnitTests
{
    
    public class LedgerMapperTests
    {
        [Fact]
        public void Round_Trip_Test()
        {
            var mockFileSystem = new MockFileSystem();
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
            var mapper = new LedgerCsvMapper(mockFileSystem);
            mapper.Write(ledger, filename);
            var fromDisk = mapper.Read(filename);

            Assert.NotNull(fromDisk);
            Assert.Equal(ledger.Count, fromDisk.Count());
        }
    }
}
