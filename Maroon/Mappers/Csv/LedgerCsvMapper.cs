using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Mappers.Csv
{
    public class LedgerCsvMapper : CsvDiskMapper<AccountTransfer>
    {
        public LedgerCsvMapper(string filename) : base(filename)
        {
        }

        public override string FieldsHeader => "Date,FromAccount,ToAccount,Notes,Name,Amount,ID,Rev";

        public override AccountTransfer FromTokens(string[] tokens)
        {
            return new AccountTransfer
            {
                Date = DateTime.Parse(tokens[0]),
                FromAccount = tokens[1],
                ToAccount = tokens[2],
                Description = tokens[3],
                Category = tokens[4],
                Amount = decimal.Parse(tokens[5]),
                ID = new Guid(tokens[6]),
                Revision = int.Parse(tokens[7])
            };
        }

        public override string ToCsv(AccountTransfer obj)
        {
            return string.Format("{0:yyyy-MM-dd},{1},{2},{3},{4},{5},{6},{7}", obj.Date, obj.FromAccount, obj.ToAccount,
                obj.Description, obj.Category, obj.Amount, obj.ID, obj.Revision);
        }
    }
}
