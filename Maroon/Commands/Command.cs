using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon
{
    public abstract class Command
    {
        Guid CommandID { get; set; }
        DateTime Timestamp { get; set; }
    }
}
