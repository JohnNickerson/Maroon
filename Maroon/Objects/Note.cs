using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Objects
{
    public class Note : ModelObject
    {
        public string Text { get; set; }
        public List<string> Tags { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? ParentId { get; set; }
        public Note Parent { get; set; }
        public int Revision { get; set; }
        public string TagString
        {
            get
            {
                if (Tags == null || Tags.Count == 0)
                {
                    return string.Empty;
                }
                return string.Join(" ", Tags.Select(t => $"#{t}"));
            }
            set => Tags = value.Replace("#", "").Split(' ').ToList();
        }
    }
}
