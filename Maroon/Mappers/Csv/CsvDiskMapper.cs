using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Mappers.Csv
{
    /// <summary>
    /// Generic CSV mapper.
    /// </summary>
    /// <typeparam name="T">The type of model data to work with.</typeparam>
    public abstract class CsvDiskMapper<T> : IDiskMapper<T> where T : ModelObject
    {
        public abstract string FieldsHeader { get; }
        public abstract T FromTokens(string[] tokens);
        public abstract string ToCsv(T obj);
        public abstract IFileSystem FileSystem { get; protected set; }

        private IEnumerable<T> LoadAll(string filename)
        {
            if (!FileSystem.File.Exists(filename))
            {
                yield break;
            }
            var lines = FileSystem.File.ReadAllLines(filename);

            for (var i = 1; i < lines.Length; i++)
            {
                T item = null;
                try
                {
                    item = FromTokens(lines[i].Tokenise().ToArray());
                }
                catch
                {
                    Trace.WriteLine($"Bad line in file: {lines[i]}");
                }

                if (item == null) yield break;
                yield return item;
            }
        }

        public IEnumerable<T> Read(params string[] fileNames)
        {
            return fileNames.SelectMany(LoadAll);
        }

        public void Write(IEnumerable<T> items, string filename)
        {
            var output = new StringBuilder();
            output.AppendLine(FieldsHeader);
            foreach (var row in items)
            {
                output.AppendLine(ToCsv(row));
            }

            FileSystem.File.WriteAllText(filename, output.ToString());
        }

        public void Delete(string filename)
        {
            FileSystem.File.Delete(filename);
        }

        public string[] GetFiles(string path, string search, SearchOption searchOption)
        {
            return FileSystem.Directory.GetFiles(path, search, searchOption);
        }
    }
}
