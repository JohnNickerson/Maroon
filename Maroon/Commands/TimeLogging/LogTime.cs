using AssimilationSoftware.Maroon.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.TimeLogging
{
    public class LogTime : Command
    {
        public TimeLogEntry NewEntry { get; set; }
    }
}
