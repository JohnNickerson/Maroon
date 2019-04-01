using System;
using System.Collections.Generic;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Interfaces
{
    public interface IMergeRepository<T> : IRepository<T> where T : ModelObject
    {
        /// <summary>
        /// Writes pending changes into the baseline collection.
        /// </summary>
        /// <returns>Number of changes committed.</returns>
        int CommitChanges();

        /// <summary>
        /// Gets a list containing sets of conflicting edits.
        /// </summary>
        /// <returns></returns>
        /// <remarks>A conflict here is defined as two or more updates or deletes to the same version (ie revision number) of the same object.</remarks>
        List<PendingChange<T>> FindConflicts();

        void ResolveConflict(T item);

        void ResolveByDelete(Guid id);

        void Revert(Guid id);

        /// <summary>
        /// Get a full list of pending changes yet to be committed.
        /// </summary>
        /// <returns>A list of all edits yet to be committed to base storage.</returns>
        List<PendingChange<T>> GetPendingChanges();
    }
}
