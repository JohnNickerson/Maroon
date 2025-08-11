using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AssimilationSoftware.Maroon.DataSources.Text;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class AccountTransferCsvSourceTests
{
    private AccountTransferCsvSource _source;
    private MockFileSystem _fileSystem;

    private void Setup()
    {
        _fileSystem = new MockFileSystem();
        _source = new AccountTransferCsvSource("D:\\Temp\\test.csv", _fileSystem);
        _fileSystem.AddDirectory("D:\\Temp");
    }

    [Fact]
    public void Create_ShouldAppendToFile()
    {
        Setup();
        var transfer = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer",
            Category = "Test Category",
            Amount = 100.00m,
            ImportHash = "hash123"
        });
        Assert.NotNull(transfer);
        Assert.True(_fileSystem.FileExists("D:\\Temp\\test.csv"));
        var content = _fileSystem.File.ReadAllText("D:\\Temp\\test.csv");
        Assert.Contains("Account1", content);
        Assert.Contains("Account2", content);
        Assert.Contains("Test Transfer", content);
        Assert.Contains("100.00", content);
        Assert.Contains("Test Category", content);
        Assert.Contains("hash123", content);
        Assert.Contains(transfer.ID.ToString(), content);
        Assert.Contains(transfer.RevisionGuid.ToString(), content);
    }

    [Fact]
    public void Update_Should_Append_To_File()
    {
        Setup();
        var transfer = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer",
            Category = "Test Category",
            Amount = 100.00m,
            ImportHash = "hash123"
        });

        // Simulate an update
        transfer.Amount = 200.00m;
        var revised = _source.Update(transfer);

        var content = _fileSystem.File.ReadAllText("D:\\Temp\\test.csv");

        Assert.NotNull(revised);
        Assert.Contains("100.00", content);
        Assert.Contains(transfer.ID.ToString(), content);
        Assert.Contains(transfer.RevisionGuid.ToString(), content);
        Assert.Contains(transfer.FromAccount, content);
        Assert.Contains(transfer.ToAccount, content);
        Assert.Contains(transfer.Description, content);
        Assert.Contains(transfer.Category, content);
        Assert.Contains(transfer.ImportHash, content);

        Assert.Contains(transfer.Date.ToString("yyyy-MM-dd"), content);
        Assert.Contains("200.00", content);
        Assert.Contains(revised.RevisionGuid.ToString(), content);
    }

    [Fact]
    public void Delete_Should_Append_To_File()
    {
        Setup();
        var transfer = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer",
            Category = "Test Category",
            Amount = 100.00m,
            ImportHash = "hash123"
        });

        // Simulate a delete
        var deletedTransfer = _source.Delete(transfer);

        var content = _fileSystem.File.ReadAllText("D:\\Temp\\test.csv");

        Assert.NotNull(deletedTransfer);
        Assert.Contains("100.00", content);
        Assert.Contains(transfer.ID.ToString(), content);
        Assert.Contains(transfer.RevisionGuid.ToString(), content);
        Assert.Contains(transfer.FromAccount, content);
        Assert.Contains(transfer.ToAccount, content);
        Assert.Contains(transfer.Description, content);
        Assert.Contains(transfer.Category, content);
        Assert.Contains(transfer.ImportHash, content);
        Assert.Contains(transfer.Date.ToString("yyyy-MM-dd"), content);
        Assert.Contains("False", content); // Assuming IsDeleted is represented as "false" in the CSV

        Assert.Contains(deletedTransfer.RevisionGuid.ToString(), content);
        Assert.True(deletedTransfer.IsDeleted);
        Assert.Equal(deletedTransfer.PrevRevision, transfer.RevisionGuid);
        Assert.Contains("True", content); // Assuming IsDeleted is represented as "true" in the CSV
    }

    [Fact]
    public void FindAllNotes_ShouldReturnAllNotes()
    {
        Setup();
        var transfer1 = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer 1",
            Category = "Test Category 1",
            Amount = 100.00m,
            ImportHash = "hash123"
        });

        var transfer2 = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account3",
            ToAccount = "Account4",
            Description = "Test Transfer 2",
            Category = "Test Category 2",
            Amount = 200.00m,
            ImportHash = "hash456"
        });

        var allTransfers = _source.FindAll().ToList();

        Assert.Equal(2, allTransfers.Count);
        Assert.Contains(allTransfers, t => t.ID == transfer1.ID);
        Assert.Contains(allTransfers, t => t.ID == transfer2.ID);
    }

    [Fact]
    public void Can_Find_By_RevisionGuid()
    {
        Setup();
        var transfer = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer",
            Category = "Test Category",
            Amount = 100.00m,
            ImportHash = "hash123"
        });

        var foundTransfer = _source.FindRevision(transfer.RevisionGuid.Value);
        Assert.NotNull(foundTransfer);
        Assert.Equal(transfer.ID, foundTransfer.ID);
    }

    [Fact]
    public void FindRevision_ShouldReturnNullIfNotFound()
    {
        Setup();
        var transfer = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer",
            Category = "Test Category",
            Amount = 100.00m,
            ImportHash = "hash123"
        });
        var nonExistentGuid = Guid.NewGuid();
        var foundTransfer = _source.FindRevision(nonExistentGuid);
        Assert.Null(foundTransfer);
    }

    [Fact]
    public void GetLastWriteTime_ShouldReturnLastWriteTime()
    {
        Setup();
        var transfer = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer",
            Category = "Test Category",
            Amount = 100.00m,
            ImportHash = "hash123"
        });

        var lastWriteTime = _source.GetLastWriteTime();
        Assert.Equal(transfer.LastModified, lastWriteTime);
    }

    [Fact]
    public void Purge_Should_Remove_Revision()
    {
        Setup();
        var transfer = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer",
            Category = "Test Category",
            Amount = 100.00m,
            ImportHash = "hash123"
        });

        // Purge the transfer
        _source.Purge(transfer.RevisionGuid.Value);

        // Verify that the transfer is no longer found
        var foundTransfer = _source.FindRevision(transfer.RevisionGuid.Value);
        Assert.Null(foundTransfer);
        Assert.False(_fileSystem.FileExists("D:\\Temp\\test.csv"));
    }

    [Fact]
    public void Purge_Should_Retain_Previous_Entries()
    {
        Setup();
        var transfer1 = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now,
            FromAccount = "Account1",
            ToAccount = "Account2",
            Description = "Test Transfer 1",
            Category = "Test Category 1",
            Amount = 100.00m,
            ImportHash = "hash123"
        });

        var transfer2 = _source.Create(new AccountTransfer
        {
            Date = DateTime.Now.AddDays(1),
            FromAccount = "Account3",
            ToAccount = "Account4",
            Description = "Test Transfer 2",
            Category = "Test Category 2",
            Amount = 200.00m,
            ImportHash = "hash456"
        });

        // Purge the first transfer
        _source.Purge(transfer1.RevisionGuid.Value);

        // Verify that the second transfer is still found
        var foundTransfer2 = _source.FindRevision(transfer2.RevisionGuid.Value);
        Assert.NotNull(foundTransfer2);
        Assert.Equal(transfer2.ID, foundTransfer2.ID);
        Assert.True(_fileSystem.FileExists("D:\\Temp\\test.csv"));
        var content = _fileSystem.File.ReadAllText("D:\\Temp\\test.csv");
        Assert.Contains(transfer2.FromAccount, content);
        Assert.Contains(transfer2.ToAccount, content);
        Assert.Contains(transfer2.Description, content);
        Assert.Contains(transfer2.Category, content);
        Assert.Contains(transfer2.Amount.ToString(), content);
        Assert.Contains(transfer2.ImportHash, content);
        Assert.Contains(transfer2.RevisionGuid.ToString(), content);
        Assert.Contains(transfer2.PrevRevision.ToString(), content);
    }
}

    