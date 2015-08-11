using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Actions
{
    public class MoveActionItem : Command
    {
        public Guid ActionId { get; set; }
        public string Context { get; set; }
    }
}
