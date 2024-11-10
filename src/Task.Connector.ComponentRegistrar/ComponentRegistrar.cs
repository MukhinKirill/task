using Microsoft.Extensions.DependencyInjection;
using Task.Connector.Infrastructure.Repository;
using Task.Connector.Infrastructure.Services.Logger;

namespace Task.Connector.ComponentRegistrar;

public static class ComponentRegistrar
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services)
    {
        services.AddScoped<ILogger, FileLogger>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        return services;
    }
}
