using Ardalis.GuardClauses;
using Microsoft.Data.SqlClient;
using System.Data;
using Task.Connector.Interfaces;
using Task.Integration.Data.Models;

namespace Task.Connector.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger _logger;

    public DatabaseService(string connectionString, ILogger logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IDbConnection GetOpenConnection()
    {
        Guard.Against.Null(_connectionString, nameof(_connectionString));

        var sqlConnection = new SqlConnection(_connectionString);
        sqlConnection.Open();

        _logger.Debug("Database connection opened.");

        return sqlConnection;
    }

    public void ExecuteInTransaction(Action<IDbConnection, IDbTransaction> action)
    {
        using var connection = GetOpenConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            action(connection, transaction);
            transaction.Commit();

            _logger.Debug("Transaction committed successfully.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.Warn("Transaction rolled back due to error: " + ex.Message);
            throw;
        }
    }

    public T ExecuteInTransaction<T>(Func<IDbConnection, IDbTransaction, T> func)
    {
        using var connection = GetOpenConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            var result = func(connection, transaction);
            transaction.Commit();

            _logger.Debug("Transaction committed successfully.");
            return result;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.Warn("Transaction rolled back due to error: " + ex.Message);
            throw;
        }
    }
}

