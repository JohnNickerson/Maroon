using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.DataSources.SQLite;

public class TimeLogSqliteSource : IDataSource<TimeLogEntry>
{
    private readonly ISqlConnWrapper _connectionFactory;

    public TimeLogSqliteSource(ISqlConnWrapper connectionFactory)
    {
        _connectionFactory = connectionFactory;
        EnsureTableCreated();
    }

    private void EnsureTableCreated()
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS TimeLogEntries (
                    ID TEXT,
                    Client TEXT,
                    Project TEXT,
                    Note TEXT,
                    Billable INTEGER,
                    StartTime TEXT,
                    EndTime TEXT,
                    IsDeleted INTEGER,
                    LastModified TEXT,
                    RevisionGuid TEXT PRIMARY KEY,
                    PrevRevision TEXT,
                    MergeRevision TEXT,
                    ImportHash TEXT
                );";
        cmd.ExecuteNonQuery();
    }

    public Task SaveAsync(TimeLogEntry item)
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        // The only way we update time log entries is by inserting a new revision with a unique RevisionGuid.
        cmd.CommandText = "INSERT INTO TimeLogEntries (ID, Client, Project, Note, Billable, StartTime, EndTime, IsDeleted, LastModified, RevisionGuid, PrevRevision, MergeRevision, ImportHash) VALUES (@id, @client, @project, @note, @billable, @startTime, @endTime, @isDeleted, @lastModified, @revisionGuid, @prevRevision, @mergeRevision, @importHash);";
        cmd.Parameters.AddWithValue("@id", item.ID.ToString());
        cmd.Parameters.AddWithValue("@client", item.Client ?? "");
        cmd.Parameters.AddWithValue("@project", item.Project ?? "");
        cmd.Parameters.AddWithValue("@note", item.Note ?? "");
        cmd.Parameters.AddWithValue("@billable", item.Billable ? 1 : 0);
        cmd.Parameters.AddWithValue("@startTime", item.StartTime.ToString("o"));
        cmd.Parameters.AddWithValue("@endTime", item.EndTime.ToString("o"));
        cmd.Parameters.AddWithValue("@isDeleted", item.IsDeleted ? 1 : 0);
        cmd.Parameters.AddWithValue("@lastModified", item.LastModified.ToString("O"));
        cmd.Parameters.AddWithValue("@revisionGuid", item.RevisionGuid.ToString());
        cmd.Parameters.AddWithValue("@prevRevision", item.PrevRevision?.ToString() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@mergeRevision", item.MergeRevision?.ToString() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importHash", item.ImportHash?.ToString() ?? "");
        cmd.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    public IEnumerable<TimeLogEntry> FindAll()
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, Client, Project, Note, Billable, StartTime, EndTime, IsDeleted, LastModified, RevisionGuid, PrevRevision, MergeRevision, ImportHash FROM TimeLogEntries;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var item = new TimeLogEntry
            {
                ID = Guid.Parse(reader.GetString(0)),
                Client = reader.GetString(1),
                Project = reader.GetString(2),
                Note = reader.GetString(3),
                Billable = reader.GetInt32(4) == 1,
                StartTime = DateTime.Parse(reader.GetString(5)),
                EndTime = DateTime.Parse(reader.GetString(6)),
                IsDeleted = reader.GetInt32(7) == 1,
                LastModified = DateTime.Parse(reader.GetString(8)),
                RevisionGuid = Guid.Parse(reader.GetString(9)),
                PrevRevision = reader.IsDBNull(10) ? null : Guid.Parse(reader.GetString(10)),
                MergeRevision = reader.IsDBNull(11) ? null : Guid.Parse(reader.GetString(11)),
                ImportHash = reader.IsDBNull(12) ? null : reader.GetString(12)
            };
            yield return item;
        }
    }

    public TimeLogEntry? FindRevision(Guid id)
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, Client, Project, Note, Billable, StartTime, EndTime, IsDeleted, LastModified, RevisionGuid, PrevRevision, MergeRevision, ImportHash FROM TimeLogEntries WHERE RevisionGuid = @revisionGuid;";
        cmd.Parameters.AddWithValue("@revisionGuid", id.ToString());
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var item = new TimeLogEntry
            {
                ID = Guid.Parse(reader.GetString(0)),
                Client = reader.GetString(1),
                Project = reader.GetString(2),
                Note = reader.GetString(3),
                Billable = reader.GetInt32(4) == 1,
                StartTime = DateTime.Parse(reader.GetString(5)),
                EndTime = DateTime.Parse(reader.GetString(6)),
                IsDeleted = reader.GetInt32(7) == 1,
                LastModified = DateTime.Parse(reader.GetString(8)),
                RevisionGuid = Guid.Parse(reader.GetString(9)),
                PrevRevision = reader.IsDBNull(10) ? null : Guid.Parse(reader.GetString(10)),
                MergeRevision = reader.IsDBNull(11) ? null : Guid.Parse(reader.GetString(11)),
                ImportHash = reader.IsDBNull(12) ? null : reader.GetString(12)
            };
            return item;
        }
        return null;
    }

    public TimeLogEntry Insert(TimeLogEntry item)
    {
        item.LastModified = DateTime.Now;
        SaveAsync(item).Wait();
        return item;
    }

    public void Purge(params Guid[] ids)
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        var revisionGuids = "[" + string.Join(",", ids.Select(id => $"'{id}'")) + "]";
        cmd.CommandText = $"DELETE FROM TimeLogEntries WHERE RevisionGuid IN (SELECT value FROM json_each(@revisionGuids));";
        cmd.Parameters.AddWithValue("@revisionGuids", revisionGuids);
        cmd.ExecuteNonQuery();
    }

    public DateTime GetLastWriteTime()
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(LastModified) FROM TimeLogEntries;";
        var result = cmd.ExecuteScalar();
        if (result != null && result != DBNull.Value)
        {
            return DateTime.Parse((string)result);
        }
        return DateTime.MinValue;
    }
}