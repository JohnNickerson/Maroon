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
        [TestInitialize]
        public void Setup()
        {
            foreach (var updateFile in Directory.GetFiles(".", "update*.xml"))
            {
                File.Delete(updateFile);
            }
            foreach (var updateFile in Directory.GetFiles(".", "*.csv"))
            {
                File.Delete(updateFile);
            }
        }

        [TestMethod]
        public void Round_Trip_Test()
        {
            var path = ".";
            string filename = Path.Combine(path, "TimeLogRepoBase.csv");

            var mapper = new TimeLogCsvMapper(filename);
            var repo = new MergeDiskRepository<TimeLogEntry>(mapper, path);

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
            Assert.IsNotNull(found);
            Assert.IsTrue(File.Exists(Path.Combine(path, $"update-{log.RevisionGuid}.xml")));

            repo.CommitChanges();

            found = repo.Find(log.ID);
            Assert.IsNotNull(found);
            Assert.AreEqual(0, repo.FindConflicts().Count);
            Assert.IsFalse(File.Exists(Path.Combine(path, $"update-{log.RevisionGuid}.xml")), $"File.Exists(Path.Combine({path}, $'update-{log.RevisionGuid}.xml'))");

            repo.Delete(log);
            repo.SaveChanges();
            Assert.AreEqual(0, repo.FindConflicts().Count);
            repo.CommitChanges();
            Assert.IsNull(repo.Find(log.ID), "Deleted item still in repository.");
        }
    }
}
