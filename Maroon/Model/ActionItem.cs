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
            Notes = new List<string>();
            Tags = new Dictionary<string, string>();
        }
        #endregion

        #region Methods

        public ActionItem GetProject(IRepository<ActionItem> repository)
        {
            return ProjectId.HasValue ? repository.Find(ProjectId.Value) : null;
        }

        public ActionItem GetParent(IRepository<ActionItem> repository)
        {
            return ParentId.HasValue ? repository.Find(ParentId.Value) : null;
        }

        public int GetRankDepth(IRepository<ActionItem> repository)
        {
            if (ParentId == null) return 0;
            // Recursive version would be simpler ("return Parent.RankDepth + 1;") but can get stuck on loops.
            var ancestors = new List<ActionItem> {GetParent(repository)};
            var cursor = GetParent(repository)?.GetParent(repository);
            while (cursor != null && !ancestors.Contains(cursor))
            {
                ancestors.Add(cursor);
                cursor = cursor.GetParent(repository);
            }

            return ancestors.Count;
        }
        
        public override ModelObject Clone()
        {
            return new ActionItem
            {
                Context = Context,
                Done = Done,
                DoneDate = DoneDate,
                Notes = Notes,
                ParentId = ParentId,
                ProjectId = ProjectId,
                Status = Status,
                Tags = Tags,
                TickleDate = TickleDate,
                Title = Title,
                Upvotes = Upvotes,
                RevisionGuid = RevisionGuid,
                PrevRevision = PrevRevision,
                IsDeleted = IsDeleted,
                ID = ID,
                LastModified = LastModified
            };
        }

        #endregion

        #region Properties
        public string Title { get; set; }
        public string Context { get; set; }
        public string Status { get; set; }
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
