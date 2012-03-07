using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Notes
{
    public class DeleteNote : Command
    {
        public Guid NoteID { get; set; }
    }
}
