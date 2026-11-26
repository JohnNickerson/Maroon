using System.IO.Abstractions.TestingHelpers;
using AssimilationSoftware.Maroon.DataSources.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class TimeLogCsvSourceTests
{
    private const string ColumnHeaders = "StartTime,EndTime,Client,Project,Note,Billable,LastModified,IsDeleted,ID,RevisionGuid,PrevRevision,MergeRevision";
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
        var callerLastModified = new DateTime(2000, 1, 1);
        var timeLog = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            LastModified = callerLastModified,
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });
        Assert.NotNull(timeLog);
        Assert.True(_fileSystem.FileExists("D:\\Temp\\timelog.csv"));
        var content = _fileSystem.File.ReadAllText("D:\\Temp\\timelog.csv");
        Assert.StartsWith(ColumnHeaders + Environment.NewLine, content);
        Assert.NotEqual(callerLastModified, timeLog.LastModified);
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
        var timeLog = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });

        timeLog.Note = "Updated Log";
        var updatedLog = _source.Insert(timeLog);
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
        var timeLog = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = false,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });

        // Simulate deletion
        timeLog.IsDeleted = true;
        var deletedLog = _source.Insert(timeLog);
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
        var timeLog1 = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Client A",
            Project = "Project A",
            Note = "Log A",
        });
        var timeLog2 = _source.Insert(new TimeLogEntry
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
        var timeLog = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });
        var foundEntry = _source.FindRevision(timeLog.RevisionGuid);
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
        var timeLog = _source.Insert(new TimeLogEntry
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
        var timeLog = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Test Client",
            Project = "Test Project",
            Note = "Test Log",
        });
        _source.Purge(timeLog.RevisionGuid);
        var foundEntry = _source.FindRevision(timeLog.RevisionGuid);
        Assert.Null(foundEntry);
        Assert.False(_fileSystem.FileExists("D:\\Temp\\timelog.csv"));
    }

    [Fact]
    public void Purge_Should_Retain_Previous_Entries()
    {
        Setup();
        var timeLog1 = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Billable = true,
            Client = "Client A",
            Project = "Project A",
            Note = "Log A",
        });
        var firstRevisionGuid = timeLog1.RevisionGuid;
        timeLog1.Project = "Updated Project A";
        timeLog1.UpdateRevision();
        var timeLog2 = _source.Insert(timeLog1);

        _source.Purge(firstRevisionGuid);
        var allEntries = _source.FindAll().ToList();
        Assert.Single(allEntries);
        Assert.Contains(allEntries, e => e.ID == timeLog2.ID);
        Assert.Contains(allEntries, e => e.RevisionGuid == timeLog2.RevisionGuid);
        Assert.Contains(allEntries, e => e.PrevRevision == firstRevisionGuid);
    }

    [Fact]
    public void FindAll_Should_ReadLegacyHeaderlessRows()
    {
        Setup();
        var id = Guid.NewGuid();
        var revisionGuid = Guid.NewGuid();
        var startTime = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var endTime = startTime.AddHours(1);
        var lastModified = startTime.AddDays(1);
        var line = string.Join(",", new[]
        {
            startTime.ToString("O"),
            endTime.ToString("O"),
            "Legacy Client",
            "Legacy Project",
            "Legacy Note",
            "True",
            lastModified.ToString("O"),
            "False",
            id.ToString(),
            revisionGuid.ToString(),
            ""
        });
        _fileSystem.File.WriteAllText("D:\\Temp\\timelog.csv", line + Environment.NewLine);

        var entries = _source.FindAll().ToList();

        var entry = Assert.Single(entries);
        Assert.Equal(id, entry.ID);
        Assert.Equal(revisionGuid, entry.RevisionGuid);
        Assert.Null(entry.PrevRevision);
        Assert.Null(entry.MergeRevision);
        Assert.Equal("Legacy Note", entry.Note);
    }

    [Fact]
    public void FindAll_Should_ReadHeaderedFile()
    {
        Setup();
        var id = Guid.NewGuid();
        var revisionGuid = Guid.NewGuid();
        var prevRevision = Guid.NewGuid();
        var mergeRevision = Guid.NewGuid();
        var startTime = new DateTime(2024, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        var endTime = startTime.AddHours(1);
        var lastModified = startTime.AddDays(1);
        var line = string.Join(",", new[]
        {
            startTime.ToString("O"),
            endTime.ToString("O"),
            "Headered Client",
            "Headered Project",
            "Headered Note",
            "False",
            lastModified.ToString("O"),
            "False",
            id.ToString(),
            revisionGuid.ToString(),
            prevRevision.ToString(),
            mergeRevision.ToString()
        });
        _fileSystem.File.WriteAllLines("D:\\Temp\\timelog.csv", new[] { ColumnHeaders, line });

        var entries = _source.FindAll().ToList();

        var entry = Assert.Single(entries);
        Assert.Equal(id, entry.ID);
        Assert.Equal(prevRevision, entry.PrevRevision);
        Assert.Equal(mergeRevision, entry.MergeRevision);
    }

    [Fact]
    public void MergeRevision_Should_RoundTrip()
    {
        Setup();
        var mergeRevision = Guid.NewGuid();
        _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Billable = true,
            Client = "Client",
            Project = "Project",
            Note = "Note",
            MergeRevision = mergeRevision
        });

        var reloaded = new TimeLogCsvSource("D:\\Temp\\timelog.csv", _fileSystem);
        var entry = Assert.Single(reloaded.FindAll());

        Assert.Equal(mergeRevision, entry.MergeRevision);
    }

    [Fact]
    public void Purge_Should_RetainHeaderAndRevisionFields()
    {
        Setup();
        var previousRevision = Guid.NewGuid();
        var mergeRevision = Guid.NewGuid();
        var retained = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Billable = true,
            Client = "Retained Client",
            Project = "Retained Project",
            Note = "Retained Note",
            PrevRevision = previousRevision,
            MergeRevision = mergeRevision
        });
        var purged = _source.Insert(new TimeLogEntry
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Billable = false,
            Client = "Purged Client",
            Project = "Purged Project",
            Note = "Purged Note"
        });

        _source.Purge(purged.RevisionGuid);

        var lines = _fileSystem.File.ReadAllLines("D:\\Temp\\timelog.csv");
        Assert.Equal(ColumnHeaders, lines[0]);
        var reloaded = new TimeLogCsvSource("D:\\Temp\\timelog.csv", _fileSystem);
        var entry = Assert.Single(reloaded.FindAll());
        Assert.Equal(retained.RevisionGuid, entry.RevisionGuid);
        Assert.Equal(previousRevision, entry.PrevRevision);
        Assert.Equal(mergeRevision, entry.MergeRevision);
        Assert.Equal(retained.LastModified, entry.LastModified);
    }

    [Fact]
    public void InsertMigrated_Should_PreserveCallerLastModified()
    {
        Setup();
        var importedLastModified = new DateTime(2020, 5, 6, 7, 8, 9, DateTimeKind.Utc);
        var timeLog = new TimeLogEntry
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            LastModified = importedLastModified,
            Billable = true,
            Client = "Imported Client",
            Project = "Imported Project",
            Note = "Imported Note"
        };

        _source.InsertMigrated(timeLog);

        Assert.Equal(importedLastModified, timeLog.LastModified);
        var reloaded = new TimeLogCsvSource("D:\\Temp\\timelog.csv", _fileSystem);
        Assert.Equal(importedLastModified, Assert.Single(reloaded.FindAll()).LastModified);
    }
}