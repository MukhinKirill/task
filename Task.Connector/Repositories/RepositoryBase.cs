using System.Data;
using Dapper;

namespace Task.Connector.Repositories;

public abstract class RepositoryBase
{
    private readonly IDbConnection _dbConnection;

    private readonly string? _schemaName;

    protected RepositoryBase(IDbConnection dbConnection, string? schemaName)
    {
        _dbConnection = dbConnection;
        _schemaName = schemaName;
    }

    protected void EnsureConnectionOpened()
    {
        if (_dbConnection.State == ConnectionState.Open)
        {
            return;
        }

        _dbConnection.Open();

        if (string.IsNullOrEmpty(_schemaName))
        {
            return;
        }

        var sql = $"""
                   ALTER DATABASE "testDb" SET search_path TO "{_schemaName}";
                   """;

        _dbConnection.Execute(sql);
    }
}