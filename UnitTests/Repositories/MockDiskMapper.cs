using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Mappers.Csv;

namespace AssimilationSoftware.Maroon.Repositories.Tests
{
    public class MockDiskMapper : CsvDiskMapper<MockObj>, IDataSource<MockObj>
    {
        private readonly Dictionary<string, List<MockObj>> _items = new Dictionary<string, List<MockObj>>();

        public override string FieldsHeader => "ID,RevisionGuid,PrevRevision,LastModified";

        public override IFileSystem FileSystem { get; protected set; }

        public MockDiskMapper()
        {
            FileSystem = new MockFileSystem();
            FileSystem.Directory.CreateDirectory(Environment.CurrentDirectory);
        }

        public override MockObj FromTokens(string[] tokens)
        {
            return new MockObj
            {
                ID = Guid.Parse(tokens[0]),
                RevisionGuid = Guid.Parse(tokens[1]),
                PrevRevision = Guid.Parse(tokens[2]),
                LastModified = DateTime.Parse(tokens[3])
            };
        }

        public override string ToCsv(MockObj obj)
        {
            return $"{obj.ID},{obj.RevisionGuid},{obj.PrevRevision},{obj.LastModified}";
        }

        public IEnumerable<MockObj> FindAll()
        {
    return _items.Values.SelectMany(x => x);
        }

        public MockObj? FindRevision(Guid id)
        {
            throw new NotImplementedException();
        }

        public MockObj Create(MockObj item)
        {
            throw new NotImplementedException();
        }

        public MockObj Update(MockObj item)
        {
            throw new NotImplementedException();
        }

        public MockObj Delete(MockObj item)
        {
            throw new NotImplementedException();
        }

        public void Purge(params Guid[] ids)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTime()
        {
            throw new NotImplementedException();
        }
    }
}