using Microsoft.EntityFrameworkCore;

using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;

namespace Task.Connector.Persistence;
public sealed class DataContextFactory
{
    public const string MSSQL = "MSSQL";
    public const string POSTGRES = "POSTGRE";

    private readonly string _connectionString;
    private readonly ILogger _logger;

    public DataContextFactory(string connectionString, ILogger logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public DataContext GetContext()
    {
        DbContextOptionsBuilder<DataContext> dbContextOptionsBuilder = new();
        try
        {
            dbContextOptionsBuilder.UseNpgsql(_connectionString);
            return new DataContext(dbContextOptionsBuilder.Options);
        }
        catch
        {
            _logger.Error($"Invalid connection string '{_connectionString}'");
            throw;
        }
    }
}
