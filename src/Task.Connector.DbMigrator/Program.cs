using Microsoft.EntityFrameworkCore;

namespace Task.Connector.DbMigrator;

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder().ConfigureServices((hostContext, services) => services.AddServices(hostContext.Configuration)).Build();
        Migrate(host.Services);
        host.Run();
    }

    private static void Migrate(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetService<MigrationDbContext>()!;
        context.Database.Migrate();
    }
}