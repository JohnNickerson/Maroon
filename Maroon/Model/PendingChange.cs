using System;
using System.Collections.Generic;
using System.Linq;
using AssimilationSoftware.Maroon.Annotations;

namespace AssimilationSoftware.Maroon.Model
{
    public class PendingChange<T> where T : ModelObject
    {
        public PendingChange()
        {
            Updates = new Dictionary<Guid, T>();
        }

        public PendingChange(T item) : this()
        {
            OriginalVersion = item;
            Updates[item.RevisionGuid] = item;
        }

        public void AddRevision(T item)
        {
            Updates[item.RevisionGuid] = item;
            Validate();
        }

        private void Validate()
        {
            IsConflict = false;
            // 1. Maximum 1 item with a null previous revision ID
            // 2. No two matching previous revision IDs.
            IsConflict |= Updates.Count != Updates.Values.Select(tem => tem.PrevRevision).Distinct().Count();
            // 3. No unknown previous revision IDs
            IsConflict |= Updates.Values.Any(tem => tem.PrevRevision.HasValue && !Updates.Keys.ToList().Contains(tem.PrevRevision.Value));
        }

        public Guid Id { [UsedImplicitly] get; set; }
        public bool IsConflict { get; private set; }
        public Dictionary<Guid, T> Updates { get; }
        public T OriginalVersion { get; set; }
    }
}
