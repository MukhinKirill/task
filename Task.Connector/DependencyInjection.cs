using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
