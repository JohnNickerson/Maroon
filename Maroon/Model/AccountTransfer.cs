using System;

namespace AssimilationSoftware.Maroon.Model
{
    public class AccountTransfer
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
        /// A globally unique identifier for the transfer.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// The source account from which the transfer originated.
        /// </summary>
        public string FromAccount { get; set; }

        /// <summary>
        /// The destination account to which the transfer was sent.
        /// </summary>
        public string ToAccount { get; set; }
    }
}
