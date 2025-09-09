using System.IO.Abstractions.TestingHelpers;
using AssimilationSoftware.Maroon.DataSources.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class ActionItemTextSourceTests
{
    private string TestFileName;
    private MockFileSystem _fileSystem;
    private ActionItemTextSource _textSource;

    private void Setup()
    {
        _fileSystem = new MockFileSystem();
        TestFileName = _fileSystem.Path.Combine(".", "testfile.txt");//"D:\\Temp\\testfile.txt";
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
        _textSource.Insert(actionItem);

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
        _textSource.Insert(actionItem);

        // Act
        actionItem.Notes.Add("Updated note");
        var updatedItem = _textSource.Insert(actionItem);

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
        _textSource.Insert(actionItem);

        // Act
        actionItem.IsDeleted = true;
        actionItem.UpdateRevision();
        var deletedItem = _textSource.Insert(actionItem);

        // Assert
        var fileContent = _fileSystem.File.ReadAllText(TestFileName);
        Assert.Contains(actionItem.RevisionGuid.ToString(), fileContent);
        Assert.Contains("deleted:true", fileContent);
        Assert.Contains(deletedItem.RevisionGuid.ToString(), fileContent);
    }

    [Fact]
    public void FindAll_Should_Return_AllEntries()
    {
        Setup();
        // Arrange
        var actionItem1 = new ActionItem
        {
            ID = Guid.NewGuid(),
            Context = "inbox",
            Notes = new List<string> { "First note" },
            Tags = new Dictionary<string, string> { { "tag1", "value1" } },
            Title = "First Action Item",
            LastModified = DateTime.Now,
            RevisionGuid = Guid.NewGuid()
        };
        var actionItem2 = new ActionItem
        {
            ID = Guid.NewGuid(),
            Context = "inbox",
            Notes = new List<string> { "Second note" },
            Tags = new Dictionary<string, string> { { "tag2", "value2" } },
            Title = "Second Action Item",
            LastModified = DateTime.Now,
            RevisionGuid = Guid.NewGuid()
        };
        _textSource.Insert(actionItem1);
        _textSource.Insert(actionItem2);

        // Act
        var allItems = _textSource.FindAll();

        // Assert
        Assert.Contains(allItems, item => item.ID == actionItem1.ID);
        Assert.Contains(allItems, item => item.ID == actionItem2.ID);
        Assert.Equal(2, allItems.Count());
    }

    [Fact]
    public void FindRevision_Should_Return_CorrectEntry()
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
        _textSource.Insert(actionItem);
        // Act
        var foundItem = _textSource.FindRevision(actionItem.RevisionGuid);
        // Assert
        Assert.NotNull(foundItem);
        Assert.Equal(actionItem.ID, foundItem.ID);
        Assert.Equal(actionItem.Title, foundItem.Title);
    }

    [Fact]
    public void FindRevision_Should_Return_Null_For_NonExistentEntry()
    {
        Setup();
        // Act
        var foundItem = _textSource.FindRevision(Guid.NewGuid());
        // Assert
        Assert.Null(foundItem);
    }

    [Fact]
    public void GetLastWriteTime_Should_Return_CorrectTime()
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
        _textSource.Insert(actionItem);
        // Act
        var lastWriteTime = _textSource.GetLastWriteTime();
        // Assert
        Assert.True(lastWriteTime <= DateTime.Now);
        Assert.True(lastWriteTime >= actionItem.LastModified);
    }

    [Fact]
    public void Purge_Should_Remove_Entry()
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
        _textSource.Insert(actionItem);
        // Act
        _textSource.Purge(actionItem.RevisionGuid);
        // Assert
        var foundItem = _textSource.FindRevision(actionItem.RevisionGuid);
        Assert.Null(foundItem);
        Assert.False(_fileSystem.File.Exists(TestFileName));
    }
}