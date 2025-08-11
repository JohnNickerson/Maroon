using System.IO.Abstractions.TestingHelpers;
using AssimilationSoftware.Maroon.DataSources.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class TimeLogCsvSourceTests
{
    private TimeLogCsvSource _source;
    private MockFileSystem _fileSystem;

    private void Setup()
    {
        _fileSystem = new MockFileSystem();
        _source = new TimeLogCsvSource("D:\\Temp\\timelog.csv", _fileSystem);
        _fileSystem.AddDirectory("D:\\Temp");
    }

    [Fact]
    public void Create_ShouldAppendToFile()
    {
        Setup();
        var timeLog = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });
        Assert.NotNull(timeLog);
        Assert.True(_fileSystem.FileExists("D:\\Temp\\timelog.csv"));
        var content = _fileSystem.File.ReadAllText("D:\\Temp\\timelog.csv");
        Assert.Contains("Test Client", content);
        Assert.Contains("Test Project", content);
        Assert.Contains("Test Log", content);
        Assert.Contains(timeLog.ID.ToString(), content);
        Assert.Contains(timeLog.RevisionGuid.ToString(), content);
    }

    [Fact]
    public void Update_Should_Append_To_File()
    {
        Setup();
        var timeLog = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });

        timeLog.Note = "Updated Log";
        var updatedLog = _source.Update(timeLog);
        Assert.NotNull(updatedLog);
        Assert.True(_fileSystem.FileExists("D:\\Temp\\timelog.csv"));
        var content = _fileSystem.File.ReadAllText("D:\\Temp\\timelog.csv");
        Assert.Contains("Updated Log", content);
        Assert.Contains(updatedLog.ID.ToString(), content);
        Assert.Contains(updatedLog.RevisionGuid.ToString(), content);
    }

    [Fact]
    public void Delete_Should_Append_To_File()
    {
        Setup();
        var timeLog = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = false,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });

        // Simulate deletion
        var deletedLog = _source.Delete(timeLog);
        Assert.NotNull(deletedLog);
        Assert.True(_fileSystem.FileExists("D:\\Temp\\timelog.csv"));
        var content = _fileSystem.File.ReadAllText("D:\\Temp\\timelog.csv");
        Assert.Contains(deletedLog.ID.ToString(), content);
        Assert.Contains(deletedLog.RevisionGuid.ToString(), content);
        Assert.Contains("True", content); // Check if IsDeleted is true
    }

    [Fact]
    public void FindAll_Should_Return_AllEntries()
    {
        Setup();
        var timeLog1 = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Client A",
            Project = "Project A",
            Note = "Log A",
        });
        var timeLog2 = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now.AddHours(2),
            EndTime = DateTime.Now.AddHours(3),
            Billable = false,
            Client = "Client B",
            Project = "Project B",
            Note = "Log B",
        });
        var allEntries = _source.FindAll().ToList();
        Assert.Equal(2, allEntries.Count);
        Assert.Contains(allEntries, e => e.ID == timeLog1.ID);
        Assert.Contains(allEntries, e => e.ID == timeLog2.ID);
    }

    [Fact]
    public void FindRevision_Should_Return_CorrectEntry()
    {
        Setup();
        var timeLog = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });
        var foundEntry = _source.FindRevision(timeLog.RevisionGuid.Value);
        Assert.NotNull(foundEntry);
        Assert.Equal(timeLog.RevisionGuid, foundEntry.RevisionGuid);
        Assert.Equal(timeLog.ID, foundEntry.ID);
    }

    [Fact]
    public void FindRevision_ShouldReturnNullIfNotFound()
    {
        Setup();
        var foundEntry = _source.FindRevision(Guid.NewGuid());
        Assert.Null(foundEntry);
    }

    [Fact]
    public void GetLastWriteTime_Should_Return_LastModifiedTime()
    {
        Setup();
        var timeLog = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });
        var lastWriteTime = _source.GetLastWriteTime();
        Assert.Equal(timeLog.LastModified, lastWriteTime);
    }

    [Fact]
    public void Purge_Should_RemoveEntry()
    {
        Setup();
        var timeLog = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });
        _source.Purge(timeLog.RevisionGuid.Value);
        var foundEntry = _source.FindRevision(timeLog.RevisionGuid.Value);
        Assert.Null(foundEntry);
        Assert.False(_fileSystem.FileExists("D:\\Temp\\timelog.csv"));
    }

    [Fact]
    public void Purge_Should_Retain_Previous_Entries()
    {
        Setup();
        var timeLog1 = _source.Create(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Client A",
            Project = "Project A",
            Note = "Log A",
        });
        timeLog1.Project = "Updated Project A";
        var timeLog2 = _source.Update(timeLog1);

        _source.Purge(timeLog1.RevisionGuid.Value);
        var allEntries = _source.FindAll().ToList();
        Assert.Single(allEntries);
        Assert.Contains(allEntries, e => e.ID == timeLog2.ID);
        Assert.Contains(allEntries, e => e.RevisionGuid == timeLog2.RevisionGuid);
        Assert.Contains(allEntries, e => e.PrevRevision == timeLog1.RevisionGuid);
    }
}