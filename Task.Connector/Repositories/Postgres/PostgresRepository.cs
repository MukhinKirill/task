using Microsoft.EntityFrameworkCore;
using Task.Connector.Context.Postgres;

namespace Task.Connector.Repositories.Postgres
{
    public class PostgresRepository : BaseRepository
    {
        private readonly string connectionString;
        public PostgresRepository(string _connectionString)
        {
            var str = _connectionString.Split("ConnectionString=\'")[1].Split("\'")[0];
            connectionString = str;
        }

        protected override PostgresDbContext ConnectToDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PostgresDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new PostgresDbContext(optionsBuilder.Options);
        }
    }
}

