using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Text;
using AssimilationSoftware.Maroon.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ActionItemMapperTests
    {
        [TestMethod]
        public void Round_Trip_Test()
        {
            var fileName = "TestFile.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            var i = new List<ActionItem>
            {
                new ActionItem
                {
                    Context = "test",
                    ID = Guid.NewGuid(),
                    Revision = 1,
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
                Revision = 0,
                Notes = new List<string>(),
                Parent = i[0],
                Tags = new Dictionary<string, string>(),
                Title = "Child, depth 1",
                Upvotes = 0
            });
            var m = new ActionItemDiskMapper(fileName);

            m.SaveAll(i);
            var j = m.LoadAll();

            Assert.IsNotNull(j);
            Assert.AreEqual(i.Count, j.Count());
            foreach (var n in i)
            {
                var s = j.FirstOrDefault(x => x.ID == n.ID);
                Assert.IsNotNull(s);
                Assert.AreEqual(n.Title, s.Title);
                Assert.AreEqual(n.Context, s.Context);
                Assert.AreEqual(n.Revision, s.Revision);
                Assert.AreEqual(n.RankDepth, s.RankDepth);
            }
        }
    }
}
