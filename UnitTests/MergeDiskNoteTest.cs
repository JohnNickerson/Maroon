using System;
using System.Collections.Generic;
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
            var repo = new MergeDiskRepository<Note>(new NoteDiskMapper(fileName), Path.GetDirectoryName(fileName));

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

            var repo2 = new MergeDiskRepository<Note>(new NoteDiskMapper(fileName), Path.GetDirectoryName(fileName));
            repo2.FindAll();

            Assert.AreEqual(1, repo2.Items.Count(), "Empty in new repository");

            repo.CommitChanges();

            Assert.AreEqual(1, repo2.Items.Count(), "Empty in new repository after commit");
        }
    }
}
