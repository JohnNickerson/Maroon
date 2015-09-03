using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Notes
{
    public class EditNote : Command
    {
        public Guid NoteID { get; set; }
        public string NewText { get; set; }
    }
}
