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
                ProjectId = null, ParentId = null,
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

            Assert.IsTrue(File.Exists($".\\update-{item.RevisionGuid}.xml"));
        }

        [TestMethod]
        public void Double_Edit_No_Conflicts_Test()
        {
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemDiskMapper("todo.txt"), ".");
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                Revision = 0,
                ProjectId = null,
                ParentId = null,
                Title = "A test item for saving",
                LastModified = DateTime.Now,
                DoneDate = null,
                Tags = new Dictionary<string, string>
                {
                    {"type", "test"}
                },
                Notes = new List<string> { "Note1." },
                Upvotes = 0,
                Done = false,
                Status = "watstatus",
                TickleDate = null
            };
            repo.Create(item);
            repo.SaveChanges();
            item.Context = "moved";
            repo.Update(item);

            Assert.AreEqual(0, repo.FindConflicts().Count);
        }

        [TestMethod]
        public void Conflicting_Edits_Test()
        {
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemDiskMapper("todo.txt"), ".");
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                Revision = 0,
                ProjectId = null,
                ParentId = null,
                Title = "A test item for saving",
                LastModified = DateTime.Now,
                DoneDate = null,
                Tags = new Dictionary<string, string>
                {
                    {"type", "test"}
                },
                Notes = new List<string> { "Note1." },
                Upvotes = 0,
                Done = false,
                Status = "watstatus",
                TickleDate = null
            };
            repo.Create(item);
            item.Context = "moved";
            item.Revision = 0;
            repo.Update(item);

            Assert.AreEqual(1, repo.FindConflicts().Count);
        }

        [TestMethod]
        public void Rank_Serialisation_Test()
        {
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemDiskMapper("todo.txt"), ".");
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                Revision = 0,
                ProjectId = null,
                ParentId = null,
                Title = "A test item for saving",
                LastModified = DateTime.Now,
                DoneDate = null,
                Tags = new Dictionary<string, string>
                {
                    {"type", "test"}
                },
                Notes = new List<string> { "Note1." },
                Upvotes = 0,
                Done = false,
                Status = "watstatus",
                TickleDate = null
            };
            repo.Create(item);
            var item2 = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                Revision = 0,
                ProjectId = null,
                ParentId = item.ID,
                Title = "A child test item for saving",
                LastModified = DateTime.Now,
                DoneDate = null,
                Tags = new Dictionary<string, string>
                {
                    {"type", "child"}
                },
                Notes = new List<string> { "This object should be under the other one." },
                Upvotes = 0,
                Done = false,
                Status = "notdone",
                TickleDate = null
            };
            repo.Create(item2);
            repo.SaveChanges();

            var testrepo = new MergeDiskRepository<ActionItem>(new ActionItemDiskMapper("todo.txt"), ".");
            var i = testrepo.Find(item2.ID);
            Assert.IsNotNull(i);
            Assert.AreEqual(item2.ParentId, i.ParentId);
            Assert.IsNotNull(i.ParentId);
        }
    }
}
