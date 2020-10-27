using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private string _filename;

        protected CsvDiskMapper(string filename)
        {
            _filename = filename;
        }
		
		public abstract string FieldsHeader { get; }
		public abstract T FromTokens(string[] tokens);
		public abstract string ToCsv(T obj);

        public IEnumerable<T> LoadAll()
        {
            return LoadAll(_filename);
        }

        public virtual void SaveAll(IEnumerable<T> list)
        {
            SaveAll(list, _filename);
        }

        public T Load(Guid id, string filename)
        {
            var items = LoadAll(filename);
            return items.FirstOrDefault(item => item.ID == id);
        }

        public IEnumerable<T> LoadAll(string filename)
        {
            var result = new List<T>();
            if (!File.Exists(filename))
            {
                return result;
            }
            var lines = File.ReadAllLines(filename);

            for (var i = 1; i < lines.Count(); i++)
            {
                try
                {
                    // Generic deserialisation.
                    var item = FromTokens(lines[i].Tokenise().ToArray());
                    result.Add(item);
                }
                catch
                {
                    Trace.WriteLine("Bad line in file: {0}", lines[i]);
                }
            }

            return result;
        }

        public void Save(T item, string filename, bool overwrite = false)
        {
            var items = LoadAll().ToList();
            if (overwrite)
            {
                items.RemoveAll(i => i.ID == item.ID);
            }
            items.Add(item);
            SaveAll(items, filename, overwrite);
        }

        public void SaveAll(IEnumerable<T> items, string filename, bool overwrite = false)
        {
            var output = new StringBuilder();
            output.AppendLine(FieldsHeader);
            foreach (var row in items)
            {
                output.AppendLine(ToCsv(row));
            }

            File.WriteAllText(filename, output.ToString());
        }

        public void Delete(T item, string filename)
        {
            var allitems = LoadAll().ToList();
            if (allitems.Contains(item))
            {
                allitems.Remove(item);
            }
            SaveAll(allitems, filename);
        }
    }
}
