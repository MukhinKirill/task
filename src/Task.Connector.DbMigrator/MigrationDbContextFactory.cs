using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Task.Connector.DbMigrator;

public class MigrationDbContextFactory : IDesignTimeDbContextFactory<MigrationDbContext>
{
    public MigrationDbContext CreateDbContext(string[] args)
    {
        var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        var configuration = builder.Build();
        var connectionString = configuration.GetConnectionString("POSTGRE");

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<MigrationDbContext>();
        dbContextOptionsBuilder.UseNpgsql(connectionString);

        return new MigrationDbContext(dbContextOptionsBuilder.Options);
    }
}
