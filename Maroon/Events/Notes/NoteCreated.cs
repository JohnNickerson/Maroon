using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Notes;

namespace AssimilationSoftware.Maroon.Events.Notes
{
    public class NoteCreated : Event
    {
        public Note NewNote { get; set; }
    }
}
