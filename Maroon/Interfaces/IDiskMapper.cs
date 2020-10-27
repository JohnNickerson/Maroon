using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssimilationSoftware.Maroon.Interfaces
{
    public interface IDiskMapper<T>
    {
        /// <summary>
        /// Load a specific item by ID from a given file.
        /// </summary>
        /// <param name="id">The ID of the item to load.</param>
        /// <param name="filename">The name of the file to load from.</param>
        /// <returns>The item denoted by the given ID or null, if no such item exists.</returns>
        T Load(Guid id, string filename);

        /// <summary>
        /// Load all items in the collection.
        /// </summary>
        /// <returns>An enumerable list of every item in the collection.</returns>
        IEnumerable<T> LoadAll(string filename);

        /// <summary>
        /// Save a specific item, whether new or updated.
        /// </summary>
        /// <param name="item">The item to save.</param>
        /// <param name="filename">The name of the file to write to.</param>
        /// <param name="overwrite">True to write a new file, false to add or replace the existing item.</param>
        void Save(T item, string filename, bool overwrite = false);

        /// <summary>
        /// Save the collection as specified by a list.
        /// </summary>
        /// <param name="items">The list of items to save.</param>
        /// <param name="filename">The name of the file to write to.</param>
        /// <param name="overwrite">True to write a new file, false to add or replace the existing items.</param>
        void SaveAll(IEnumerable<T> items, string filename, bool overwrite = false);

        /// <summary>
        /// Delete one particular item.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <param name="filename">The name of the file to delete from. Empty files will be deleted.</param>
        void Delete(T item, string filename);
    }
}
