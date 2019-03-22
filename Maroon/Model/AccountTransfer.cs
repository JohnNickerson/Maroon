using System;

namespace AssimilationSoftware.Maroon.Model
{
    public class AccountTransfer : ModelObject
    {
        /// <summary>
        /// The start date and time of the entry.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The category to which the fund transfer belongs.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The dollar amount of the transfer.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// A short description of the transfer.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The source account from which the transfer originated.
        /// </summary>
        public string FromAccount { get; set; }

        /// <summary>
        /// The destination account to which the transfer was sent.
        /// </summary>
        public string ToAccount { get; set; }

        public override object Clone()
        {
            return new AccountTransfer
            {
                Amount = Amount,
                Category = Category,
                Date = Date,
                Description = Description,
                FromAccount = FromAccount,
                ToAccount = ToAccount,
                RevisionGuid = RevisionGuid,
                Revision = Revision,
                ID = ID,
                LastModified = LastModified
            };
        }
    }
}
