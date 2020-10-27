using System;
using System.Collections.Generic;
using System.Linq;
using AssimilationSoftware.Maroon.Interfaces;

namespace AssimilationSoftware.Maroon.Model
{
    public class Note : ModelObject
    {
        #region Methods

        public Note GetParent(IRepository<Note> repository)
        {
            return ParentId == null ? null : repository.Find(ParentId.Value);
        }

        #endregion

        #region Properties
        public string Text { get; set; }
        public List<string> Tags { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? ParentId { get; set; }
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
            set => Tags = value.Replace("#", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        #endregion
    }
}
