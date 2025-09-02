using System.Runtime.CompilerServices;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.DataSources.SQLite;

public class AccountTransferSqliteSource : IDataSource<AccountTransfer>
{
    private ISqlConnWrapper _connectionFactory;

    public AccountTransferSqliteSource(ISqlConnWrapper connection)
    {
        this._connectionFactory = connection;
        EnsureTableCreated();
    }

    private void EnsureTableCreated()
    {
        using var conn = _connectionFactory.GetConnection();
        using var command = conn.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS AccountTransfers (
                ID TEXT,
                Date TEXT,
                Category TEXT,
                Amount REAL NOT NULL,
                Description TEXT,
                FromAccount TEXT,
                ToAccount TEXT,
                RevisionGuid TEXT PRIMARY KEY,
                PrevRevision TEXT,
                MergeRevision TEXT,
                LastModified TEXT,
                IsDeleted INTEGER,
                ImportHash TEXT
            )";
        command.ExecuteNonQuery();
    }

        public Task SaveAsync(AccountTransfer item)
        {
            using var conn = _connectionFactory.GetConnection();
            using var cmd = conn.CreateCommand();
            // The only way we update records is by inserting a new revision with a unique RevisionGuid.
            cmd.CommandText = "INSERT INTO AccountTransfers (ID, Date, Category, Amount, Description, FromAccount, ToAccount, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash) VALUES (@id, @date, @category, @amount, @description, @fromAccount, @toAccount, @revisionGuid, @prevRevision, @mergeRevision, @lastModified, @isDeleted, @importHash);";
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@date", item.Date.ToString("o"));
            cmd.Parameters.AddWithValue("@category", item.Category ?? "");
            cmd.Parameters.AddWithValue("@amount", item.Amount);
            cmd.Parameters.AddWithValue("@description", item.Description ?? "");
            cmd.Parameters.AddWithValue("@fromAccount", item.FromAccount ?? "");
            cmd.Parameters.AddWithValue("@toAccount", item.ToAccount ?? "");
            cmd.Parameters.AddWithValue("@revisionGuid", item.RevisionGuid.ToString());
            cmd.Parameters.AddWithValue("@prevRevision", item.PrevRevision?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@mergeRevision", item.MergeRevision?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@lastModified", item.LastModified.ToString("O"));
            cmd.Parameters.AddWithValue("@isDeleted", item.IsDeleted.ToString());
            cmd.Parameters.AddWithValue("@importHash", item.ImportHash?.ToString() ?? "");
            cmd.ExecuteNonQuery();
            return Task.CompletedTask;
        }

    public AccountTransfer Create(AccountTransfer item)
    {
        item.PrevRevision = null;
        item.RevisionGuid = Guid.NewGuid();
        item.LastModified = DateTime.Now;
        item.MergeRevision = null;
        item.IsDeleted = false;
        SaveAsync(item).Wait();
        return item;
    }

    public AccountTransfer Delete(AccountTransfer item)
    {
        var result = (AccountTransfer)item.Clone();
        result.PrevRevision = item.RevisionGuid;
        result.RevisionGuid = Guid.NewGuid();
        result.LastModified = DateTime.Now;
        result.MergeRevision = null;
        result.IsDeleted = true;
        SaveAsync(result).Wait();
        return result;
    }

    public IEnumerable<AccountTransfer> FindAll()
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, Date, Category, Amount, Description, FromAccount, ToAccount, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash FROM AccountTransfers;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            yield return new AccountTransfer
            {
                ID = Guid.Parse(reader.GetString(0)),
                Date = DateTime.Parse(reader.GetString(1)),
                Category = reader.IsDBNull(2) ? null : reader.GetString(2),
                Amount = reader.GetDecimal(3),
                Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                FromAccount = reader.IsDBNull(5) ? null : reader.GetString(5),
                ToAccount = reader.IsDBNull(6) ? null : reader.GetString(6),
                RevisionGuid = Guid.Parse(reader.GetString(7)),
                PrevRevision = reader.IsDBNull(8) ? null : Guid.Parse(reader.GetString(8)),
                MergeRevision = reader.IsDBNull(9) ? null : Guid.Parse(reader.GetString(9)),
                LastModified = reader.IsDBNull(10) ? DateTime.MinValue : DateTime.Parse(reader.GetString(10)),
                IsDeleted = !reader.IsDBNull(11) && reader.GetInt32(11) != 0,
                ImportHash = reader.IsDBNull(12) ? null : reader.GetString(12)
            };
        }
    }

    public AccountTransfer? FindRevision(Guid id)
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, Date, Category, Amount, Description, FromAccount, ToAccount, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash FROM AccountTransfers Where RevisionGuid = @revisionGuid;";
        cmd.Parameters.AddWithValue("@revisionGuid", id.ToString());
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new AccountTransfer
            {
                ID = Guid.Parse(reader.GetString(0)),
                Date = DateTime.Parse(reader.GetString(1)),
                Category = reader.IsDBNull(2) ? null : reader.GetString(2),
                Amount = reader.GetDecimal(3),
                Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                FromAccount = reader.IsDBNull(5) ? null : reader.GetString(5),
                ToAccount = reader.IsDBNull(6) ? null : reader.GetString(6),
                RevisionGuid = Guid.Parse(reader.GetString(7)),
                PrevRevision = reader.IsDBNull(8) ? null : Guid.Parse(reader.GetString(8)),
                MergeRevision = reader.IsDBNull(9) ? null : Guid.Parse(reader.GetString(9)),
                LastModified = reader.IsDBNull(10) ? DateTime.MinValue : DateTime.Parse(reader.GetString(10)),
                IsDeleted = !reader.IsDBNull(11) && reader.GetInt32(11) != 0,
                ImportHash = reader.IsDBNull(12) ? null : reader.GetString(12)
            };
        }
        return null;
    }

    public DateTime GetLastWriteTime()
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(LastModified) FROM AccountTransfers;";
        var result = cmd.ExecuteScalar();
        if (result != null && result != DBNull.Value)
        {
            return DateTime.Parse((string)result);
        }
        return DateTime.MinValue;
    }

    public void Purge(params Guid[] ids)
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        var revisionGuids = "[" + string.Join(",", ids.Select(id => $"'{id}'")) + "]";
        cmd.CommandText = $"DELETE FROM AccountTransfers WHERE RevisionGuid IN (SELECT value FROM json_each(@revisionGuids));";
        cmd.Parameters.AddWithValue("@revisionGuids", revisionGuids);
        cmd.ExecuteNonQuery();
    }

    public AccountTransfer Update(AccountTransfer item)
    {
        var result = (AccountTransfer)item.Clone();
        result.PrevRevision = item.RevisionGuid;
        result.RevisionGuid = Guid.NewGuid();
        result.LastModified = DateTime.Now;
        result.IsDeleted = false;
        SaveAsync(result).Wait();
        return result;
    }
}