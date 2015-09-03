using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.TimeLogging
{
    /// <summary>
    /// Switches off the "billable" flag for a given time log entry.
    /// </summary>
    public class CompTime : Command
    {
        public Guid TimeLogEntryID { get; set; }
    }
}
