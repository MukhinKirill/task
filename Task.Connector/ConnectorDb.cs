using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Npgsql;
using Task.Connector.Commands;
using System.Data.Common;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private NpgsqlConnection _dbConnection;

        public ConnectorDb() { }

        public void StartUp(string connectionString)
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };
            _dbConnection = new NpgsqlConnection((string)builder["ConnectionString"]);
            _dbConnection.Open();
        }

        public void CreateUser(UserToCreate user)
        {
            var command = new CreateUserCommand(_dbConnection, Logger);
            command.Execute(user);
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var query = new GetAllPropertiesQuery(_dbConnection, Logger);
            return query.Execute();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var query = new GetUserPropertiesQuery(_dbConnection, Logger);
            return query.Execute(userLogin);
        }

        public bool IsUserExists(string userLogin)
        {
            var query = new IsUserExistsQuery(_dbConnection, Logger);
            return query.Execute(userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var command = new UpdateUserPropertiesCommand(_dbConnection, Logger);
            command.Execute(properties, userLogin);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var query = new GetAllPermissionsQuery(_dbConnection, Logger);
            return query.Execute();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var command = new AddUserPermissionsCommand(_dbConnection, Logger);
            command.Execute(userLogin, rightIds);
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var command = new RemoveUserPermissionsCommand(_dbConnection, Logger);
            command.Execute(userLogin, rightIds);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var query = new GetUserPermissionsQuery(_dbConnection, Logger);
            return query.Execute(userLogin);
        }

        public ILogger Logger { get; set; }
    }
}