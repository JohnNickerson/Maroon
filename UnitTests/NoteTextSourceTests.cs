namespace AssimilationSoftware.Maroon.UnitTests;

public class NoteTextSourceTests
{
    [Fact]
    public void ParseGuidLineFalse()
    {
        var line = "This is not a guid";
        var noteTextSource = new AssimilationSoftware.Maroon.DataSources.Text.NoteTextSource("dummy.txt");
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
        var noteTextSource = new AssimilationSoftware.Maroon.DataSources.Text.NoteTextSource("dummy.txt");
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
        var noteTextSource = new AssimilationSoftware.Maroon.DataSources.Text.NoteTextSource("dummy.txt");
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
        var noteTextSource = new AssimilationSoftware.Maroon.DataSources.Text.NoteTextSource("dummy.txt");
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
        var noteTextSource = new AssimilationSoftware.Maroon.DataSources.Text.NoteTextSource("dummy.txt");
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
        var noteTextSource = new AssimilationSoftware.Maroon.DataSources.Text.NoteTextSource("dummy.txt");
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
        var noteTextSource = new AssimilationSoftware.Maroon.DataSources.Text.NoteTextSource("dummy.txt");
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
        var noteTextSource = new AssimilationSoftware.Maroon.DataSources.Text.NoteTextSource("dummy.txt");
        var result = noteTextSource.TryParseGuidLine(line, out var guid, out var revision, out var prevRevision, out var mergeRevision, out var parentId);
        Assert.True(result);
        Assert.Null(guid);
        Assert.Equal(new Guid("12345678-1234-1234-1234-123456789012"), revision);
        Assert.Null(prevRevision);
        Assert.Null(mergeRevision);
        Assert.Equal(new Guid("67890123-4567-4567-4567-456789012345"), parentId);
    }
}