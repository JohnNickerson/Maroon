using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Notes
{
    public class Note
    {
        public Guid ID { get; set; }
        public string Text { get; set; }
        public List<string> Tags { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid ParentId { get; set; }
        public int Revision { get; set; }
    }
}
