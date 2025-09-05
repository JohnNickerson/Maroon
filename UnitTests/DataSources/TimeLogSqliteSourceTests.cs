using AssimilationSoftware.Maroon.DataSources.SQLite;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class TimeLogSqliteSourceTests
{
    private TimeLogSqliteSource _source;
    private ISqlConnWrapper _connection;

    public TimeLogSqliteSourceTests()
    {
        _connection = new MockSqlConnWrapper();
        _source = new TimeLogSqliteSource(_connection);
    }

    [Fact]
    public void Create_Should_InsertTimeLogEntry()
    {
        var entry = new TimeLogEntry
        {
            Billable = true,
            Client = "ClientA",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-1),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Test entry",
            Project = "ProjectA",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash123"
        };

        var createdEntry = _source.Create(entry);
        Assert.NotNull(createdEntry);
        Assert.Equal(entry.Billable, createdEntry.Billable);
        Assert.Equal(entry.Client, createdEntry.Client);
        Assert.Equal(entry.EndTime, createdEntry.EndTime);
        Assert.Equal(entry.StartTime, createdEntry.StartTime);
        Assert.False(createdEntry.IsDeleted);
        Assert.NotEqual(Guid.Empty, createdEntry.RevisionGuid);
        Assert.NotEqual(Guid.Empty, createdEntry.ID);
        Assert.Equal(entry.ImportHash, createdEntry.ImportHash);
    }

    [Fact]
    public void FindRevision_ShouldReturnCorrectEntry()
    {
        var entry = new TimeLogEntry
        {
            Billable = false,
            Client = "ClientB",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-2),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Revision test entry",
            Project = "ProjectB",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash456"
        };

        var createdEntry = _source.Create(entry);
        var fetchedEntry = _source.FindRevision(createdEntry.RevisionGuid);

        Assert.NotNull(fetchedEntry);
        Assert.Equal(createdEntry.ID, fetchedEntry.ID);
        Assert.Equal(createdEntry.RevisionGuid, fetchedEntry.RevisionGuid);
        Assert.Equal(createdEntry.Client, fetchedEntry.Client);
    }

    [Fact]
    public void Delete_Should_MarkEntryAsDeleted()
    {
        var entry = new TimeLogEntry
        {
            Billable = true,
            Client = "ClientC",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-3),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Delete test entry",
            Project = "ProjectC",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash789"
        };

        var createdEntry = _source.Create(entry);
        var deletedEntry = _source.Delete(createdEntry);

        Assert.NotNull(deletedEntry);
        Assert.True(deletedEntry.IsDeleted);
        Assert.Equal(createdEntry.ID, deletedEntry.ID);
        Assert.NotEqual(createdEntry.RevisionGuid, deletedEntry.RevisionGuid);
        Assert.Equal(createdEntry.RevisionGuid, deletedEntry.PrevRevision);
    }

    [Fact]
    public void GetLastWriteTime_ShouldReturnRecentTime()
    {
        var beforeCreation = DateTime.Now;
        var entry = new TimeLogEntry
        {
            Billable = false,
            Client = "ClientD",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-4),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Last write time test entry",
            Project = "ProjectD",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash101"
        };

        _source.Create(entry);
        var lastWriteTime = _source.GetLastWriteTime();
        var afterCreation = DateTime.Now;

        Assert.True(lastWriteTime >= beforeCreation && lastWriteTime <= afterCreation);
    }

    [Fact]
    public void FindAll_ShouldReturnAllEntries()
    {
        var entry1 = new TimeLogEntry
        {
            Billable = true,
            Client = "ClientE",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-1),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Find all test entry 1",
            Project = "ProjectE",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash111"
        };
        var entry2 = new TimeLogEntry
        {
            Billable = false,
            Client = "ClientF",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-2),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Find all test entry 2",
            Project = "ProjectF",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash222"
        };
        _source.Create(entry1);
        _source.Create(entry2);
        var allEntries = _source.FindAll().ToList();
        Assert.True(allEntries.Count >= 2);
        Assert.Contains(allEntries, e => e.Note == "Find all test entry 1");
        Assert.Contains(allEntries, e => e.Note == "Find all test entry 2");
    }

    [Fact]
    public void Purge_Should_Remove_SpecifiedEntries()
    {
        var entry1 = new TimeLogEntry
        {
            Billable = true,
            Client = "ClientG",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-1),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Purge test entry 1",
            Project = "ProjectG",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash333"
        };
        var entry2 = new TimeLogEntry
        {
            Billable = false,
            Client = "ClientH",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-2),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Purge test entry 2",
            Project = "ProjectH",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash444"
        };
        var createdEntry1 = _source.Create(entry1);
        var createdEntry2 = _source.Create(entry2);
        _source.Purge(createdEntry1.RevisionGuid, createdEntry2.RevisionGuid);
        var foundEntry1 = _source.FindRevision(createdEntry1.RevisionGuid);
        var foundEntry2 = _source.FindRevision(createdEntry2.RevisionGuid);
        Assert.Null(foundEntry1);
        Assert.Null(foundEntry2);
    }

    [Fact]
    public void Update_Should_Insert_New_Revision()
    {
        var entry = new TimeLogEntry
        {
            Billable = true,
            Client = "ClientI",
            EndTime = DateTime.Now,
            StartTime = DateTime.Now.AddHours(-1),
            IsDeleted = false,
            LastModified = DateTime.Now,
            Note = "Update test entry",
            Project = "ProjectI",
            PrevRevision = null,
            RevisionGuid = Guid.NewGuid(),
            ImportHash = "importHash555"
        };
        var createdEntry = _source.Create(entry);
        createdEntry.Note = "Updated note";
        var updatedEntry = _source.Update(createdEntry);
        Assert.NotNull(updatedEntry);
        Assert.Equal(createdEntry.ID, updatedEntry.ID);
        Assert.NotEqual(createdEntry.RevisionGuid, updatedEntry.RevisionGuid);
    }
}