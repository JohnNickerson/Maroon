using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Text;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MergeDiskNoteTest
    {
        [TestInitialize]
        public void Setup()
        {
            foreach (var updateFile in Directory.GetFiles(".", "update*.xml"))
            {
                File.Delete(updateFile);
            }
            foreach (var updateFile in Directory.GetFiles(".", "*.txt"))
            {
                File.Delete(updateFile);
            }
        }

        [TestMethod]
        public void Create_File_From_Scratch()
        {
            var fileName = "LogFile.txt";
            var repo = new MergeDiskRepository<Note>(new NoteDiskMapper(), fileName);

            repo.Create(new Note
            {
                ID = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                RevisionGuid = Guid.NewGuid(),
                LastModified = DateTime.Now,
                Text = "This is a test entry",
                ParentId = null,
                Tags = new List<string> { "test" }
            });
            repo.SaveChanges();

            Assert.AreEqual(1, repo.Items.Count(), "Empty after saving");

            var repo2 = new MergeDiskRepository<Note>(new NoteDiskMapper(), fileName);
            repo2.FindAll();

            Assert.AreEqual(1, repo2.Items.Count(), "Empty in new repository");

            repo.CommitChanges();

            Assert.AreEqual(1, repo2.Items.Count(), "Empty in new repository after commit");
        }

        [TestMethod]
        public void Revert_Conflict()
        {
            // Set up a conflict.
            var primaryFileName = "notes.txt";
            var mapper = new NoteDiskMapper();
            Guid root = Guid.NewGuid();
            Guid rev1 = Guid.NewGuid();
            Guid rev2 = Guid.NewGuid();
            Guid rev3 = Guid.NewGuid();
            var data = new List<Note>
            {
                new Note
                {
                    ID = root,
                    Timestamp = DateTime.Now,
                    ParentId = null,
                    LastModified = DateTime.Now,
                    RevisionGuid = rev1,
                    Text = "base text",
                    Tags = new List<string>{"tag"}
                },
                new Note
                {
                    ID = root,
                    Timestamp = DateTime.Now,
                    ParentId = null,
                    LastModified = DateTime.Now,
                    RevisionGuid = rev2,
                    PrevRevision = rev1,
                    Text = "edited text",
                    Tags = new List<string>{"tag"}
                },
                new Note
                {
                    ID = root,
                    Timestamp = DateTime.Now,
                    ParentId = null,
                    LastModified = DateTime.Now,
                    RevisionGuid = rev3,
                    PrevRevision = rev1,
                    Text = "base text has been modified",
                    Tags = new List<string>{"tag"}
                },
            };

            foreach (var n in data)
            {
                mapper.Save(n, $"update-{n.RevisionGuid}.txt");
            }

            var path = Path.GetDirectoryName(Path.GetFullPath(primaryFileName));
            Assert.AreEqual(3, Directory.GetFiles(path, "update-*.txt").Length);

            // Get the conflicts into a repository.
            var repo = new MergeDiskRepository<Note>(new NoteDiskMapper(), primaryFileName);
            repo.FindAll();
            Assert.AreEqual(1, repo.Items.Count());
            Assert.AreEqual(1, repo.FindConflicts().Count);

            repo.Revert(root);
            repo.SaveChanges();
            Assert.AreEqual(1, repo.Items.Count());
            Assert.AreEqual(1, Directory.GetFiles(path, "update-*.txt").Length);
            Assert.AreEqual(0, repo.FindConflicts().Count);
        }
    }
}
