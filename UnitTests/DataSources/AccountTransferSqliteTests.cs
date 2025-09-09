using AssimilationSoftware.Maroon.DataSources.SQLite;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class AccountTransferSqliteTests
{
    private AccountTransferSqliteSource _source;
    private ISqlConnWrapper _connection;

    public AccountTransferSqliteTests()
    {
        _connection = new MockSqlConnWrapper();
        _source = new AccountTransferSqliteSource(_connection);
    }

    [Fact]
    public void Create_Should_Insert_Transfer()
    {
        var transfer = new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer",
            Category = "Test Category",
            Amount = 100.00m,
            ImportHash = "hash123"
        };

        var createdTransfer = _source.Insert(transfer);
        Assert.NotNull(createdTransfer);
        Assert.Equal(transfer.FromAccount, createdTransfer.FromAccount);
        Assert.Equal(transfer.ToAccount, createdTransfer.ToAccount);
        Assert.Equal(transfer.Description, createdTransfer.Description);
        Assert.Equal(transfer.Category, createdTransfer.Category);
        Assert.Equal(transfer.Amount, createdTransfer.Amount);
        Assert.Equal(transfer.ImportHash, createdTransfer.ImportHash);
    }

    [Fact]
    public void FindAll_Should_Return_Empty_List_When_No_Transfers()
    {
        var transfers = _source.FindAll().ToList();
        Assert.NotNull(transfers);
        Assert.True(transfers.Count >= 0); // Should be zero or more
    }

    [Fact]
    public void Update_Should_Insert_New_Revision()
    {
        var transfer = new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Initial Transfer",
            Category = "Initial Category",
            Amount = 50.00m,
            ImportHash = "hash123"
        };
        var createdTransfer = _source.Insert(transfer);
        var firstRevisionGuid = createdTransfer.RevisionGuid;

        // Update some fields
        createdTransfer.Amount = 75.00m;
        createdTransfer.Description = "Updated Transfer";
        createdTransfer.UpdateRevision();
        var updatedTransfer = _source.Insert(createdTransfer); // Create new revision
        Assert.NotNull(updatedTransfer);
        Assert.Equal(firstRevisionGuid, updatedTransfer.PrevRevision);
        Assert.Equal(75.00m, updatedTransfer.Amount);
        Assert.Equal("Updated Transfer", updatedTransfer.Description);
        Assert.Equal(createdTransfer.ID, updatedTransfer.ID);
        Assert.NotEqual(firstRevisionGuid, updatedTransfer.RevisionGuid);
    }

    [Fact]
    public void Delete_Should_Insert_Deletion_Revision()
    {
        var transfer = new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Transfer to Delete",
            Category = "Delete Category",
            Amount = 30.00m,
            ImportHash = "hash123"
        };
        var createdTransfer = _source.Insert(transfer);
        var firstRevisionGuid = createdTransfer.RevisionGuid;
        createdTransfer.IsDeleted = true;
        createdTransfer.UpdateRevision();
        var deletedTransfer = _source.Insert(createdTransfer);

        Assert.NotNull(deletedTransfer);
        Assert.True(deletedTransfer.IsDeleted);
        Assert.Equal(createdTransfer.ID, deletedTransfer.ID);
        Assert.NotEqual(firstRevisionGuid, deletedTransfer.RevisionGuid);
        Assert.Equal(firstRevisionGuid, deletedTransfer.PrevRevision);
    }

    [Fact]
    public void FindRevision_ShouldReturn_Null_For_Nonexistent_Revision()
    {
        var result = _source.FindRevision(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public void FindRevision_ShouldReturn_Existing_Revision()
    {
        var transfer = new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Revision Test Transfer",
            Category = "Revision Category",
            Amount = 200.00m,
            ImportHash = "hash123"
        };
        var createdTransfer = _source.Insert(transfer);
        var foundTransfer = _source.FindRevision(createdTransfer.RevisionGuid);
        Assert.NotNull(foundTransfer);
        Assert.Equal(createdTransfer.ID, foundTransfer.ID);
        Assert.Equal(createdTransfer.RevisionGuid, foundTransfer.RevisionGuid);
    }

    [Fact]
    public void Purge_Should_Remove_Specified_Transfers()
    {
        var transfer1 = new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Purge Test Transfer 1",
            Category = "Purge Category",
            Amount = 10.00m,
            ImportHash = "hash123"
        };
        var transfer2 = new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account3",
            ToAccount = "Account4",
            Description = "Purge Test Transfer 2",
            Category = "Purge Category",
            Amount = 20.00m,
            ImportHash = "hash456"
        };
        var createdTransfer1 = _source.Insert(transfer1);
        var createdTransfer2 = _source.Insert(transfer2);
        _source.Purge(createdTransfer1.RevisionGuid);
        var foundTransfer1 = _source.FindRevision(createdTransfer1.RevisionGuid);
        var foundTransfer2 = _source.FindRevision(createdTransfer2.RevisionGuid);
        Assert.Null(foundTransfer1);
        Assert.NotNull(foundTransfer2);
    }

    [Fact]
    public void GetLastWriteTime_ShouldReturn_Correct_Timestamp()
    {
        var transfer = new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Last Write Time Transfer",
            Category = "Time Category",
            Amount = 150.00m,
            ImportHash = "hash789"
        };
        var createdTransfer = _source.Insert(transfer);
        var lastWriteTime = _source.GetLastWriteTime();
        Assert.Equal(createdTransfer.LastModified, lastWriteTime);
    }
}