using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Text;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Xunit;

namespace UnitTests
{
    
    public class MergeDiskNoteTest
    {
        [Fact]
        public void Create_File_From_Scratch()
        {
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(Environment.CurrentDirectory);
            var fileName = "LogFile.txt";
            var repo = new MergeDiskRepository<Note>(new NoteDiskMapper(mockFileSystem), fileName);

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

            Assert.Single(repo.Items);

            var repo2 = new MergeDiskRepository<Note>(new NoteDiskMapper(mockFileSystem), fileName);
            repo2.FindAll();

            Assert.Single(repo2.Items);

            repo.CommitChanges();

            Assert.Single(repo2.Items);
        }

        [Fact]
        public void Revert_Conflict()
        {
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.Directory.CreateDirectory(Environment.CurrentDirectory);
            var primaryFileName = "notes.txt";
            var mapper = new NoteDiskMapper(mockFileSystem);
            var repo = new MergeDiskRepository<Note>(mapper, primaryFileName);
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
            repo.Create(data[0]);
            var found = repo.Find(root);
            Assert.NotNull(found);
            found.Text = "edited text";
            repo.Update(found);
            var updated = repo.Find(root);
            Assert.NotNull(updated);
            Assert.Equal("edited text", updated.Text);

            repo.Revert(updated.ID);
            var revved = repo.Find(updated.ID);
            Assert.NotNull(revved);
            Assert.Equal(data[0].Text, revved.Text);

            repo.SaveChanges();
            Assert.Single(repo.Items);
            Assert.Single(mockFileSystem.Directory.GetFiles(".", "update-*.txt"));
            Assert.Empty(repo.FindConflicts());
        }
    }
}
