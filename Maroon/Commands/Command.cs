using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon
{
    public abstract class Command
    {
        public Guid CommandID { get; set; }
        public DateTime Timestamp { get; set; }
        public int SourceRevision { get; set; }
        public bool Accepted { get; set; }
    }
}
