using System.Text;
using Task.Connector.Config.Parse;
using Task.Connector.Storage;

namespace Task.Connector;

public partial class ConnectorDb
{
    private static string BuildPostgresConnectionString(ConnectionScheme scheme)
    {
        // build npgsql connection string
        var sb = new StringBuilder();
        void AppendParameter(string name, string value)
        {
            sb.Append(name).Append('=').Append(value).Append(';');
        }

        void AppendMatchedParameter(string name, string newName)
        {
            if (scheme.TryGetParameter(name, out var value))
            {
                AppendParameter(newName, value);
            }
        }
                
        AppendParameter("Server", scheme.Host);
        // append default port if not defined in scheme
        AppendParameter("Port", scheme.Port > 0 ? scheme.Port.ToString() : "5432");
        
        AppendParameter("Database", scheme.Database);
        AppendParameter("User Id", scheme.Username);
        AppendParameter("Password", scheme.Password);

        // append common Npgsql specific parameters
        // https://www.npgsql.org/doc/connection-string-parameters.html
        AppendMatchedParameter("schema", "Search Path");
        AppendMatchedParameter("security", "Integrated Security");
        AppendMatchedParameter("timeout", "Timeout");
        AppendMatchedParameter("command_timeout", "Command Timeout");
        AppendMatchedParameter("protocol", "Protocol");
        AppendMatchedParameter("ssl", "SSL");
        AppendMatchedParameter("ssl_mode", "SslMode");
        AppendMatchedParameter("pool", "Pooling");
        AppendMatchedParameter("pool_min", "MinPoolSize");
        AppendMatchedParameter("pool_max", "MaxPoolSize");
        AppendMatchedParameter("conn_lifetime", "ConnectionLifetime");

        return sb.ToString();
    }
    
    /// <summary>
    /// Converts given <c>ConnectionScheme</c> to <c>PostgreSQL</c> connection string and creates <c>IUserRepository</c>
    /// bound to <c>PostgreSQL</c> cluster.
    /// </summary>
    /// <param name="scheme">Connetion scheme</param>
    /// <exception cref="ConnectorConnectException">Thrown on connection failure</exception>
    private IUserRepository CreatePostgresUserRepository(ConnectionScheme scheme)
    {
        IUserRepository repo;
        try
        {
            var connStr = BuildPostgresConnectionString(scheme);
            repo = new NpgsqlUserRepository(connStr);
        }
        catch (Exception e)
        {
            throw new ConnectorConnectException("Can't resolve connection string: {0}", e.Message);
        }

        return repo;
    }

    /// <summary>
    /// Converts given <c>ConnectionScheme</c> to <c>PostgreSQL</c> connection string and creates <c>IPermissionRepository</c>
    /// bound to <c>PostgreSQL</c> cluster.
    /// </summary>
    /// <param name="scheme">Connetion scheme</param>
    /// <exception cref="ConnectorConnectException">Thrown on connection failure</exception>
    private IPermissionRepository CreatePostgresPermissionRepository(ConnectionScheme scheme)
    {
        IPermissionRepository repo;
        try
        {
            var connStr = BuildPostgresConnectionString(scheme);
            repo = new NpgsqlPermissionRepository(connStr);
        }
        catch (Exception e)
        {
            throw new ConnectorConnectException("Can't resolve connection string: {0}", e.Message);
        }

        return repo;
    }
}