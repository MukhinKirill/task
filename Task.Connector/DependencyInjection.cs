using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Task.Connector.Infrastructure;

namespace Task.Connector
{
    internal static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services, string connectionString)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddDbContext<TaskDbContext>(_ => _.UseNpgsql(connectionString));

            return services;
        }
    }
}
