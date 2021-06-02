using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;
// ReSharper disable MemberCanBeProtected.Global

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

        private IEnumerable<T> LoadAll(string filename)
        {
            if (!File.Exists(filename))
            {
                yield break;
            }
            var lines = File.ReadAllLines(filename);

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

            File.WriteAllText(filename, output.ToString());
        }

        public void Delete(string filename)
        {
            File.Delete(filename);
        }
    }
}
