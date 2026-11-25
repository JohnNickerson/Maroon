using AssimilationSoftware.Maroon.Interfaces;
using Microsoft.Data.Sqlite;

namespace AssimilationSoftware.Maroon.Helpers;

public class SqliteConnWrapper : ISqlConnWrapper
{
    private string _connectionString;

    public SqliteConnWrapper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }
        return connection;
    }
}