using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;
using Xunit;

namespace UnitTests
{
    
    public class TimeLogMapperTests
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
            var mapper = new TimeLogCsvMapper();

            mapper.Write(timeLog, filename);
            var fromDisk = mapper.Read(filename);

            Assert.NotNull(fromDisk );
            Assert.Equal(timeLog.Count, fromDisk.Count());
            Cleanup();
        }

        [Fact]
        public void Sort_Saving_Test()
        {
            Cleanup();
            var timeLog = new List<TimeLogEntry>
            {
                new TimeLogEntry
                {
                    ID = Guid.NewGuid(),
                    RevisionGuid = Guid.NewGuid(),
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
            var mapper = new TimeLogCsvMapper();

            mapper.Write(timeLog.OrderBy(t => t.StartTime), filename);
            var fromDisk = mapper.Read(filename).ToArray();

            Assert.NotNull(fromDisk);
            Assert.Equal(timeLog.Count, fromDisk.Count());
            Assert.True(fromDisk.ElementAt(0).StartTime < fromDisk.ElementAt(1).StartTime);
            Cleanup();
        }
    }
}
