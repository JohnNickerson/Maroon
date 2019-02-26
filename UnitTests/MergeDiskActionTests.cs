using System;
using System.Collections.Generic;
using System.IO;
using AssimilationSoftware.Maroon.Mappers.Text;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MergeDiskActionTests
    {
        [TestMethod]
        public void Save_Changes_Test()
        {
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemDiskMapper("todo.txt"), ".");
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                Revision = 0,
                Project = null, Parent = null,
                Title = "A test item for saving",
                LastModified = DateTime.Now,
                DoneDate = null,
                Tags = new Dictionary<string, string>
                {
                    {"type", "test"}
                },
                Notes = new List<string> {"Note1."},
                Upvotes = 0,
                Done = false,
                Status = "watstatus",
                TickleDate = null
            };
            repo.Create(item);
            repo.SaveChanges();

            Assert.IsTrue(File.Exists($".\\update-{item.ID}.xml"));
        }
    }
}
