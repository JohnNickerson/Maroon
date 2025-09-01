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
            // The only way we update notes is by inserting a new revision with a unique RevisionGuid.
            cmd.CommandText = "INSERT INTO Notes (ID, Text, Tags, Timestamp, ParentId, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash) VALUES (@id, @text, @tags, @timestamp, @parentId, @revisionGuid, @prevRevision, @mergeRevision, @lastModified, @isDeleted, @importHash);";
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@text", item.Text ?? "");
            cmd.Parameters.AddWithValue("@tags", string.Join(",", item.Tags ?? new List<string>()));
            cmd.Parameters.AddWithValue("@timestamp", item.Timestamp.ToString("o"));
            cmd.Parameters.AddWithValue("@parentId", item.ParentId?.ToString() ?? (object)DBNull.Value);
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
        throw new NotImplementedException();
    }

    public AccountTransfer Delete(AccountTransfer item)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<AccountTransfer> FindAll()
    {
        throw new NotImplementedException();
    }

    public AccountTransfer? FindRevision(Guid id)
    {
        throw new NotImplementedException();
    }

    public DateTime GetLastWriteTime()
    {
        throw new NotImplementedException();
    }

    public void Purge(params Guid[] ids)
    {
        throw new NotImplementedException();
    }

    public AccountTransfer Update(AccountTransfer item)
    {
        throw new NotImplementedException();
    }
}