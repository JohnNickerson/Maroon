using System.IO.Abstractions.TestingHelpers;
using AssimilationSoftware.Maroon.DataSources.Text;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class NoteTextSourceTests
{
    [Fact]
    public void ParseGuidLineFalse()
    {
        var line = "This is not a guid";
        var noteTextSource = new NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.False(result);
        Assert.Null(guid);
        Assert.Null(revision);
        Assert.Null(prevRevision);
        Assert.Null(mergeRevision);
        Assert.Null(parentId);
    }

    [Fact]
    public void ParseGuidLine_IdOnly()
    {
        var line = "12345678-1234-1234-1234-123456789012";
        var noteTextSource = new NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.True(result);
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), guid);
        Assert.Null(revision);
        Assert.Null(prevRevision);
        Assert.Null(mergeRevision);
        Assert.Null(parentId);
    }

    [Fact]
    public void ParseGuidLine_RevisionOnly()
    {
        var line = "[12345678-1234-1234-1234-123456789012]";
        var noteTextSource = new NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.True(result);
        Assert.Null(guid);
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), revision);
        Assert.Null(prevRevision);
        Assert.Null(mergeRevision);
        Assert.Null(parentId);
    }

    [Fact]
    public void ParseGuidLine_RevisionAndPrev()
    {
        var line = "[12345678-1234-1234-1234-123456789012;23456789-2345-2345-2345-234567890123]";
        var noteTextSource = new NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.True(result);
        Assert.Null(guid);
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), revision);
        Assert.Equal(new Guid("23456789-2345-2345-2345-234567890123"), prevRevision);
        Assert.Null(mergeRevision);
        Assert.Null(parentId);
    }

    [Fact]
    public void ParseGuidLine_RevisionPrevAndMerge()
    {
        var line = "[12345678-1234-1234-1234-123456789012;23456789-2345-2345-2345-234567890123;34567890-3456-3456-3456-345678901234]";
        var noteTextSource = new NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.True(result);
        Assert.Null(guid);
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), revision);
        Assert.Equal(new Guid("23456789-2345-2345-2345-234567890123"), prevRevision);
        Assert.Equal(new Guid("34567890-3456-3456-3456-345678901234"), mergeRevision);
        Assert.Null(parentId);
    }

    [Fact]
    public void ParseGuidLine_RevisionPrevMergeAndParent()
    {
        var line = "[12345678-1234-1234-1234-123456789012;23456789-2345-2345-2345-234567890123;34567890-3456-3456-3456-345678901234]:67890123-4567-4567-4567-456789012345";
        var noteTextSource = new NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.True(result);
        Assert.Null(guid);
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), revision);
        Assert.Equal(new Guid("23456789-2345-2345-2345-234567890123"), prevRevision);
        Assert.Equal(new Guid("34567890-3456-3456-3456-345678901234"), mergeRevision);
        Assert.Equal(new Guid("67890123-4567-4567-4567-456789012345"), parentId);
    }

    [Fact]
    public void ParseGuidLine_RevisionPrevAndParent()
    {
        var line = "[12345678-1234-1234-1234-123456789012;23456789-2345-2345-2345-234567890123]:67890123-4567-4567-4567-456789012345";
        var noteTextSource = new NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.True(result);
        Assert.Null(guid);
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), revision);
        Assert.Equal(new Guid("23456789-2345-2345-2345-234567890123"), prevRevision);
        Assert.Null(mergeRevision);
        Assert.Equal(new Guid("67890123-4567-4567-4567-456789012345"), parentId);
    }

    [Fact]
    public void ParseGuidLine_RevisionAndParent()
    {
        var line = "[12345678-1234-1234-1234-123456789012]:67890123-4567-4567-4567-456789012345";
        var noteTextSource = new NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.True(result);
        Assert.Null(guid);
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), revision);
        Assert.Null(prevRevision);
        Assert.Null(mergeRevision);
        Assert.Equal(new Guid("67890123-4567-4567-4567-456789012345"), parentId);
    }

    [Fact]
    public void CreateNote()
    {
        MockFileSystem fileSystem = new();
        var noteTextSource = new NoteTextSource("D:\\Temp\\dummy.txt", fileSystem);
        fileSystem.AddDirectory("D:\\Temp");
        var note = noteTextSource.Insert(new Model.Note()
        {
            Text = "This is a test note",
            Tags = new List<string> { "test", "note" },
        });
        Assert.NotNull(note);
        Assert.Equal("This is a test note", note.Text);
        Assert.Contains("test", note.Tags);
        Assert.Contains("note", note.Tags);
        Assert.True(fileSystem.FileExists("D:\\Temp\\dummy.txt"));
        var fileContent = fileSystem.File.ReadAllText("D:\\Temp\\dummy.txt");
        Assert.Contains("This is a test note", fileContent);
        Assert.Contains("#test", fileContent);
        Assert.Contains("#note", fileContent);
        Assert.Contains(note.Timestamp.ToString("s"), fileContent);
        Assert.Contains(note.ID.ToString(), fileContent);
    }

    [Fact]
    public void UpdateNote()
    {
        MockFileSystem fileSystem = new();
        var noteTextSource = new NoteTextSource("D:\\Temp\\dummy.txt", fileSystem);
        fileSystem.AddDirectory("D:\\Temp");
        var note = noteTextSource.Insert(new Model.Note()
        {
            Text = "This is a test note",
            Tags = new List<string> { "test", "note" },
        });
        note.Text = "This is an updated test note";
        note.Tags = new List<string> { "updated", "note" };
        var updatedNote = noteTextSource.Insert(note);
        Assert.NotNull(updatedNote);
        Assert.Equal("This is an updated test note", updatedNote.Text);
        Assert.Contains("updated", updatedNote.Tags);
        Assert.Contains("note", updatedNote.Tags);
        Assert.True(fileSystem.FileExists("D:\\Temp\\dummy.txt"));
        var fileContent = fileSystem.File.ReadAllText("D:\\Temp\\dummy.txt");
        Assert.Contains("This is an updated test note", fileContent);
        Assert.Contains("#updated", fileContent);
        Assert.Contains("#note", fileContent);
        Assert.Contains(updatedNote.Timestamp.ToString("s"), fileContent);
        Assert.Contains(updatedNote.ID.ToString(), fileContent);
        Assert.Contains(updatedNote.RevisionGuid.ToString(), fileContent);
        Assert.Contains("This is a test note", fileContent);
        Assert.Contains("#test", fileContent);
        Assert.Contains("#note", fileContent);
        Assert.Contains(note.Timestamp.ToString("s"), fileContent);
        Assert.Contains(note.ID.ToString(), fileContent);
        Assert.Contains(note.RevisionGuid.ToString(), fileContent);
    }

    [Fact]
    public void DeleteNote()
    {
        MockFileSystem fileSystem = new();
        var noteTextSource = new NoteTextSource("D:\\Temp\\dummy.txt", fileSystem);
        fileSystem.AddDirectory("D:\\Temp");
        var note = noteTextSource.Insert(new Model.Note()
        {
            Text = "This is a test note",
            Tags = new List<string> { "test", "note" },
        });
        note.IsDeleted = true;
        note.UpdateRevision();
        var deletedNote = noteTextSource.Insert(note);
        Assert.NotNull(deletedNote);
        Assert.True(deletedNote.IsDeleted);
        Assert.True(fileSystem.FileExists("D:\\Temp\\dummy.txt"));
        var fileContent = fileSystem.File.ReadAllText("D:\\Temp\\dummy.txt");
        Assert.Contains("This is a test note", fileContent);
        Assert.Contains("#test", fileContent);
        Assert.Contains("#note", fileContent);
        Assert.Contains(deletedNote.Timestamp.ToString("s"), fileContent);
        Assert.Contains(deletedNote.ID.ToString(), fileContent);
        Assert.Contains(deletedNote.RevisionGuid.ToString(), fileContent);
    }

    [Fact]
    public void FindAllNotes()
    {
        MockFileSystem fileSystem = new();
        var noteTextSource = new NoteTextSource("D:\\Temp\\dummy.txt", fileSystem);
        fileSystem.AddDirectory("D:\\Temp");
        noteTextSource.Insert(new Model.Note()
        {
            Text = "First note",
            Tags = new List<string> { "first" },
        });
        noteTextSource.Insert(new Model.Note()
        {
            Text = "Second note",
            Tags = new List<string> { "second" },
        });
        var notes = noteTextSource.FindAll().ToList();
        Assert.Equal(2, notes.Count);
        Assert.Contains(notes, n => n.Text == "First note");
        Assert.Contains(notes, n => n.Text == "Second note");
    }

    [Fact]
    public void FindNoteByRevisionId()
    {
        MockFileSystem fileSystem = new();
        var noteTextSource = new NoteTextSource("D:\\Temp\\dummy.txt", fileSystem);
        fileSystem.AddDirectory("D:\\Temp");
        var note = noteTextSource.Insert(new Model.Note()
        {
            Text = "This is a test note",
            Tags = new List<string> { "test", "note" },
        });
        var foundNote = noteTextSource.FindRevision(note.RevisionGuid);
        Assert.NotNull(foundNote);
        Assert.Equal(note.ID, foundNote.ID);
        Assert.Equal(note.Text, foundNote.Text);
    }

    [Fact]
    public void GetLastWriteTime()
    {
        MockFileSystem fileSystem = new();
        var noteTextSource = new NoteTextSource("D:\\Temp\\dummy.txt", fileSystem);
        fileSystem.AddDirectory("D:\\Temp");
        fileSystem.AddFile("D:\\Temp\\dummy.txt", new MockFileData(@"2023-10-01T12:00:00
This is a test note
#test #note
12345678-1234-1234-1234-123456789012
"));
        var lastWriteTime = noteTextSource.GetLastWriteTime();
        Assert.Equal(new DateTime(2023, 10, 1, 12, 0, 0), lastWriteTime);
    }

    [Fact]
    public void TestPurgeRevision()
    {
        MockFileSystem fileSystem = new();
        var noteTextSource = new NoteTextSource("D:\\Temp\\dummy.txt", fileSystem);
        fileSystem.AddDirectory("D:\\Temp");
        var note = noteTextSource.Insert(new Model.Note()
        {
            Text = "This is a test note",
            Tags = new List<string> { "test", "note" },
        });
        var revisionId = note.RevisionGuid;
        noteTextSource.Purge(revisionId);
        var foundNote = noteTextSource.FindRevision(revisionId);
        Assert.Null(foundNote);
        Assert.False(fileSystem.FileExists("D:\\Temp\\dummy.txt"));
    }
}