using System;
using System.Collections.Generic;
using AssimilationSoftware.Maroon.Interfaces;

namespace AssimilationSoftware.Maroon.Model
{
    public class ActionItem : ModelObject
    {
        #region Constructors
        public ActionItem()
        {
            Notes = [];
            Tags = [];
            Title = string.Empty;
        }
        #endregion

        #region Methods

        // TODO: Refactor to repository.
        public ActionItem? GetProject(IRepository<ActionItem> repository)
        {
            return ProjectId.HasValue ? repository.Find(ProjectId.Value) : null;
        }

        public ActionItem? GetParent(IRepository<ActionItem> repository)
        {
            return ParentId.HasValue ? repository.Find(ParentId.Value) : null;
        }

        public int GetRankDepth(IRepository<ActionItem> repository)
        {
            if (ParentId == null) return 0;
            // Recursive version would be simpler ("return Parent.RankDepth + 1;") but can get stuck on loops.
            var ancestors = new List<ActionItem>();
            var parent = GetParent(repository);
            if (parent != null) ancestors.Add(parent);
            var cursor = GetParent(repository)?.GetParent(repository);
            while (cursor != null && !ancestors.Contains(cursor))
            {
                ancestors.Add(cursor);
                cursor = cursor.GetParent(repository);
            }

            return ancestors.Count;
        }

        #endregion

        #region Properties
        public string Title { get; set; }
        public string? Context { get; set; }
        public string? Status { get; set; }
        public List<string> Notes { get; set; }
        public DateTime? DoneDate { get; set; }
        public bool Done
        {
            get => DoneDate.HasValue;
            set
            {
                if (DoneDate.HasValue == value) return;
                DoneDate = value ? DateTime.Today : (DateTime?)null;
            }
        }
        public DateTime? TickleDate { get; set; }
        public Dictionary<string, string> Tags { get; set; }

        public Guid? ParentId { get; set; }

        public Guid? ProjectId { get; set; }

        public int Upvotes { get; set; }
        #endregion
    }
}
