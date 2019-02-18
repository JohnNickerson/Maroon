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
    public class NoteMapperTests
    {
        [TestMethod]
        public void Round_Trip_Test()
        {
            var notebook = new List<Note>
            {
                new Note
                {
                    ID = Guid.NewGuid(),
                    Revision = 1,
                    TagString = "#test1 #test2",
                    Text = "This is a test entry",
                    Timestamp = DateTime.Now
                }
            };
            var filename = "NotesOnDisk.txt";
            if (File.Exists(filename)) File.Delete(filename);
            var noteMapper = new NoteDiskMapper(filename);
            noteMapper.SaveAll(notebook);

            var fromDisk = noteMapper.LoadAll();
            Assert.IsNotNull(fromDisk );
            Assert.AreEqual(notebook.Count, fromDisk.Count());
            foreach (var i in fromDisk)
            {
                var n = notebook.FirstOrDefault(f => f.ID == i.ID);
                Assert.IsNotNull(n);
                Assert.AreEqual(n.TagString, i.TagString);
                Assert.AreEqual(n.Text, i.Text);
                Assert.AreEqual(n.Timestamp.Year, i.Timestamp.Year);
                Assert.AreEqual(n.Timestamp.Month, i.Timestamp.Month);
                Assert.AreEqual(n.Timestamp.Day, i.Timestamp.Day);
                Assert.AreEqual(n.Timestamp.Hour, i.Timestamp.Hour);
                Assert.AreEqual(n.Timestamp.Minute, i.Timestamp.Minute);
                Assert.AreEqual(n.Revision, i.Revision);
            }
        }
    }
}
