using System;

namespace AssimilationSoftware.Maroon.Model
{
    public abstract class ModelObject
    {
        /// <summary>
        /// Unique identifier for this item.
        /// </summary>
        public Guid ID { get; set; }

        public ModelObject() { }

        public override bool Equals(object obj)
        {
            return obj is ModelObject other && other.ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
