using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Actions
{
    public class AddActionItem : Command
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Context { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public string Note { get; set; }
    }
}
