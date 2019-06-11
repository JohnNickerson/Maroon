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
        /// True if at least one more item can be returned. False otherwise.
        /// </summary>
        bool HasNext();

        /// <summary>
        /// Returns the next item in the source, or null if not present, with its ImportHash field set.
        /// </summary>
        T GetNext();
    }
}
