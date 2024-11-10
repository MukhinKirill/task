using System.Data;
using System.Text.RegularExpressions;
using Npgsql;
using Task.Connector.Repositories;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; } = null!;

        private string _connectionString = null!;
        private string _schemaName = null!;
        private IDbConnection _dbConnection = null!;

        private IRepositoryRegistry _repositoryRegistry = null!;

        public void StartUp(string connectionString)
        {
            _connectionString = ExtractValue(connectionString, "ConnectionString");
            _schemaName = ExtractValue(connectionString, "SchemaName");
            _dbConnection = new NpgsqlConnection(_connectionString);
            _repositoryRegistry = new RepositoryRegistry(Logger, _dbConnection, _schemaName);
        }

        private static string ExtractValue(string input, string key)
        {
            var pattern = $@"{key}='([^']+)'";
            var match = Regex.Match(input, pattern);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private void EnsureInitialized()
        {
            if (_dbConnection == null)
            {
                throw new InvalidOperationException("Connector is not initialized. Call StartUp with a valid connection string.");
            }
        }

        public void CreateUser(UserToCreate user)
        {
            EnsureInitialized();
            _repositoryRegistry.UserRepository.CreateUser(user);
        }

        public bool IsUserExists(string userLogin)
        {
            EnsureInitialized();
            return _repositoryRegistry.UserRepository.IsUserExists(userLogin);
        }

        public IEnumerable<Property> GetAllProperties()
        {
            EnsureInitialized();
            return _repositoryRegistry.UserRepository.GetAllProperties();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            EnsureInitialized();
            return _repositoryRegistry.UserRepository.GetUserProperties(userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            EnsureInitialized();
            _repositoryRegistry.UserRepository.UpdateUserProperties(properties, userLogin); 
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            EnsureInitialized();
            return _repositoryRegistry.PermissionRepository.GetAllPermissions();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            EnsureInitialized();
            _repositoryRegistry.PermissionRepository.AddUserPermissions(userLogin, rightIds);
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            EnsureInitialized();
            _repositoryRegistry.PermissionRepository.RemoveUserPermissions(userLogin, rightIds);
        }
        
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            EnsureInitialized();
           return _repositoryRegistry.PermissionRepository.GetUserPermissions(userLogin);
        }
    }
}