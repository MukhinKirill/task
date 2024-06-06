using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Task.Connector.Context.Postgres;

namespace Task.Connector.Repositories.Postgres
{
    internal class PostgresRepository : BaseRepository
    {
        private readonly string connectionString;
        public PostgresRepository(string connectionString)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            if (!builder.ContainsKey("Provider")) throw new Exception("Отсутвует ConnectionString в строке подключения.");
            this.connectionString = (string)builder["ConnectionString"];

        }

        protected override PostgresDbContext ConnectToDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PostgresDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new PostgresDbContext(optionsBuilder.Options);
        }
    }
}

