using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Budget
{
    public class RenameSpending : Command
    {
        public Guid RecordId { get; set; }
        public string NewName { get; set; }
        public string NewCategory { get; set; }
    }
}
