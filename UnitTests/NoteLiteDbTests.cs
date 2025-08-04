using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AssimilationSoftware.Maroon.Model;
using Xunit;

namespace UnitTests
{
    
    public class NoteLiteDbTests
    {
        [Obsolete("use file system abstraction")]
        private void Cleanup()
        {
            foreach (var updateFile in Directory.GetFiles(".", "notes-test.db"))
            {
                try
                {
                    File.Delete(updateFile);
                }
                catch { /*ignored */ }
            }
        }

        [Fact]
        public void Round_Trip_Test()
        {
            Cleanup();
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
            Assert.Equal(note.ID, loaded.ID);
            Cleanup();
        }
    }
}
