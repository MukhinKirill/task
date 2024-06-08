using Microsoft.Extensions.Logging;
using Task.Connector.Services;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using ILogger = Task.Integration.Data.Models.ILogger;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private static ConnectorService _connectorService = null!;

        public void StartUp(string connectionString)
            => _connectorService = new(connectionString, Logger);
        
        public void CreateUser(UserToCreate user)
            => _connectorService.AddUser(user);

        public IEnumerable<Property> GetAllProperties()
            => _connectorService.GetProperties();

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
            => _connectorService.GetUserProperties(userLogin);

        public bool IsUserExists(string userLogin)
            => _connectorService.IsUserExists(userLogin);

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
            => _connectorService.UpdateUserProperties(properties, userLogin);

        public IEnumerable<Permission> GetAllPermissions()
            => _connectorService.GetPermissions();

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
            => _connectorService.AddUserPermissions(userLogin, rightIds);

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
            => _connectorService.RemoveUserPermissions(userLogin, rightIds);

        public IEnumerable<string> GetUserPermissions(string userLogin)
            => _connectorService.GetUserPermissions(userLogin);

        /// <inheritdoc />
        public ILogger Logger { get; set; }
    }
}