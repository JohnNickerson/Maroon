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
        [TestCleanup, TestInitialize]
        public void Cleanup()
        {
            foreach (var updateFile in Directory.GetFiles(".", "*.txt"))
            {
                File.Delete(updateFile);
            }
        }

        [TestMethod]
        public void Round_Trip_Test()
        {
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
            var m = new ActionItemTextMapper();

            m.SaveAll(i, fileName);
            var j = m.LoadAll(fileName);

            Assert.IsNotNull(j);
            Assert.AreEqual(i.Count, j.Count());
            foreach (var n in i)
            {
                var s = j.FirstOrDefault(x => x.ID == n.ID);
                Assert.IsNotNull(s);
                Assert.AreEqual(n.Title, s.Title);
                Assert.AreEqual(n.Context, s.Context);
                Assert.AreEqual(n.RevisionGuid, s.RevisionGuid);
            }
        }
    }
}
