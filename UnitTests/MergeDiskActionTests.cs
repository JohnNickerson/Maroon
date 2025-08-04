using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Text;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Xunit;

namespace UnitTests
{
    
    public class MergeDiskActionTests
    {
        [Obsolete("Use file system abstraction")]
        private void Cleanup()
        {
            foreach (var updateFile in Directory.GetFiles(".", "*.txt"))
            {
                File.Delete(updateFile);
            }
        }

        [Fact]
        public void Save_Changes_Test()
        {
            Cleanup();
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemTextMapper(), ".");
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                RevisionGuid = Guid.NewGuid(),
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

            Assert.True(File.Exists($".\\update-{item.RevisionGuid}.txt"));
            Cleanup();
        }

        [Fact]
        public void Double_Edit_No_Conflicts_Test()
        {
            Cleanup();
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemTextMapper(), ".");
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                RevisionGuid = Guid.NewGuid(),
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

            Assert.Empty(repo.FindConflicts());
            Cleanup();
        }

        [Fact]
        public void Conflicting_Edits_Test()
        {
            Cleanup();
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemTextMapper(), ".");
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                RevisionGuid = Guid.NewGuid(),
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
            repo.Create((ActionItem)item.Clone());

            Assert.Empty(repo.FindConflicts());

            item = repo.Find(item.ID);
            item.Context = "moved";
            repo.Update((ActionItem)item.Clone());

            Assert.Empty(repo.FindConflicts());

            item.Context = "conflicted";
            repo.Update((ActionItem)item.Clone());

            Assert.Single(repo.Items);
            Assert.Single(repo.FindConflicts());
            Cleanup();
        }

        [Fact]
        public void Rank_Serialisation_Test()
        {
            Cleanup();
            var primaryFileName = "todo.txt";
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemTextMapper(), primaryFileName);
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                RevisionGuid = Guid.NewGuid(),
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
                RevisionGuid = Guid.NewGuid(),
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

            var testrepo = new MergeDiskRepository<ActionItem>(new ActionItemTextMapper(), primaryFileName);
            var i = testrepo.Find(item2.ID);
            Assert.NotNull(i);
            Assert.Equal(item2.ParentId, i.ParentId);
            Assert.NotNull(i.ParentId);
            Cleanup();
        }

        [Fact]
        public void Double_Create_No_Conflicts_Test()
        {
            Cleanup();
            var repo = new MergeDiskRepository<ActionItem>(new ActionItemTextMapper(), ".");
            var item = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                RevisionGuid = Guid.NewGuid(),
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
            repo.Create((ActionItem)item.Clone());
            Assert.Single(repo.Items);
            var item2 = new ActionItem
            {
                ID = Guid.NewGuid(),
                Context = "context",
                RevisionGuid = Guid.NewGuid(),
                ProjectId = null,
                ParentId = null,
                Title = "Another test item for saving",
                LastModified = DateTime.Now,
                DoneDate = null,
                Tags = new Dictionary<string, string>
                {
                    {"type", "test"}
                },
                Notes = new List<string> { "Note2." },
                Upvotes = 0,
                Done = false,
                Status = "statuses",
                TickleDate = null
            };
            repo.Create((ActionItem)item2.Clone());
            Assert.Equal(2, repo.Items.Count());
            Assert.Equal(2, repo.GetPendingChanges().Count);
            Assert.Empty(repo.FindConflicts());
            Cleanup();
        }
    }
}
