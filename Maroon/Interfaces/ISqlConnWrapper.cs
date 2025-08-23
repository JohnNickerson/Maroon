using Microsoft.Data.Sqlite;

namespace AssimilationSoftware.Maroon.Interfaces;

public interface ISqlConnWrapper
{
    public SqliteConnection GetConnection();
}