using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Interfaces
{
    public interface IDiskMapper<T> where T : ModelObject
    {
        /// <summary>
        /// Load all items from a list of files. If the list is empty, look for all files.
        /// </summary>
        /// <returns>An enumerable list of every item in the collection.</returns>
        IEnumerable<T> Read(params string[] fileNames);

        /// <summary>
        /// Save the collection as specified by a list, to a given file. The file will be overwritten.
        /// </summary>
        /// <param name="items">The list of items to save.</param>
        /// <param name="filename">The name of the file to write to.</param>
        void Write(IEnumerable<T> items, string filename);

        /// <summary>
        /// Delete a file on disk.
        /// </summary>
        /// <param name="filename">The file to delete.</param>
        /// <remarks> To allow proper separation of the repository from disk operations, and to allow any base path redirection, if required.</remarks>
        void Delete(string filename);

        string[] GetFiles(string primaryPath, string fileSearch, SearchOption searchOption);
    }
}
