using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Xunit;

namespace UnitTests
{
    
    public class MergeDiskTimeLogTests
    {
        [Obsolete("Use file system abstraction")]
        private void Cleanup()
        {
            foreach (var updateFile in Directory.GetFiles(".", "*.csv"))
            {
                File.Delete(updateFile);
            }
        }

        [Fact]
        public void Round_Trip_Test()
        {
            Cleanup();
            var path = ".";
            var filename = Path.Combine(path, "TimeLogRepoBase.csv");

            var mapper = new TimeLogCsvMapper();
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
            Assert.True(File.Exists(Path.Combine(path, $"update-{log.RevisionGuid}.txt")));

            repo.CommitChanges();

            found = repo.Find(log.ID);
            Assert.NotNull(found);
            Assert.Empty(repo.FindConflicts());
            Assert.False(File.Exists(Path.Combine(path, $"update-{log.RevisionGuid}.txt")), $"File.Exists(Path.Combine({path}, $'update-{log.RevisionGuid}.xml'))");

            repo.Delete(found);
            repo.SaveChanges();
            Assert.Empty(repo.FindConflicts());
            repo.CommitChanges();
            Assert.Null(repo.Find(found.ID));
            Cleanup();
        }
    }
}
