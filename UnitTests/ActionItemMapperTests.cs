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
                    Revision = 0,
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
            }
        }
    }
}
