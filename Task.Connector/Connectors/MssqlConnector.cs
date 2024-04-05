using Dapper;
using System.Data.Common;
using System.Data.SqlClient;
using Task.Connector.Interfaces;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Connectors
{
    internal class MssqlConnector : IConnectorDb
    {
        public ILogger Logger { get; set; }

        private bool _disposed = false;

        private DbConnection _dbConnection;

        public void StartUp(string connectionString)
        {
            var connection =  new ConnectionConfiguration(connectionString);

            _dbConnection = new SqlConnection(connection.ConnectionString);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void CreateUser(UserToCreate user)
        {
            var query = "";
            var parameters = new UserObjectCreateParamaters(user.Login, user.Properties);

            var id = _dbConnection.Query<int>(query, parameters).Single();


        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
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

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _dbConnection.Dispose();

            GC.SuppressFinalize(this);

            _disposed = true;
        }

        ~MssqlConnector()
        {
            Dispose();
        }
    }
}
