using AssimilationSoftware.Maroon.DataSources.SQLite;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;
using Microsoft.Data.Sqlite;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class NoteSqliteSourceTests
{
    private NoteSqliteSource _source;
    private ISqlConnWrapper _connection;

    public NoteSqliteSourceTests()
    {
        _connection = new MockSqlConnWrapper();
        _source = new NoteSqliteSource(_connection);
    }

    [Fact]
    public void CreateNote()
    {
        var note = new Note
        {
            Text = "Test Note",
            Tags = new List<string> { "test", "note" },
            Timestamp = DateTime.UtcNow,
            ParentId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = null
        };

        var savedNote = _source.Create(note);
        Assert.NotNull(savedNote);
        Assert.Equal(note.Text, savedNote.Text);
        Assert.Equal(note.Tags, savedNote.Tags);
        Assert.Equal(note.Timestamp, savedNote.Timestamp);
        Assert.Equal(note.ParentId, savedNote.ParentId);
        Assert.Equal(note.RevisionGuid, savedNote.RevisionGuid);
        Assert.Equal(note.LastModified, savedNote.LastModified);
        Assert.False(savedNote.IsDeleted);
        Assert.Null(savedNote.ImportHash);
    }

    [Fact]
    public void UpdateNote()
    {
        var note = new Note
        {
            ID = Guid.NewGuid(),
            Text = "Initial Note",
            Tags = new List<string> { "initial" },
            Timestamp = DateTime.UtcNow,
            ParentId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = null
        };

        _source.Create(note);

        note.Text = "Updated Note";
        note.Tags.Add("updated");

        var updatedNote = _source.Update(note);
        Assert.NotNull(updatedNote);
        Assert.Equal("Updated Note", updatedNote.Text);
        Assert.Contains("updated", updatedNote.Tags);
    }

    [Fact]
    public void DeleteNote()
    {
        var note = new Note
        {
            ID = Guid.NewGuid(),
            Text = "Delete Note",
            Tags = new List<string> { "delete" },
            Timestamp = DateTime.UtcNow,
            ParentId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = null
        };

        note = _source.Create(note);
        var deletedNote = _source.Delete(note);
        Assert.NotNull(deletedNote);
        Assert.True(deletedNote.IsDeleted);
        var foundNote = _source.FindRevision(deletedNote.RevisionGuid!.Value);
        Assert.NotNull(foundNote);
        Assert.Equal(deletedNote.Text, foundNote.Text);
        Assert.Equal(deletedNote.Tags, foundNote.Tags);
    }

    [Fact]
    public void FindAllNotes()
    {
        var note1 = new Note
        {
            ID = Guid.NewGuid(),
            Text = "Find Note 1",
            Tags = new List<string> { "find", "note1" },
            Timestamp = DateTime.UtcNow,
            ParentId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = null
        };

        var note2 = new Note
        {
            ID = Guid.NewGuid(),
            Text = "Find Note 2",
            Tags = new List<string> { "find", "note2" },
            Timestamp = DateTime.UtcNow,
            ParentId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = null
        };

        _source.Create(note1);
        _source.Create(note2);

        var allNotes = _source.FindAll().ToList();
        Assert.Contains(allNotes, n => n.Text == note1.Text);
        Assert.Contains(allNotes, n => n.Text == note2.Text);
    }

    [Fact]
    public void FindRevision()
    {
        var note = new Note
        {
            ID = Guid.NewGuid(),
            Text = "Revision Note",
            Tags = new List<string> { "revision" },
            Timestamp = DateTime.UtcNow,
            ParentId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = null
        };

        _source.Create(note);

        var foundNote = _source.FindRevision(note.RevisionGuid.Value);
        Assert.NotNull(foundNote);
        Assert.Equal(note.Text, foundNote.Text);
        Assert.Equal(note.Tags, foundNote.Tags);
    }

    [Fact]
    public void GetLastWriteTime()
    {
        var note = new Note
        {
            ID = Guid.NewGuid(),
            Text = "Last Write Time Note",
            Tags = new List<string> { "last", "write", "time" },
            Timestamp = DateTime.UtcNow,
            ParentId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = new DateTime(2023, 10, 1, 12, 0, 0),
            IsDeleted = false,
            ImportHash = null
        };
        note = _source.Create(note);
        var lastWriteTime = _source.GetLastWriteTime();
        Assert.Equal(note.LastModified, lastWriteTime);
    }

    [Fact]
    public void PurgeRevision()
    {
        var note = new Note
        {
            ID = Guid.NewGuid(),
            Text = "Purge Note",
            Tags = new List<string> { "purge" },
            Timestamp = DateTime.UtcNow,
            ParentId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = null
        };

        note = _source.Create(note);
        _source.Purge(note.RevisionGuid.Value);

        var foundNote = _source.FindRevision(note.RevisionGuid.Value);
        Assert.Null(foundNote); // The note should be purged and not found
    }
}