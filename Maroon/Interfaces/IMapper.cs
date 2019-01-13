using System;
using System.Collections.Generic;

namespace AssimilationSoftware.Maroon.Interfaces
{
    public interface IMapper<T>
    {
        /// <summary>
        /// Load a specific item by ID.
        /// </summary>
        /// <param name="id">The ID of the item to load.</param>
        /// <returns>The item denoted by the given ID or null, if no such item exists.</returns>
        T Load(Guid id);

        /// <summary>
        /// Load all items in the collection.
        /// </summary>
        /// <returns>An enumerable list of every item in the collection.</returns>
        IEnumerable<T> LoadAll();

        /// <summary>
        /// Save a specific item, whether new or updated.
        /// </summary>
        /// <param name="item">The item to save.</param>
        void Save(T item);

        /// <summary>
        /// Save the collection as specified by a list.
        /// </summary>
        /// <param name="items">The list of items to save.</param>
        void SaveAll(IEnumerable<T> items);

        /// <summary>
        /// Delete one particular item.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        void Delete(T item);
    }
}
