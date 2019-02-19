using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class TimeLogMapperTests
    {
        [TestMethod]
        public void Round_Trip_Test()
        {
            var timeLog = new List<TimeLogEntry>
            {
                new TimeLogEntry
                {
                    ID = Guid.NewGuid(),
                    Billable = true,
                    Client = "TestClient",
                    EndTime = DateTime.Now.AddHours(1),
                    StartTime = DateTime.Now,
                    Project = "TestProject",
                    Note = "Test Note"
                }
            };
            var filename = "TestTimeFile.csv";
            if (File.Exists(filename)) { File.Delete(filename); }
            var mapper = new TimeLogCsvMapper(filename);

            mapper.SaveAll(timeLog);
            var fromDisk = mapper.LoadAll();

            Assert.IsNotNull(fromDisk );
            Assert.AreEqual(timeLog.Count, fromDisk.Count());
        }

        [TestMethod]
        public void Sort_Saving_Test()
        {
            var timeLog = new List<TimeLogEntry>
            {
                new TimeLogEntry
                {
                    ID = Guid.NewGuid(),
                    Revision = 0,
                    StartTime = DateTime.Now.AddMinutes(1),
                    Note = "Should save second",
                    Billable = true,
                    Project = "Second entry",
                    Client = "TestClient",
                    EndTime = DateTime.Now.AddHours(1).AddMinutes(1)
                },
                new TimeLogEntry
                {
                    ID = Guid.NewGuid(),
                    Billable = true,
                    Client = "TestClient",
                    EndTime = DateTime.Now.AddHours(1),
                    StartTime = DateTime.Now,
                    Project = "TestProject",
                    Note = "Test Note"
                }
            };
            var filename = "TestTimeFile.csv";
            if (File.Exists(filename)) { File.Delete(filename); }
            var mapper = new TimeLogCsvMapper(filename);

            mapper.SaveAll(timeLog);
            var fromDisk = mapper.LoadAll();

            Assert.IsNotNull(fromDisk);
            Assert.AreEqual(timeLog.Count, fromDisk.Count());
            Assert.IsTrue(fromDisk.ElementAt(0).StartTime < fromDisk.ElementAt(1).StartTime);
        }
    }
}
