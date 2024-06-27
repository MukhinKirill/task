using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Parsers.Records;

namespace Task.Connector
{
    public static class ConnectionBuilder
    {
        private const string Postgres = "PostgreSQL.9.5";
        private const string MySql = "SqlServer.2019";

        public static DbContextOptions GetConnection(ConnectionConfiguration connectionConfiguration)
        {
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();

            switch (connectionConfiguration.Provider)
            {
                case Postgres:
                    builder.UseNpgsql(connectionConfiguration.ConnectionString);
                    break;
                case MySql:
                    builder.UseSqlServer(connectionConfiguration.ConnectionString);
                    break;
            }

            return builder.Options;
        }
    }
}
