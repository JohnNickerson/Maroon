using System;
using System.Collections.Generic;
using System.IO;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MergeDiskTimeLogTests
    {
        [TestMethod]
        public void Round_Trip_Test()
        {
            var path = ".";
            string filename = Path.Combine(path, "TimeLogRepoBase.csv");
            if (File.Exists(filename)) File.Delete(filename);

            var mapper = new TimeLogCsvMapper(filename);
            var repo = new MergeDiskRepository<TimeLogEntry>(mapper, path);

            var log = new TimeLogEntry
                {
                    ID = Guid.NewGuid(),
                    Revision = 0,
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
            Assert.IsNotNull(found);
            Assert.IsTrue(File.Exists(Path.Combine(path, $"update-{log.RevisionGuid}.xml")));

            repo.CommitChanges();

            found = repo.Find(log.ID);
            Assert.IsNotNull(found);
            Assert.IsFalse(File.Exists(Path.Combine(path, $"update-{log.RevisionGuid}.xml")));

            repo.Delete(log);
            repo.SaveChanges();
            Assert.IsTrue(File.Exists(Path.Combine(path, $"delete-{log.RevisionGuid}.xml")));
            repo.CommitChanges();
            Assert.IsFalse(File.Exists(Path.Combine(path, $"delete-{log.RevisionGuid}.xml")));
        }
    }
}
