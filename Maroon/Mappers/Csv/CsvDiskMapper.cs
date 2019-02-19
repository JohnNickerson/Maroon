using System;
using System.Collections.Generic;
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
    public abstract class CsvDiskMapper<T> : IMapper<T> where T : ModelObject
    {
        private string _filename;

        public CsvDiskMapper(string filename)
        {
            _filename = filename;
        }
		
		public abstract string FieldsHeader { get; }
		public abstract T FromTokens(string[] tokens);
		public abstract string ToCsv(T obj);

        public IEnumerable<T> LoadAll()
        {
            var result = new List<T>();
			if (!File.Exists(_filename))
			{
				return result;
			}
            var lines = File.ReadAllLines(_filename);

            for (var i = 1; i < lines.Count(); i++)
            {
                // Generic deserialisation.
				var item = FromTokens(lines[i].Tokenise().ToArray());
                result.Add(item);
            }

            return result;
        }

        public virtual void SaveAll(IEnumerable<T> list)
        {
            var output = new StringBuilder();
            output.AppendLine(FieldsHeader);
            foreach (var row in list)
            {
                output.AppendLine(ToCsv(row));
            }

            File.WriteAllText(_filename, output.ToString());
        }

        public void Save(T item)
        {
            var items = LoadAll().ToList();
            items.Add(item);
            SaveAll(items);
        }

        public T Load(Guid id)
        {
            var items = LoadAll();
            return items.FirstOrDefault(item => item.ID == id);
        }

        public void Delete(T item)
        {
            var allitems = LoadAll().ToList();
            if (allitems.Contains(item))
            {
                allitems.Remove(item);
            }
            SaveAll(allitems);
        }
    }
}
