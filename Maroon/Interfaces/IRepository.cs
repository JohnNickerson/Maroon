using System;
using System.Collections.Generic;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Interfaces
{
    public interface IRepository<T> where T : ModelObject
    {
        IEnumerable<T> Items { get; }

        T Find(Guid id);

        IEnumerable<T> FindAll();

        IEnumerable<List<T>> FindConflicts();

        void Create(T entity);

        void Delete(T entity);

        void Update(T entity);

        void Merge(T entity, Guid mergeId);

        [Obsolete("Save should be done automatically now.")]
        void SaveChanges(bool force = false);

        /// <summary>
        /// Shrink the size of the repository appropriately by storage pattern.
        /// </summary>
        /// <remarks>
        /// For single-origin repositories, purge obsolete revisions.
        /// For origin-shard repositories, purge obsolete revisions known to all other sources.
        /// For revision-shard repositories, rewrite the main file to contain only current revisions and purge individual revision files.
        /// </remarks>
        void Compress();
    }
}
