using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Objects
{
    public class Note
    {
        public Guid ID { get; set; }
        public string Text { get; set; }
        public List<string> Tags { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? ParentId { get; set; }
        public int Revision { get; set; }
        public string TagString { get
            {
                if (Tags == null || Tags.Count == 0)
                {
                    return string.Empty;
                }
                return string.Join(" ", Tags.Select(t => string.Format("#{0}", t)));
            } }
    }
}
