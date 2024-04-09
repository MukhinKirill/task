using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Models;
using Task.Connector.Factory;
namespace Task.Connector
{

    public class ConnectorDb : IConnector
    {
        private IConnector connector;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            if(Logger is null)
            {
                throw new ArgumentNullException("Logger is not executed");
            }

            var config = new ConnectionConfig(connectionString);

            connector = ConnectorsFactory.GetConnector(config.Provider);

            Logger.Debug($"Used {config.Provider} provider");

            connector.StartUp(connectionString);
        }

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
    }
}