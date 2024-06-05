using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task.Common.EntityFrameWork;
using Task.Connector;
using Task.Connector.Logger;
using Task.Infrastructure.EntityFrameWork;
using Task.Integration.Data.Models;

namespace Task.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static void AddTaskLibrary(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddTransient<ILogger>(provider => new FileLogger("logs.txt", "POSTGRE"));

        AddRepositories(services);
        AddServices(services);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        var assembly = Assembly.AssemblyReference.Assembly;

        var types = assembly.GetTypes().Where(p => p.GetInterfaces().Contains(typeof(IRepository<,>))).ToList();

        foreach (var type in types)
        {
            services.AddScoped(type.GetInterface("IRepository"), type);
        }
    }
    
    private static void AddServices(IServiceCollection services)
    {
        var assembly = Connector.Assembly.AssemblyReference.Assembly;

        var types = assembly.GetTypes().Where(p => p.GetInterfaces().Contains(typeof(IApplicationBaseService))).ToList();

        foreach (var type in types)
        {
            services.AddScoped(type.GetInterface("IApplicationBaseService"), type);
        }
    }
}