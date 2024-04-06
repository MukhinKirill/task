namespace Task.Connector.Tests.Constants
{
    internal static class DatabaseConnectors
    {
        const string CONNECTION_TEMPLATE = "@connection";

        const string MSSQL_CONFIGURATION = $"ConnectionString='{CONNECTION_TEMPLATE}';Provider='SqlServer.2019';SchemaName='AvanpostIntegrationTestTaskSchema';";

        const string POSTGRE_CONFIGURATION = $"ConnectionString='{CONNECTION_TEMPLATE}';Provider='PostgreSQL.9.5';SchemaName='AvanpostIntegrationTestTaskSchema';";

        public const string MSSQL_PROVIDER = "MSSQL";

        public const string POSGRE_PROVIDER = "POSTGRE";

        public static string GetMssqlConfiguration(string connectionString)
        {
            return MSSQL_CONFIGURATION.Replace(CONNECTION_TEMPLATE, connectionString);
        }

        public static string GetPostgresConfiguration(string connectionString)
        {
            return POSTGRE_CONFIGURATION.Replace(CONNECTION_TEMPLATE, connectionString);
        }
    }
}
