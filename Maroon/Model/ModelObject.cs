using System;

namespace AssimilationSoftware.Maroon.Model
{
    public abstract class ModelObject
    {
        /// <summary>
        /// Unique identifier for this item across all revisions.
        /// </summary>
        public Guid ID { get; set; }

        public DateTime LastModified { get; set; }

        public Guid? PrevRevision { get; set; }

        /// <summary>
        /// A unique ID assigned just to this revision of this object, used mostly for serialisation.
        /// </summary>
        public Guid RevisionGuid { get; set; }

        public bool IsDeleted { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ModelObject other && other.ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public void UpdateRevision()
        {
            PrevRevision = RevisionGuid;
            RevisionGuid = Guid.NewGuid();
            LastModified = DateTime.Now;
        }

        public abstract ModelObject Clone();
    }
}
