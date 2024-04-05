using Task.Connector.Factories;
using Task.Connector.Interfaces;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Connectors
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }

        private IConnectorDb _connector;

        public void StartUp(string connectionString)
        {
            var configuration = new ConnectionConfiguration(connectionString);
            _connector = ConnectorFactory.GetConnector(configuration.Provider);

            _connector.StartUp(connectionString);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            _connector.AddUserPermissions(userLogin, rightIds);
        }

        public void CreateUser(UserToCreate user)
        {
            _connector.CreateUser(user);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return _connector.GetAllPermissions();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return _connector.GetAllProperties();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            return _connector.GetUserPermissions(userLogin);
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            return _connector.GetUserProperties(userLogin);
        }

        public bool IsUserExists(string userLogin)
        {
            return _connector.IsUserExists(userLogin);    
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            _connector.RemoveUserPermissions(userLogin, rightIds);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            _connector.UpdateUserProperties(properties, userLogin);
        }

        ~ConnectorDb()
        {
            _connector.Dispose();
        }
    }
}
