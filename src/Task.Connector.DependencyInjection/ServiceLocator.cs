using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task.Connector.ComponentRegistrar;
using Task.Connector.DataAccess;

namespace Task.Connector.DependencyInjection;

public static class ServiceLocator
{
    private static string _connectionString = null!;
    private static readonly IServiceProvider _serviceProvider;

    static ServiceLocator()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ConnectorDbContext>(options => options.UseNpgsql(_connectionString));
        services.AddApplicationService();

        _serviceProvider = services.BuildServiceProvider();
    }

    public static void Init(string? connectionString)
    {
        _connectionString = connectionString ?? GetConnectionString();
    }

    private static string GetConnectionString()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json");
        
        return builder.Build().GetConnectionString("POSTGRE")!;
    }

    public static T GetService<T>()
    {
        var service = _serviceProvider.GetService<T>();
        return service ?? throw new ArgumentNullException();
    }

    public static T GetService<T>(params object[] parameters)
    {
        var service = (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T), parameters);
        return service ?? throw new ArgumentNullException();
    }
}
