using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Actions
{
    public class DeferActionItem : Command
    {
        public Guid ActionId { get; set; }
        public DateTime? TickleDate { get; set; }
    }
}
