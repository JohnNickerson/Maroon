using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.DataSources.SQLite;

public class ActionItemSqliteSource : IDataSource<ActionItem>
{
    private ISqlConnWrapper _connectionFactory;

    public ActionItemSqliteSource(ISqlConnWrapper connectionFactory)
    {
        _connectionFactory = connectionFactory;
        EnsureTablesCreated();
    }

    private void EnsureTablesCreated()
    {
        using var conn = _connectionFactory.GetConnection();
        using var command = conn.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS ActionItems (
                ID TEXT,
                Title TEXT,
                Context TEXT,
                Notes TEXT,
                DoneDate TEXT,
                TickleDate TEXT,
                ParentId TEXT,
                ProjectId TEXT,
                Upvotes INTEGER,
                RevisionGuid TEXT PRIMARY KEY,
                PrevRevision TEXT,
                MergeRevision TEXT,
                LastModified TEXT,
                IsDeleted INTEGER,
                ImportHash TEXT
            );
            CREATE TABLE IF NOT EXISTS ActionItemTags (
                RevisionGuid TEXT,
                Tag TEXT,
                Value TEXT,
                PRIMARY KEY (RevisionGuid, Tag)
            );";
        command.ExecuteNonQuery();
    }

    public Task SaveAsync(ActionItem item)
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        // The only way we update records is by inserting a new revision with a unique RevisionGuid.
        cmd.CommandText = "INSERT INTO ActionItems (ID, Title, Context, Notes, DoneDate, TickleDate, ParentId, ProjectId, Upvotes, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash) VALUES (@id, @title, @context, @notes, @doneDate, @tickleDate, @parentId, @projectId, @upvotes, @revisionGuid, @prevRevision, @mergeRevision, @lastModified, @isDeleted, @importHash);";
        cmd.Parameters.AddWithValue("@id", item.ID.ToString());
        cmd.Parameters.AddWithValue("@title", item.Title ?? "");
        cmd.Parameters.AddWithValue("@context", item.Context ?? "");
        cmd.Parameters.AddWithValue("@notes", string.Join(Environment.NewLine, item.Notes));
        cmd.Parameters.AddWithValue("@doneDate", item.DoneDate?.ToString("o") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@tickleDate", item.TickleDate?.ToString("o") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@parentId", item.ParentId?.ToString() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@projectId", item.ProjectId?.ToString() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@upvotes", item.Upvotes);
        cmd.Parameters.AddWithValue("@revisionGuid", item.RevisionGuid.ToString());
        cmd.Parameters.AddWithValue("@prevRevision", item.PrevRevision?.ToString() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@mergeRevision", item.MergeRevision?.ToString() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@lastModified", item.LastModified.ToString("O"));
        cmd.Parameters.AddWithValue("@isDeleted", item.IsDeleted.ToString());
        cmd.Parameters.AddWithValue("@importHash", item.ImportHash?.ToString() ?? "");
        cmd.ExecuteNonQuery();
        using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = "INSERT INTO ActionItemTags (RevisionGuid, Tag, Value) VALUES (@revisionGuid, @tag, @value);";
        cmd2.Parameters.AddWithValue("@revisionGuid", item.RevisionGuid.ToString());
        foreach (var tag in item.Tags)
        {
            cmd2.Parameters.AddWithValue("@tag", tag.Key);
            cmd2.Parameters.AddWithValue("@value", tag.Value);
            cmd2.ExecuteNonQuery();
            cmd2.Parameters.RemoveAt("@tag");
            cmd2.Parameters.RemoveAt("@value");
        }
        return Task.CompletedTask;
    }

    public ActionItem Create(ActionItem item)
    {
        item.PrevRevision = null;
        item.RevisionGuid = Guid.NewGuid();
        item.LastModified = DateTime.Now;
        item.MergeRevision = null;
        item.IsDeleted = false;
        SaveAsync(item).Wait();
        return item;
    }

    public ActionItem Delete(ActionItem item)
    {
        var result = (ActionItem)item.Clone();
        result.PrevRevision = item.RevisionGuid;
        result.RevisionGuid = Guid.NewGuid();
        result.LastModified = DateTime.Now;
        result.IsDeleted = true;
        SaveAsync(result).Wait();
        return result;
    }

    public IEnumerable<ActionItem> FindAll()
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "Select ID, Title, Context, Notes, DoneDate, TickleDate, ParentId, ProjectId, Upvotes, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash FROM ActionItems";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var notesString = reader.IsDBNull(3) ? "" : reader.GetString(3);
            var notes = notesString.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var item = new ActionItem
            {
                ID = Guid.Parse(reader.GetString(0)),
                Title = reader.IsDBNull(1) ? null : reader.GetString(1),
                Context = reader.IsDBNull(2) ? null : reader.GetString(2),
                Notes = notes,
                DoneDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                TickleDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
                ParentId = reader.IsDBNull(6) ? null : Guid.Parse(reader.GetString(6)),
                ProjectId = reader.IsDBNull(7) ? null : Guid.Parse(reader.GetString(7)),
                Upvotes = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                RevisionGuid = Guid.Parse(reader.GetString(9)),
                PrevRevision = reader.IsDBNull(10) ? null : Guid.Parse(reader.GetString(10)),
                MergeRevision = reader.IsDBNull(11) ? null : Guid.Parse(reader.GetString(11)),
                LastModified = DateTime.Parse(reader.GetString(12)),
                IsDeleted = reader.GetBoolean(13),
                ImportHash = reader.IsDBNull(14) ? null : reader.GetString(14),
                Tags = new Dictionary<string, string>()
            };
            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "SELECT Tag, Value FROM ActionItemTags WHERE RevisionGuid = @revisionGuid;";
            cmd2.Parameters.AddWithValue("@revisionGuid", item.RevisionGuid.ToString());
            using var reader2 = cmd2.ExecuteReader();
            while (reader2.Read())
            {
                var tag = reader2.GetString(0);
                var value = reader2.IsDBNull(1) ? "" : reader2.GetString(1);
                item.Tags[tag] = value;
            }
            yield return item;
        }
    }

    public ActionItem? FindRevision(Guid id)
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ID, Title, Context, Notes, DoneDate, TickleDate, ParentId, ProjectId, Upvotes, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash FROM ActionItems Where RevisionGuid = @revisionGuid;";
        cmd.Parameters.AddWithValue("@revisionGuid", id.ToString());
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var notesString = reader.IsDBNull(3) ? "" : reader.GetString(3);
            var notes = notesString.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var item = new ActionItem
            {
                ID = Guid.Parse(reader.GetString(0)),
                Title = reader.IsDBNull(1) ? null : reader.GetString(1),
                Context = reader.IsDBNull(2) ? null : reader.GetString(2),
                Notes = notes,
                DoneDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                TickleDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
                ParentId = reader.IsDBNull(6) ? null : Guid.Parse(reader.GetString(6)),
                ProjectId = reader.IsDBNull(7) ? null : Guid.Parse(reader.GetString(7)),
                Upvotes = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                RevisionGuid = Guid.Parse(reader.GetString(9)),
                PrevRevision = reader.IsDBNull(10) ? null : Guid.Parse(reader.GetString(10)),
                MergeRevision = reader.IsDBNull(11) ? null : Guid.Parse(reader.GetString(11)),
                LastModified = DateTime.Parse(reader.GetString(12)),
                IsDeleted = reader.GetBoolean(13),
                ImportHash = reader.IsDBNull(14) ? null : reader.GetString(14),
                Tags = new Dictionary<string, string>()
            };
            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "SELECT Tag, Value FROM ActionItemTags WHERE RevisionGuid = @revisionGuid;";
            cmd2.Parameters.AddWithValue("@revisionGuid", item.RevisionGuid.ToString());
            using var reader2 = cmd2.ExecuteReader();
            while (reader2.Read())
            {
                var tag = reader2.GetString(0);
                var value = reader2.IsDBNull(1) ? "" : reader2.GetString(1);
                item.Tags[tag] = value;
            }
            return item;
        }
        return null;
    }

    public DateTime GetLastWriteTime()
    {
        using var conn = _connectionFactory.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(LastModified) FROM ActionItems;";
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
        cmd.CommandText = $"DELETE FROM ActionItems WHERE RevisionGuid IN (SELECT value FROM json_each(@revisionGuids));";
        cmd.Parameters.AddWithValue("@revisionGuids", revisionGuids);
        cmd.ExecuteNonQuery();
        using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = $"DELETE FROM ActionItemTags WHERE RevisionGuid IN (SELECT value FROM json_each(@revisionGuids));";
        cmd2.Parameters.AddWithValue("@revisionGuids", revisionGuids);
        cmd2.ExecuteNonQuery();
    }

    public ActionItem Update(ActionItem item)
    {
        var result = (ActionItem)item.Clone();
        result.PrevRevision = item.RevisionGuid;
        result.RevisionGuid = Guid.NewGuid();
        result.LastModified = DateTime.Now;
        result.IsDeleted = false;
        SaveAsync(result).Wait();
        return result;
    }
}