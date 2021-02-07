using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssimilationSoftware.Maroon.Mappers.Text;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class OriginDiskRepositoryTests
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
        public void TestConstructor()
        {
            var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt", Environment.MachineName);
            Assert.IsNotNull(repo.Items);
        }

        [TestMethod]
        public void TestSaveOne()
        {
            var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt", Environment.MachineName);
            var item = NewTestItem();
            repo.Create(item);
            repo.SaveChanges();
        }

        [TestMethod]
        public void TestSaveMany()
        {
            var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt", Environment.MachineName);
            var items = new List<ActionItem>()
            {
                NewTestItem(),
                NewTestItem(),
                NewTestItem()
            };
            foreach (var i in items)
            {
                repo.Create(i);
            }
            repo.SaveChanges();
        }

        [TestMethod]
        public void TestCommitOne()
        {
            var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt", Environment.MachineName);
            var item = NewTestItem();
            repo.Create(item);
            repo.SaveChanges();
            repo.CommitChanges();
        }

        [TestMethod]
        public void TestCommitMany()
        {
            var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt", Environment.MachineName);
            var items = new List<ActionItem>()
            {
                NewTestItem(),
                NewTestItem(),
                NewTestItem()
            };
            foreach (var i in items)
            {
                repo.Create(i);
            }
            repo.SaveChanges();
            repo.CommitChanges();
        }

        [TestMethod]
        public void TestLoadOne()
        {
            var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt", Environment.MachineName);
            var item = NewTestItem();
            repo.Create(item);
            repo.SaveChanges();
            var found = repo.Find(item.ID);
            Assert.AreEqual(item.ID, found.ID);
            Assert.AreEqual(item.Title, found.Title);
            Assert.AreEqual(item.RevisionGuid, found.RevisionGuid);
        }

        [TestMethod]
        public void TestLoadMany()
        {
            var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt", Environment.MachineName);
            foreach (var d in repo.FindAll().ToList())
            {
                repo.Delete(d);
            }
            var items = new List<ActionItem>()
            {
                NewTestItem(),
                NewTestItem(),
                NewTestItem()
            };
            foreach (var i in items)
            {
                repo.Create(i);
            }
            repo.SaveChanges();
            repo.CommitChanges();
            var found = repo.FindAll();
            Assert.AreEqual(3, found.Count());
            for (int x = 0; x < items.Count(); x++)
            {
                var f = found.FirstOrDefault(z => z.ID == items[x].ID);
                Assert.AreEqual(items[x].ID, f.ID);
                Assert.AreEqual(items[x].Title, f.Title);
                Assert.AreEqual(items[x].RevisionGuid, f.RevisionGuid);
            }
        }

        [TestMethod]
        public void TestSaveTwice()
        {
            var primaryFileName = "todo.txt";
            {
                if (File.Exists(primaryFileName)) File.Delete(primaryFileName);
                if (File.Exists($"updates-{Environment.MachineName}.txt")) File.Delete($"updates-{Environment.MachineName}.txt");
                var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), primaryFileName, Environment.MachineName);
                var i1 = NewTestItem();
                repo.Create(i1);
                repo.SaveChanges();
            }
            var repo2 = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), primaryFileName, Environment.MachineName);
            var i2 = NewTestItem();
            repo2.Create(i2);
            repo2.SaveChanges();
            var found = repo2.FindAll();
            Assert.AreEqual(2, found.Count());
        }

        [TestMethod]
        public void TestUpdateTwice()
        {
            {
                if (File.Exists("todos.txt"))
                {
                    File.Delete("todos.txt");
                }

                if (File.Exists($"updates-{Environment.MachineName}.txt"))
                {
                    File.Delete($"updates-{Environment.MachineName}.txt");
                }

                var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt",
                    Environment.MachineName);
                var i1 = NewTestItem();
                repo.Create(i1);
                repo.SaveChanges();
                i1.Title = "edited";
                repo.Update(i1);
                repo.SaveChanges();
            }
            {
                var repo = new OriginDiskRepository<ActionItem>(new ActionItemTextMapper(), "todo.txt", Environment.MachineName);
                var found = repo.FindAll();
                Assert.AreEqual(1, found.Count());
                var pendingChanges = repo.GetPendingChanges();
                Assert.AreEqual(1, pendingChanges.Count);
                Assert.AreEqual(2, pendingChanges[0].Updates.Count);
                Assert.IsTrue(pendingChanges[0].Updates.Exists(i => i.Title == "a test item for the origin XML serialiser"));
                Assert.IsTrue(pendingChanges[0].Updates.Exists(i => i.Title == "edited"));
            }
        }

        private static ActionItem NewTestItem()
        {
            return new ActionItem
            {
                Context = "contek",
                Done = false,
                DoneDate = null,
                ID = Guid.NewGuid(),
                ImportHash = null,
                IsDeleted = false,
                LastModified = DateTime.Now,
                Notes = new List<string>(new[] { "note line 1", "note 2" }),
                ParentId = null,
                PrevRevision = null,
                RevisionGuid = Guid.NewGuid(),
                ProjectId = null,
                Status = "undone",
                Tags = new Dictionary<string, string>() { { "nsfw", "true" } },
                TickleDate = null,
                Title = "a test item for the origin XML serialiser",
                Upvotes = 2
            };
        }
    }
}
