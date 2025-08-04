using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Xunit;

namespace UnitTests
{
    
    public class MergeDiskTimeLogTests
    {
        [Fact]
        public void Round_Trip_Test()
        {
            var mockFileSystem = new MockFileSystem();
            var path = ".";
            var filename = Path.Combine(path, "TimeLogRepoBase.csv");

            var mapper = new TimeLogCsvMapper(mockFileSystem);
            var repo = new MergeDiskRepository<TimeLogEntry>(mapper, filename);

            var log = new TimeLogEntry
                {
                    ID = Guid.NewGuid(),
                    RevisionGuid = Guid.NewGuid(),
                    StartTime = DateTime.Now,
                    Note = "Test note",
                    Billable = true,
                    Project = "The Project",
                    Client = "A Client",
                    EndTime = DateTime.Now.AddHours(1)
            };
            repo.Create(log);
            repo.SaveChanges();

            var found = repo.Find(log.ID);
            Assert.NotNull(found);
            Assert.True(mockFileSystem.File.Exists(mockFileSystem.Path.Combine(path, $"update-{log.RevisionGuid}.txt")));

            repo.CommitChanges();

            found = repo.Find(log.ID);
            Assert.NotNull(found);
            Assert.Empty(repo.FindConflicts());
            Assert.False(mockFileSystem.File.Exists(mockFileSystem.Path.Combine(path, $"update-{log.RevisionGuid}.txt")), $"File.Exists(Path.Combine({path}, $'update-{log.RevisionGuid}.xml'))");

            repo.Delete(found);
            repo.SaveChanges();
            Assert.Empty(repo.FindConflicts());
            repo.CommitChanges();
            Assert.Null(repo.Find(found.ID));
        }
    }
}
