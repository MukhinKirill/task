using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

using Task.Connector.Models;
using Task.Connector.Factory;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger logger { get; set; }

        private IConnector _connector;
        public void StartUp(string connectionString)
        {
            if(logger is null)
            {
                throw new ArgumentNullException("Logger is not executed");
            }

            var config = new ConnectionConfig(connectionString);

            _connector = ConnectorsFactory.GetConnector(config.Provider);

            Logger.Debug($"Used {config.Provider} provider");

            _connector.StartUp(connectionString);
        }

        //TODO:Do method calls depending on the provider type 

        public void CreateUser(UserToCreate user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }

        public bool IsUserExists(string userLogin)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            throw new NotImplementedException();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }

        public ILogger Logger { get; set; }
    }
}