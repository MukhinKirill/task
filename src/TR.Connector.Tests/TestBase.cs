using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.Connectors.Api.Entities.DTO.Requests;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector.Tests
{
    public class TestBase : IAsyncLifetime
    {
        protected IConnector _connector = null!;
        private readonly string connectorString = "url=http://localhost:5000;login=login;password=password";

        public async Task InitializeAsync()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddHttpClient<IApiClient, ApiClient>(client => {
                client.BaseAddress = new Uri("http://localhost:5000");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });


            services.AddSingleton<IAccessTypeParser, AccessTypeParser>();

            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<IUserService, UserService>();

            // Connector facade
            services.AddSingleton<IConnector, Connector>();

            var provider = services.BuildServiceProvider();
            _connector = provider.GetRequiredService<IConnector>();

            // Ждём завершения логина
            await _connector.StartUpAsync(connectorString);
        }

        protected string GetPropertyValue(IEnumerable<UserProperty> properties, string propertyName)
        {
            return properties.FirstOrDefault(p => p.Name == propertyName)?.Value ?? string.Empty;
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
