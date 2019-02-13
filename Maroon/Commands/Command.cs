using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Commands
{
    [Obsolete]
    public abstract class Command : ModelObject // Just to keep things going for a while.
    {
        public Command()
        {
            CommandID = Guid.NewGuid();
            Timestamp = DateTime.Now;
            Accepted = false;
        }

        public Guid CommandID { get; set; }
        public DateTime Timestamp { get; set; }
        public int SourceRevision { get; set; }
        public bool Accepted { get; set; }
        // RecordID?
    }
}
