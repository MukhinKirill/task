using Microsoft.EntityFrameworkCore;
using Task.Connector.Context;
using Task.Connector.Context.Mssql;

namespace Task.Connector.Repositories.MSSsql
{
    public class MSSqlRepository : BaseRepository
    {
        private readonly string connectionString;
        public MSSqlRepository(string _connectionString)
        {
            var str = _connectionString.Split("ConnectionString=\'")[1].Split("\'")[0];
            connectionString = str;
        }

        protected override SqlDbContext ConnectToDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MSSqlDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new MSSqlDbContext(optionsBuilder.Options);
        }
    }
}
