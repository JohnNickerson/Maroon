using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Text;
using AssimilationSoftware.Maroon.Model;
using Xunit;

namespace UnitTests
{
    
    public class NoteMapperTests
    {
        [Fact]
        public void Round_Trip_Test()
        {
            var mockFileSystem = new MockFileSystem();
            var notebook = new List<Note>
            {
                new Note
                {
                    ID = Guid.NewGuid(),
                    RevisionGuid = Guid.NewGuid(),
                    TagString = "#test1 #test2",
                    Text = "This is a test entry",
                    Timestamp = DateTime.Now
                }
            };
            var filename = "NotesOnDisk.txt";
            var noteMapper = new NoteDiskMapper(mockFileSystem);
            noteMapper.Write(notebook, filename);

            var fromDisk = noteMapper.Read(filename);
            Assert.NotNull(fromDisk );
            Assert.Equal(notebook.Count, fromDisk.Count());
            foreach (var i in fromDisk)
            {
                var n = notebook.FirstOrDefault(f => f.ID == i.ID);
                Assert.NotNull(n);
                Assert.Equal(n.TagString, i.TagString);
                Assert.Equal(n.Text, i.Text);
                Assert.Equal(n.Timestamp.Year, i.Timestamp.Year);
                Assert.Equal(n.Timestamp.Month, i.Timestamp.Month);
                Assert.Equal(n.Timestamp.Day, i.Timestamp.Day);
                Assert.Equal(n.Timestamp.Hour, i.Timestamp.Hour);
                Assert.Equal(n.Timestamp.Minute, i.Timestamp.Minute);
                Assert.Equal(n.RevisionGuid, i.RevisionGuid);
            }
        }
    }
}
