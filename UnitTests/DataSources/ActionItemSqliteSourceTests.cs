using AssimilationSoftware.Maroon.DataSources.SQLite;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model; // Add this if ActionItemSqliteSource is in this namespace

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class ActionItemSqliteSourceTests
{
    private ActionItemSqliteSource _source;
    private ISqlConnWrapper _connection;

    public ActionItemSqliteSourceTests()
    {
        _connection = new MockSqlConnWrapper();
        _source = new ActionItemSqliteSource(_connection);
    }

    [Fact]
    public void Create_Should_InsertActionItem()
    {
        var actionItem = new ActionItem
        {
            Title = "Test Action Item",
            Context = "Test Context",
            Notes = new List<string> { "Note1", "Note2" },
            DoneDate = null,
            TickleDate = null,
            ParentId = null,
            ProjectId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = "importHash123",
            Tags = new Dictionary<string, string> { { "Priority", "High" }, { "Category", "Work" } },
            Upvotes = 1
        };

        var createdItem = _source.Insert(actionItem);
        Assert.NotNull(createdItem);
        Assert.Equal(actionItem.Title, createdItem.Title);
        Assert.Equal(actionItem.Context, createdItem.Context);
        Assert.False(createdItem.IsDeleted);
        Assert.NotEqual(Guid.Empty, createdItem.RevisionGuid);
        Assert.NotEqual(Guid.Empty, createdItem.ID);
        Assert.Equal(actionItem.ImportHash, createdItem.ImportHash);
        Assert.Equal(actionItem.Notes, createdItem.Notes);
        Assert.Equal(actionItem.Tags, createdItem.Tags);
        Assert.Equal(actionItem.Upvotes, createdItem.Upvotes);
    }

    [Fact]
    public void FindRevision_ShouldReturnCorrectItem()
    {
        var actionItem = new ActionItem
        {
            Title = "Revision Test Item",
            Context = "Test Context",
            Notes = new List<string> { "Note1" },
            DoneDate = null,
            TickleDate = null,
            ParentId = null,
            ProjectId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = null,
            Tags = new Dictionary<string, string> { { "Priority", "High" }, { "Category", "Work" } },
            Upvotes = 1
        };
        var createdItem = _source.Insert(actionItem);
        var foundItem = _source.FindRevision(createdItem.RevisionGuid);
        Assert.NotNull(foundItem);
        Assert.Equal(createdItem.ID, foundItem.ID);
        Assert.Equal(createdItem.RevisionGuid, foundItem.RevisionGuid);
        Assert.Equal(createdItem.Title, foundItem.Title);
        Assert.Equal(createdItem.Context, foundItem.Context);
        Assert.Equal(createdItem.Notes, foundItem.Notes);
        Assert.Equal(createdItem.Tags, foundItem.Tags);
        Assert.Equal(createdItem.Upvotes, foundItem.Upvotes);
    }

    [Fact]
    public void Update_Should_Insert_New_Revision()
    {
        var actionItem = new ActionItem
        {
            Title = "Test Action Item",
            Context = "Test Context",
            Notes = new List<string> { "Note1", "Note2" },
            DoneDate = null,
            TickleDate = null,
            ParentId = null,
            ProjectId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = "importHash123",
            Tags = new Dictionary<string, string> { { "Priority", "High" }, { "Category", "Work" } },
            Upvotes = 1
        };

        var createdItem = _source.Insert(actionItem);
        var firstRevisionGuid = createdItem.RevisionGuid;
        createdItem.Title = "Updated title";
        createdItem.Context = "inbox";
        createdItem.Notes.Add("Note3");
        createdItem.Tags["Priority"] = "Low";
        createdItem.Upvotes = 2;
        createdItem.UpdateRevision();
        var updatedItem = _source.Insert(createdItem);
        Assert.Equal(createdItem.Title, updatedItem.Title);
        Assert.Equal(createdItem.Context, updatedItem.Context);
        Assert.Equal(3, updatedItem.Notes.Count);
        Assert.Contains("Note3", updatedItem.Notes);
        Assert.Equal(createdItem.Tags["Priority"], updatedItem.Tags["Priority"]);
        Assert.Equal(createdItem.Upvotes, updatedItem.Upvotes);
        Assert.NotEqual(firstRevisionGuid, updatedItem.RevisionGuid);
    }

    [Fact]
    public void Delete_Should_Insert_Deletion_Revision()
    {
        var actionItem = new ActionItem
        {
            Title = "Test Action Item",
            Context = "Test Context",
            Notes = new List<string> { "Note1", "Note2" },
            DoneDate = null,
            TickleDate = null,
            ParentId = null,
            ProjectId = null,
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            ImportHash = "importHash123",
            Tags = new Dictionary<string, string> { { "Priority", "High" }, { "Category", "Work" } },
            Upvotes = 1
        };

        var createdItem = _source.Insert(actionItem);
        var firstRevisionGuid = createdItem.RevisionGuid;
        createdItem.IsDeleted = true;
        createdItem.UpdateRevision();
        var deletedItem = _source.Insert(createdItem);
        Assert.NotEqual(firstRevisionGuid, deletedItem.RevisionGuid);
        Assert.Equal(createdItem.ID, deletedItem.ID);
        Assert.True(deletedItem.IsDeleted);
    }

    [Fact]
    public void FindAll_ShouldReturnAllItems()
    {
        var item1 = new ActionItem
        {
            Title = "Item 1",
            Context = "Context 1",
            Notes = new List<string> { "Note1" },
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false
        };
        var item2 = new ActionItem
        {
            Title = "Item 2",
            Context = "Context 2",
            Notes = new List<string> { "Note2" },
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false
        };
        _source.Insert(item1);
        _source.Insert(item2);
        var allItems = _source.FindAll().ToList();
        Assert.True(allItems.Count >= 2);
        Assert.Contains(allItems, i => i.Title == "Item 1");
        Assert.Contains(allItems, i => i.Title == "Item 2");
    }

    [Fact]
    public void Purge_Should_Remove_Specified_Items()
    {
        var item1 = new ActionItem
        {
            Title = "Purge Item 1",
            Context = "Context 1",
            Notes = new List<string> { "Note1" },
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false
        };
        var item2 = new ActionItem
        {
            Title = "Purge Item 2",
            Context = "Context 2",
            Notes = new List<string> { "Note2" },
            RevisionGuid = Guid.NewGuid(),
            LastModified = DateTime.UtcNow,
            IsDeleted = false
        };
        var createdItem1 = _source.Insert(item1);
        var createdItem2 = _source.Insert(item2);
        _source.Purge(createdItem1.RevisionGuid, createdItem2.RevisionGuid);
        var foundItem1 = _source.FindRevision(createdItem1.RevisionGuid);
        var foundItem2 = _source.FindRevision(createdItem2.RevisionGuid);
        Assert.Null(foundItem1);
        Assert.Null(foundItem2);
    }

    [Fact]
    public void GetLastWriteTime_ShouldReturn_CorrectTime()
    {
        var beforeCreation = DateTime.Now;
        var actionItem = new ActionItem
        {
            Title = "Last Write Time Item",
            Context = "Context",
            Notes = new List<string> { "Note" },
            IsDeleted = false
        };
        _source.Insert(actionItem);
        var lastWriteTime = _source.GetLastWriteTime();
        var afterCreation = DateTime.Now;
        Assert.True(beforeCreation <= lastWriteTime);
        Assert.True(lastWriteTime <= afterCreation);
    }
}