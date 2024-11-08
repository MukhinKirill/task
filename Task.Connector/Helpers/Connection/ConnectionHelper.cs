
using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.DbCommon;

namespace Task.Connector.Helpers.Connection
{
    internal static class ConnectionHelper
    {
        internal static DataContext GetContext(string connectorString)
        {
            var info = GetConnectionInfo(connectorString);
            var factory = new DbContextFactory(info.connectionString);
            return factory.GetContext(info.providerName);
        }

        private static (string connectionString, string providerName) GetConnectionInfo(string connectorCS)
        {
            string connectionString = ParseConnectorString(connectorCS, "ConnectionString");
            string providerName = "UNKNOWN";
            string providerParsed = ParseConnectorString(connectorCS, "Provider");
            if (providerParsed.Contains("SqlServer"))
                providerName = "MSSQL";
            if (providerParsed.Contains("PostgreSQL"))
                providerName = "POSTGRE";
            return (connectionString, providerName);
        }
        private static string ParseConnectorString(string initialString, string propertyName)
        {
            propertyName += "='";
            int connectionStringStartIndex = initialString.IndexOf(propertyName) + propertyName.Length;
            int connectionStringEndIndex = initialString.IndexOf("'", connectionStringStartIndex);
            return initialString.Substring(connectionStringStartIndex, connectionStringEndIndex - connectionStringStartIndex);
        }
    }
}
