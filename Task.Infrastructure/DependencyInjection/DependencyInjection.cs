using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task.Connector.Connector;
using Task.Connector.Logger;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;

namespace Task.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static void AddTaskLibrary(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));
        services.AddTransient<ILogger>(provider => new FileLogger("logs.txt", "POSTGRE"));
        services.AddScoped<IConnector>(provider =>
        {
            var connector = new ConnectorDb();
            connector.StartUp(connectionString);
            return connector;
        });
    }
}