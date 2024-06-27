using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Task.Connector.Mappers;
using Task.Connector.Parsers.Records;
using Task.Connector.Repositories;

namespace Task.Connector
{
    public static class ServiceProviderSingleton
    {
        private static readonly ServiceCollection _services = new ServiceCollection();

        public static IServiceProvider? ServiceProvider { get; private set; }

        public static void RegisterServices(ConnectionConfiguration connectionConfiguration)
        {
            if (ServiceProvider == null) 
            {
                var options = ConnectionBuilder.GetConnection(connectionConfiguration);

                using (var context = new TaskDbContext(options))
                {
                    if(context.Database.CanConnect() == false)
                    {
                        throw new Exception("Cannot connect to db");
                    }
                }

                _services.AddSingleton(new TaskDbContext(options));
                _services.AddSingleton<UserMapper>();
                _services.AddSingleton<IUserRepository, UserRepository>();
                _services.AddSingleton<IPermissionRepository, PermissionRepository>();

                ServiceProvider = _services.BuildServiceProvider();
            }
        }
    }
}
