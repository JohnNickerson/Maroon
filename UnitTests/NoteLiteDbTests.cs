using System;
using System.Collections.Generic;
using System.IO;
using AssimilationSoftware.Maroon.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class NoteLiteDbTests
    {
        [TestMethod]
        public void Round_Trip_Test()
        {
            var filename = "notes-test.db";
            if (File.Exists(filename)) File.Delete(filename);
            var mapper = new AssimilationSoftware.Maroon.Mappers.LiteDb.BaseLiteDbMapper<Note>(filename, "notes");

            var note = new Note
            {
                ID = Guid.NewGuid(),
                IsDeleted = false,
                ImportHash = null,
                LastModified = DateTime.Now,
                ParentId = null,
                PrevRevision = null,
                RevisionGuid = Guid.NewGuid(),
                Tags = new List<string>(new[] {"test", "tags"}),
                Text = "This is a test item\nWith a newline",
                Timestamp = DateTime.Now
            };
            mapper.Save(note);

            var loaded = mapper.Load(note.ID);
            Assert.AreEqual(note.ID, loaded.ID);
        }
    }
}
