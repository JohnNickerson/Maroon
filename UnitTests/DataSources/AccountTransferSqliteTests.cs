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

        var createdTransfer = _source.Create(transfer);
        Assert.NotNull(createdTransfer);
        Assert.Equal(transfer.FromAccount, createdTransfer.FromAccount);
        Assert.Equal(transfer.ToAccount, createdTransfer.ToAccount);
        Assert.Equal(transfer.Description, createdTransfer.Description);
        Assert.Equal(transfer.Category, createdTransfer.Category);
        Assert.Equal(transfer.Amount, createdTransfer.Amount);
        Assert.Equal(transfer.ImportHash, createdTransfer.ImportHash);
    }
}