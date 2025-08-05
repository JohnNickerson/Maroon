using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Xunit;

namespace UnitTests
{
    
    public class MergeDiskLedgerTests
    {
        [Fact]
        public void Resolve_By_Edit()
        {
            var filename = @"spending_new.csv";
            var mapper = new LedgerCsvMapper();
            var path = Path.GetDirectoryName(filename);
            var repo = new MergeDiskRepository<AccountTransfer>(mapper, filename);
        }
    }
}
