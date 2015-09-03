using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Notes
{
    public class TagNote : Command
    {
        public Guid NoteID { get; set; }
        public List<string> NewTags { get; set; }
    }
}
