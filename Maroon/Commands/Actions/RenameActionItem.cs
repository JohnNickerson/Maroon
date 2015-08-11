using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Actions
{
    public class RenameActionItem : Command
    {
        public Guid ActionId { get; set; }
        public string Title { get; set; }
    }
}
