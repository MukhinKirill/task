using System.Text.RegularExpressions;
using Task.Integration.Data.DbCommon;

namespace Task.Connector.Helpers;

public static partial class DbConnectionHelper
{
    public static DataContext GetContext(string connectorCs)
    {
        var connectionInfo = GetConnectionInfo(connectorCs);
        var factory = new DbContextFactory(connectionInfo.ConnectionString);
        return factory.GetContext(connectionInfo.ProviderName);
    }

    private static ConnectionInfo GetConnectionInfo (string connectorCs)
    {
        var matches = MyRegex().Matches(connectorCs);

        var connectionInfo = new Dictionary<string, string>();

        foreach (Match match in matches)
        {
            connectionInfo[match.Groups["key"].Value] = match.Groups["value"].Value;
        }

        if (connectionInfo["Provider"].Contains("PostgreSQL"))
            connectionInfo["Provider"] = "POSTGRE";

        return new ConnectionInfo(
            connectionInfo["ConnectionString"], 
            connectionInfo["Provider"], 
            connectionInfo["SchemaName"]
        );
    }
    
    [GeneratedRegex("(?<key>\\w+)='(?<value>[^']+)';")]
    private static partial Regex MyRegex();
    
    private sealed record ConnectionInfo(string ConnectionString, string ProviderName, string SchemaName);
}