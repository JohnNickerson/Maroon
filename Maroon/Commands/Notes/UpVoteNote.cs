using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Notes
{
    public class UpVoteNote : Command
    {
        public Guid ChildNoteID { get; set; }
        public Guid ParentNoteId { get; set; }
    }
}
