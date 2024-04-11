using Task.Connector.Configuration;

namespace Task.Connector.Helpers;
public static class ConnectionStringHelper
{
    public static ConnectionStringProvider GetProvider(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return ConnectionStringProvider.Unrecognized;

        if (connectionString.Contains("SqlServer", StringComparison.InvariantCultureIgnoreCase))
            return ConnectionStringProvider.MSSQL;

        else if (connectionString.Contains("PostgreSql", StringComparison.InvariantCultureIgnoreCase))
            return ConnectionStringProvider.POSTGRE;

        else
            return ConnectionStringProvider.Unrecognized;
    }

    public static string GetOriginal(string connectionString)
    {
        var skip = new string(connectionString.SkipWhile(c => c != '\'').ToArray()[1..]);
        var original = skip.TakeWhile(c => c != '\'');
        return new string(original.ToArray());
    }
}
