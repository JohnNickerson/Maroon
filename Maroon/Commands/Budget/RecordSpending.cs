using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Budget
{
    public class RecordSpending : Command
    {
        public Guid RecordId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool Income { get; set; }
    }
}
