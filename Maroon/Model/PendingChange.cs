using System;
using System.Collections.Generic;

namespace AssimilationSoftware.Maroon.Model
{
    public class PendingChange<T> where T : ModelObject
    {
        public Guid Id { get; set; }
        public bool IsConflict { get; set; }
        public List<T> Updates { get; set; }
        public List<T> Deletes { get; set; }
    }
}
