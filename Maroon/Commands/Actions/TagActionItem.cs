using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Actions
{
    public class TagActionItem : Command
    {
        public Guid ActionId { get; set; }
        public Dictionary<string, string> Tags { get; set; }
    }
}
