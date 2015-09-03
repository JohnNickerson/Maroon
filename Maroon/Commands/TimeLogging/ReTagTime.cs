using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.TimeLogging
{
    public class ReTagTime : Command
    {
        public Guid TimeLogEntryID { get; set; }
        public string Project { get; set; }
        public string Client { get; set; }
    }
}
