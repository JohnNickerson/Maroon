using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Interfaces
{
    /// <summary>
    /// An interface for external, read-only entry sources, for importing.
    /// </summary>
    public interface ISourceReader<out T> where T : ModelObject
    {
        /// <summary>
        /// Reads all items in the source, with ImportHash fields set.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> ReadAll();
    }
}
