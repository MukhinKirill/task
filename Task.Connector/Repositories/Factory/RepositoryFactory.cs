using System.Data.Common;
using Task.Connector.Repositories.MSSsql;
using Task.Connector.Repositories.Postgres;

namespace Task.Connector.Repositories.Factory
{
    internal static class RepositoryFactory
    {
        public static IRepository CreateRepositoryFrom(string connectionString)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            if (!builder.ContainsKey("Provider")) throw new Exception("Отсутвует Provider в строке подключения.");
            var provider = builder["Provider"] as string;
            return provider.Contains("Postgre") ? new PostgresRepository(connectionString) : new MSSqlRepository(connectionString);
        }
    }
}
