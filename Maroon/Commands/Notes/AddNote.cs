using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Notes
{
    public class AddNote : Command
    {
        public Guid ID { get; set; }
        public string Text { get; set; }
        public List<string> Tags { get; set; }
    }
}
