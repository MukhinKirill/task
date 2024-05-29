namespace Task.Connector.Extensions
{
    public static class ConnectionSetupExtensions
    {
        private static Dictionary<string, string> GetAttributes(this string connectionString)
        {
            var keyValues = connectionString.Split("';", StringSplitOptions.RemoveEmptyEntries);
            return keyValues.Select(pair => pair.Split("='", StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(keyValuePair => keyValuePair[0], keyValuePair => keyValuePair[1]);
        }

        public static string GetDbConnectionString(this string connectionString)
        {
            return connectionString.GetAttributes()["ConnectionString"];
        }

        public static string GetDbProvider(this string connectionString)
        {
            var provider = connectionString.GetAttributes()["Provider"];
            if (provider.Contains("SqlServer")) return "MSSQL";
            else if (provider.Contains("PostgreSQL")) return "POSTGRE";
            else return "UnknownProvider";
        }
    }
}