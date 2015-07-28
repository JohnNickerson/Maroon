using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Notes
{
    public class MergeNotes : Command
    {
        public Guid MergeWinnerId { get; set; }
        public Guid MergeLoserId { get; set; }
    }
}
