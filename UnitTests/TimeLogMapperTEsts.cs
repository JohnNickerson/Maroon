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
    }
}
