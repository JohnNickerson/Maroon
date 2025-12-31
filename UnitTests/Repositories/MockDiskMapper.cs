using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Helpers;

namespace AssimilationSoftware.Maroon.Repositories.Tests
{
    public class MockDiskMapper : IDataSource<MockObj>
    {
        private readonly Dictionary<Guid, MockObj> _items = new Dictionary<Guid, MockObj>();

        public MockDiskMapper()
        {
            _items = new Dictionary<Guid, MockObj>();
        }

        public IEnumerable<MockObj> FindAll()
        {
            return _items.Values;
        }

        public MockObj? FindRevision(Guid id)
        {
            if (_items.ContainsKey(id))
            {
                return _items[id];
            }
            return null;
        }

        public MockObj Insert(MockObj item)
        {
            item.LastModified = DateTime.Now;
            _items[item.RevisionGuid] = item;
            return item;
        }

        public void Purge(params Guid[] ids)
        {
            ids.ToList().ForEach(id => _items.Remove(id));
        }

        public DateTime GetLastWriteTime()
        {
            return _items.Values.Count > 0 ? _items.Values.Max(i => i.LastModified) : DateTime.MinValue;
        }
    }
}