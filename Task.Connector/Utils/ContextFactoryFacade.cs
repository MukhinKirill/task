using System.Data.Common;
using Task.Integration.Data.DbCommon;
namespace Task.Connector.Factories;

public class ContextFactoryFacade
{
    public DataContext GetContext(string connectionString)
    {
        if (connectionString == string.Empty)
            throw new ArgumentException("Connection string is empty!");

        var stringBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        if (!stringBuilder.TryGetValue("ConnectionString", out var dbConnectionStr))
            throw new ArgumentException("Connection string does not contain database connection string!");

        if (!stringBuilder.TryGetValue("Provider", out var providerName))
            throw new ArgumentException("Connection string has not provider!");

        providerName = NormalizeProviderName(providerName);

        if (providerName == null)
            throw new ArgumentException("Unknown provider!");

        var contextFactory = new DbContextFactory((string)dbConnectionStr);

        return contextFactory.GetContext((string)providerName);
    }

    private static string? NormalizeProviderName(object? providerName)
    {
        if (providerName == null)
            return null;

        if (((string)providerName).Contains("Postgre", StringComparison.OrdinalIgnoreCase))
        {
            return "POSTGRE";
        }
        else if (((string)providerName).Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return "MSSQL";
        }

        return null;
    }
}
