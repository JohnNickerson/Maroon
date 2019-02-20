using System;
using System.Collections.Generic;

namespace AssimilationSoftware.Maroon.Model
{
    public class ActionItem : ModelObject
    {
        public ActionItem()
        {
            Notes = new List<string>();
            Tags = new Dictionary<string, string>();
        }
        public string Title { get; set; }
        public string Context { get; set; }
        public string Status { get; set; }
        public List<string> Notes { get; set; }
        public DateTime? DoneDate { get; set; }
        public bool Done { get; set; }
        public DateTime? TickleDate { get; set; }
        public Dictionary<string, string> Tags { get; set; }

        public ActionItem Parent { get; set; }
        public ActionItem Project { get; set; }

        public int RankDepth
        {
            get
            {
                if (Parent == null) return 0;
                // Recursive version would be simpler ("return Parent.RankDepth + 1;") but can get stuck on loops.
                var ancestors = new List<ActionItem> { Parent };
                var cursor = Parent.Parent;
                while (cursor != null && !ancestors.Contains(cursor))
                {
                    ancestors.Add(cursor);
                    cursor = cursor.Parent;
                }
                return ancestors.Count;
            }
        }

        public int Upvotes { get; set; }
    }
}
