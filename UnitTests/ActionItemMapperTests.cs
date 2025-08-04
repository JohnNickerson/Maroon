using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Text;
using AssimilationSoftware.Maroon.Model;
using Xunit;

namespace UnitTests
{
    
    public class ActionItemMapperTests
    {
        [Fact]
        public void Round_Trip_Test()
        {
            var mockFileSystem = new MockFileSystem();
            var fileName = "TestFile.txt";
            var i = new List<ActionItem>
            {
                new ActionItem
                {
                    Context = "test",
                    ID = Guid.NewGuid(),
                    RevisionGuid = Guid.NewGuid(),
                    Title = "Test Item",
                    Notes = new List<string>
                    {
                        "Note line 1",
                        "Note line 2"
                    },
                    Tags = new Dictionary<string, string>
                    {
                        {"type", "testValue"}
                    }
                }
            };
            i.Add(new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "test",
                RevisionGuid = Guid.NewGuid(),
                Notes = new List<string>(),
                ParentId = i[0].ParentId,
                Tags = new Dictionary<string, string>(),
                Title = "Child, depth 1",
                Upvotes = 0
            });
            var m = new ActionItemTextMapper(mockFileSystem);

            m.Write(i, fileName);
            var j = m.Read(fileName).ToArray();

            Assert.NotNull(j);
            Assert.Equal(i.Count, j.Count());
            foreach (var n in i)
            {
                var s = j.FirstOrDefault(x => x.ID == n.ID);
                Assert.NotNull(s);
                Assert.Equal(n.Title, s.Title);
                Assert.Equal(n.Context, s.Context);
                Assert.Equal(n.RevisionGuid, s.RevisionGuid);
            }
        }
    }
}
