using Npgsql;

namespace Task.Connector.Storage;

/// <summary>
/// Base class for all repositories that need to use PostgreSQL
/// </summary>
internal abstract class NpgsqlBaseRepository : IDisposable
{
    private readonly NpgsqlDataSource _ds;
    
    protected NpgsqlBaseRepository(string connInfo)
    {
        _ds = NpgsqlDataSource.Create(connInfo);
    }

    protected NpgsqlDataSource DataSource => _ds;

    public void Dispose()
    {
        _ds.Dispose();
    }
}