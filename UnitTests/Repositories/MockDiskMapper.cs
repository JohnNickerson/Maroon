using System.Collections.Generic;
using System.Linq;
using AssimilationSoftware.Maroon.Interfaces;

namespace AssimilationSoftware.Maroon.Repositories.Tests
{
    public class MockDiskMapper : IDiskMapper<MockObj>
    {
        private readonly Dictionary<string, List<MockObj>> _items = new Dictionary<string, List<MockObj>>();

        public IEnumerable<MockObj> Read(params string[] fileNames)
        {
            return fileNames.SelectMany(f => _items.ContainsKey(f) ? _items[f] : new List<MockObj>());
        }

        public void Write(IEnumerable<MockObj> items, string filename)
        {
            _items[filename] = items.ToList();
        }

        public void Delete(string filename)
        {
            _items.Remove(filename);
        }
    }
}