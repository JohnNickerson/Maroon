using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Budget
{
    public class DeleteSpending : Command
    {
        public Guid RecordId { get; set; }
    }
}
