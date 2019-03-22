using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Model
{
    public class Conflict<T> where T : ModelObject
    {
        public Guid Id { get; set; }
        public List<T> Updates { get; set; }
        public List<T> Deletes { get; set; }
    }
}
