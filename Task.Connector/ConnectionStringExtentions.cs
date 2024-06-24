using System.Text.RegularExpressions;

namespace Task.Connector;

public static partial class ConnectionStringExtentions
{
    public static string GetInnerConnectionString(this string connectionString)
    {
        return ConnectionStringRegex().Match(connectionString).Groups[1].Value;
    }

    public static string GetProviderName(this string connectionString)
    {
        var providerValue = ProviderRegex().Match(connectionString).Groups[1].Value;

        return providerValue switch
            {
                "SqlServer.2019" => "MSSQL",
                "PostgreSQL.9.5" => "POSTGRE",
                _ => "UnknownProvider"
            };
    }

    [GeneratedRegex("ConnectionString='([^']*)'")]
    private static partial Regex ConnectionStringRegex();

    [GeneratedRegex("Provider='([^']*)'")]
    private static partial Regex ProviderRegex();
}
