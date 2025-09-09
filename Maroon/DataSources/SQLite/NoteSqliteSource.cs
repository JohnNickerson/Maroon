using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using AssimilationSoftware.Maroon.Model; // Note is defined here
using AssimilationSoftware.Maroon.Interfaces; // Use existing IDataSource interface

namespace AssimilationSoftware.Maroon.DataSources.SQLite
{
    public class NoteSqliteSource : IDataSource<Note>
    {
        private readonly ISqlConnWrapper _connectionFactory;

        public NoteSqliteSource(ISqlConnWrapper connectionFactory)
        {
            _connectionFactory = connectionFactory;
            EnsureTableCreated();
        }

        private void EnsureTableCreated()
        {
            using var conn = _connectionFactory.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Notes (
                    ID TEXT,
                    Text TEXT,
                    Tags TEXT,
                    Timestamp TEXT,
                    ParentId TEXT,
                    RevisionGuid TEXT PRIMARY KEY,
                    PrevRevision TEXT,
                    MergeRevision TEXT,
                    LastModified TEXT,
                    IsDeleted INTEGER,
                    ImportHash TEXT
                );";
            cmd.ExecuteNonQuery();
        }

        public Task SaveAsync(Note note)
        {
            using var conn = _connectionFactory.GetConnection();
            using var cmd = conn.CreateCommand();
            // The only way we update notes is by inserting a new revision with a unique RevisionGuid.
            cmd.CommandText = "INSERT INTO Notes (ID, Text, Tags, Timestamp, ParentId, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash) VALUES (@id, @text, @tags, @timestamp, @parentId, @revisionGuid, @prevRevision, @mergeRevision, @lastModified, @isDeleted, @importHash);";
            cmd.Parameters.AddWithValue("@id", note.ID.ToString());
            cmd.Parameters.AddWithValue("@text", note.Text ?? "");
            cmd.Parameters.AddWithValue("@tags", string.Join(",", note.Tags ?? new List<string>()));
            cmd.Parameters.AddWithValue("@timestamp", note.Timestamp.ToString("o"));
            cmd.Parameters.AddWithValue("@parentId", note.ParentId?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@revisionGuid", note.RevisionGuid.ToString());
            cmd.Parameters.AddWithValue("@prevRevision", note.PrevRevision?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@mergeRevision", note.MergeRevision?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@lastModified", note.LastModified.ToString("O"));
            cmd.Parameters.AddWithValue("@isDeleted", note.IsDeleted.ToString());
            cmd.Parameters.AddWithValue("@importHash", note.ImportHash?.ToString() ?? "");
            cmd.ExecuteNonQuery();
            return Task.CompletedTask;
        }

        public IEnumerable<Note> FindAll()
        {
            using var conn = _connectionFactory.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ID, Text, Tags, Timestamp, ParentId, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash FROM Notes;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var tagsString = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var tags = tagsString.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList();
                yield return new Note
                {
                    ID = Guid.Parse(reader.GetString(0)),
                    Text = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Tags = tags,
                    Timestamp = reader.IsDBNull(3) ? DateTime.MinValue : DateTime.Parse(reader.GetString(3)),
                    ParentId = reader.IsDBNull(4) ? null : Guid.Parse(reader.GetString(4)),
                    RevisionGuid = Guid.Parse(reader.GetString(5)),
                    PrevRevision = reader.IsDBNull(6) ? null : Guid.Parse(reader.GetString(6)),
                    MergeRevision = reader.IsDBNull(7) ? null : Guid.Parse(reader.GetString(7)),
                    LastModified = DateTime.Parse(reader.GetString(8)),
                    IsDeleted = reader.GetBoolean(9),
                    ImportHash = reader.IsDBNull(10) ? null : reader.GetString(10)
                };
            }
        }

        public Note? FindRevision(Guid id)
        {
            using var conn = _connectionFactory.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ID, Text, Tags, Timestamp, ParentId, RevisionGuid, PrevRevision, MergeRevision, LastModified, IsDeleted, ImportHash FROM Notes Where RevisionGuid = @revisionGuid;";
            cmd.Parameters.AddWithValue("@revisionGuid", id.ToString());
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var tagsString = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var tags = tagsString.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList();
                return new Note
                {
                    ID = Guid.Parse(reader.GetString(0)),
                    Text = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Tags = tags,
                    Timestamp = reader.IsDBNull(3) ? DateTime.MinValue : DateTime.Parse(reader.GetString(3)),
                    ParentId = reader.IsDBNull(4) ? null : Guid.Parse(reader.GetString(4)),
                    RevisionGuid = Guid.Parse(reader.GetString(5)),
                    PrevRevision = reader.IsDBNull(6) ? null : Guid.Parse(reader.GetString(6)),
                    MergeRevision = reader.IsDBNull(7) ? null : Guid.Parse(reader.GetString(7)),
                    LastModified = DateTime.Parse(reader.GetString(8)),
                    IsDeleted = reader.GetBoolean(9),
                    ImportHash = reader.IsDBNull(10) ? null : reader.GetString(10)
                };
            }
            return null;
        }

        public Note Insert(Note item)
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
            cmd.CommandText = @"DELETE FROM Notes WHERE RevisionGuid IN (SELECT value FROM json_each(@revisionGuids));";
            cmd.Parameters.AddWithValue("@revisionGuids", revisionGuids);
            cmd.ExecuteNonQuery();
        }

        public DateTime GetLastWriteTime()
        {
            using var conn = _connectionFactory.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT MAX(LastModified) FROM Notes;";
            var result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                return DateTime.Parse((string)result);
            }
            return DateTime.MinValue; // or throw an exception if no notes exist
        }
    }
}
