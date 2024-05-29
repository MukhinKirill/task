using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace Task.Connector.DataBase;

public class DataContext
{
    private readonly string _connectionString;
    private readonly string _provider;

    public DataContext(string connectionString, string provider)
    {
        _connectionString = connectionString;
        _provider = provider;
    }
    
    public IDbConnection CreateConnection()
    {
        switch (_provider)
        {
            case "MSSQL":
                return new SqlConnection(_connectionString);
            default:
                return new NpgsqlConnection(_connectionString);
        }
    }
}