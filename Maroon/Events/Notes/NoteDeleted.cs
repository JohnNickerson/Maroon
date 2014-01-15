using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Events.Notes
{
    public class NoteDeleted : Event
    {
        public Guid NoteID { get; set; }
    }
}
