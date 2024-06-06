using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Task.Connector.Context;
using Task.Connector.Context.Mssql;

namespace Task.Connector.Repositories.MSSsql
{
    internal class MSSqlRepository : BaseRepository
    {
        private readonly string connectionString;
        public MSSqlRepository(string connectionString)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            if (!builder.ContainsKey("Provider")) throw new Exception("Отсутвует ConnectionString в строке подключения.");
            this.connectionString = (string)builder["ConnectionString"];
        }

        protected override SqlDbContext ConnectToDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MSSqlDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new MSSqlDbContext(optionsBuilder.Options);
        }
    }
}
