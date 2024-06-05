using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Repositories.MSSsql;
using Task.Connector.Repositories.Postgres;

namespace Task.Connector.Repositories
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
