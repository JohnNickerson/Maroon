using System.IO.Abstractions.TestingHelpers;
using AssimilationSoftware.Maroon.DataSources.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class ActionItemTextSourceTests
{
    private const string TestFileName = "D:\\Temp\\testfile.txt";
    private MockFileSystem _fileSystem;
    private ActionItemTextSource _textSource;

    public void Setup()
    {
        _fileSystem = new MockFileSystem();
        _textSource = new ActionItemTextSource(TestFileName, _fileSystem);
        _fileSystem.AddDirectory(Path.GetDirectoryName(TestFileName));
    }

    [Fact]
    public void Create_Should_Write_To_File()
    {
        Setup();
        // Arrange
        var actionItem = new ActionItem
        {
            ID = Guid.NewGuid(),
            Context = "inbox",
            Notes = new List<string> { "Test note" },
            Tags = new Dictionary<string, string> { { "tag1", "value1" } },
            Title = "Test Action Item",
            LastModified = DateTime.Now,
            RevisionGuid = Guid.NewGuid()
        };

        // Act
        _textSource.Create(actionItem);

        // Assert
        var fileContent = _fileSystem.File.ReadAllText(TestFileName);
        Assert.Contains(actionItem.Title, fileContent);
        Assert.Contains(actionItem.Context, fileContent);
        Assert.Contains(actionItem.Notes[0], fileContent);
        Assert.Contains(actionItem.Tags["tag1"], fileContent);
        Assert.Contains(actionItem.ID.ToString(), fileContent);
        Assert.Contains(actionItem.RevisionGuid.ToString(), fileContent);
        Assert.Contains(actionItem.LastModified.ToString("s"), fileContent);
    }

    [Fact]
    public void Update_Should_Append_To_File()
    {
        Setup();
        // Arrange
        var actionItem = new ActionItem
        {
            ID = Guid.NewGuid(),
            Context = "inbox",
            Notes = new List<string> { "Initial note" },
            Tags = new Dictionary<string, string> { { "tag1", "value1" } },
            Title = "Test Action Item",
            LastModified = DateTime.Now,
            RevisionGuid = Guid.NewGuid()
        };
        _textSource.Create(actionItem);

        // Act
        actionItem.Notes.Add("Updated note");
        var updatedItem = _textSource.Update(actionItem);

        // Assert
        var fileContent = _fileSystem.File.ReadAllText(TestFileName);
        Assert.Contains("Updated note", fileContent);
        Assert.Contains(actionItem.RevisionGuid.ToString(), fileContent);
        Assert.Contains(updatedItem.LastModified.ToString("s"), fileContent);
        Assert.Contains(updatedItem.RevisionGuid.ToString(), fileContent);
    }

    [Fact]
    public void Delete_Should_Append_To_File()
    {
        Setup();
        // Arrange
        var actionItem = new ActionItem
        {
            ID = Guid.NewGuid(),
            Context = "inbox",
            Notes = new List<string> { "Test note" },
            Tags = new Dictionary<string, string> { { "tag1", "value1" } },
            Title = "Test Action Item",
            LastModified = DateTime.Now,
            RevisionGuid = Guid.NewGuid()
        };
        _textSource.Create(actionItem);

        // Act
        var deletedItem = _textSource.Delete(actionItem);

        // Assert
        var fileContent = _fileSystem.File.ReadAllText(TestFileName);
        Assert.Contains(actionItem.RevisionGuid.ToString(), fileContent);
        Assert.Contains("deleted:true", fileContent);
        Assert.Contains(deletedItem.RevisionGuid.ToString(), fileContent);
    }
}