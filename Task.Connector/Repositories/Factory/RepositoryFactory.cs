using Task.Connector.Repositories.MSSsql;
using Task.Connector.Repositories.Postgres;

namespace Task.Connector.Repositories.Factory
{
    public static class RepositoryFactory
    {
        public static IStorage CreateRepositoryFrom(string connectionString)
        {
            var str = connectionString.Split("Provider=\'")[1];
            var str2 = str.Split("\'")[0];
            return str2.Contains("Postgre") ? new PostgresRepository(connectionString) : new MSSqlRepository(connectionString);
        }
    }
}
