using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Task.Connector.Infrastructure;
using Task.Connector.Models;
using Task.Connector.Validation;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector
{
    internal static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services, string connectionString)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddDbContext<TaskDbContext>(_ => _.UseNpgsql(connectionString));
            services.AddScoped<IValidator<User>,UserValidator>();
            services.AddScoped<IValidator<Password>,PasswordValidator>();

            return services;
        }
    }
}
