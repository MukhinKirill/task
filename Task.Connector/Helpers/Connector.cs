namespace Task.Connector.Helpers
{
    /// <summary>
    /// Helper class for managing database connection strings and providers.
    /// </summary>
    public static class Connector
    {
        /// <summary>
        /// Exception encountered during operations in the helper class.
        /// </summary>
        public static Exception _excep { get; private set; }
        
        /// <summary>
        /// Determines the provider based on the provided connection string.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <returns>The database provider name (MSSQL for SQL Server, POSTGRE for PostgreSQL) or null if not recognized.</returns>
        public static string GetProvider(string connectionString)
        {
            var lowerConnectionString = connectionString.ToLowerInvariant();
    
            return lowerConnectionString.Contains("sqlserver") ? "MSSQL" :
                lowerConnectionString.Contains("postgresql") ? "POSTGRE" :
                throw new ArgumentException("Unknown database provider", nameof(connectionString));
        }

        
        /// <summary>
        /// Retrieves the default connection string from the provided connection string.
        /// </summary>
        /// <param name="connectionString">The full connection string.</param>
        /// <returns>The default connection string extracted from the full connection string, or null if not found.</returns>
        public static string DefaultConnectionString(string connectionString)
        {
            const string connectionKey = "ConnectionString='";
            int startIndex = connectionString.IndexOf(connectionKey, StringComparison.Ordinal);
            if (startIndex != -1)
            {
                startIndex += connectionKey.Length;
                int endIndex = connectionString.IndexOf("'", startIndex, StringComparison.Ordinal);
                if (endIndex != -1)
                {
                    return connectionString.Substring(startIndex, endIndex - startIndex);
                }
            }
            return null!;
        }

    }
}
