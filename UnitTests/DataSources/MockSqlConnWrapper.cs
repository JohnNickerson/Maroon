using AssimilationSoftware.Maroon.Interfaces;
using Microsoft.Data.Sqlite;

namespace AssimilationSoftware.Maroon.UnitTests.DataSources;

public class MockSqlConnWrapper : ISqlConnWrapper
{
    private const string ConnectionString = "Data Source=TestDb;Mode=Memory;Cache=Shared";
    private SqliteConnection _connection;
    private SqliteConnection _keepAliveConnection;

    public MockSqlConnWrapper()
    {
        _keepAliveConnection = new SqliteConnection(ConnectionString);
        _keepAliveConnection.Open();
        _connection = new SqliteConnection(ConnectionString);
    }

    public SqliteConnection GetConnection()
    {
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            _connection.Open();
        }
        return _connection;
    }
}