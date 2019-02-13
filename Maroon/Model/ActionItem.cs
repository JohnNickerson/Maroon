using System;
using System.Collections.Generic;

namespace AssimilationSoftware.Maroon.Model
{
    public class ActionItem
    {
        public ActionItem()
        {
            Notes = new List<string>();
            Tags = new Dictionary<string, string>();
        }
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Context { get; set; }
        public string Status { get; set; }
        public List<string> Notes { get; set; }
        public DateTime? DoneDate { get; set; }
        public DateTime? TickleDate { get; set; }
        public Dictionary<string, string> Tags { get; set; }

        public int Revision { get; set; }
        public ActionItem Parent { get; set; }
        public ActionItem Project { get; set; }
        public int RankDepth { get; set; }
    }
}
