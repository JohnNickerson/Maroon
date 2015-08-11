using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Actions
{
    public class UpVoteActionItem : Command
    {
        public Guid ParentId { get; set; }
        public Guid ChildId { get; set; }
    }
}
