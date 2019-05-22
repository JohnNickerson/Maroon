using System;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Mappers.Csv
{
    public class LedgerCsvMapper : CsvDiskMapper<AccountTransfer>
    {
        public LedgerCsvMapper(string filename) : base(filename)
        {
        }

        public override string FieldsHeader => "Date,FromAccount,ToAccount,Notes,Name,Amount,ID,Rev,Hash";

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
                RevisionGuid = Guid.Parse(tokens[7]),
                ImportHash = tokens.Length > 8 ? tokens[8] : null
            };
        }

        public override string ToCsv(AccountTransfer obj)
        {
            return $"{obj.Date:yyyy-MM-dd},{obj.FromAccount},{obj.ToAccount},{obj.Description},{obj.Category},{obj.Amount},{obj.ID},{obj.RevisionGuid},{obj.ImportHash}";
        }
    }
}
