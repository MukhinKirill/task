using Microsoft.EntityFrameworkCore;
using Task.Connector.DataAccess;

namespace Task.Connector.DbMigrator;

public class MigrationDbContext : ConnectorDbContext
{
    public MigrationDbContext(DbContextOptions options) : base(options) { }
}
