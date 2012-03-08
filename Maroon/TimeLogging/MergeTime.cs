using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.TimeLogging
{
    public class MergeTime : Command
    {
        public List<Guid> TimeLogEntryIDs { get; set; }
    }
}
