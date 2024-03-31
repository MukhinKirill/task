using Npgsql;
using Dapper;
using System.Data;
using System.Linq;
using Task.Connector.Constants;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using static Dapper.SqlMapper;
using Task.Connector.Helpers;
using Task.Connector.Abstractions;
using Task.Connector.DataBaseContext;
using Task.Connector.Repositories;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private string _connectionString;
        private IAvanpostRepository _repository;
        public ConnectorDb() 
        {
            _connectionString = DataBaseConnectionStrings.DefaultConnectionString;
            _repository = new PostgreRepository();
        }
        private void SetContext()
        {
            switch (ConnectionStringParser.GetProvider(_connectionString))
            {
                case Enums.DataBaseProvider.POSTGRE:
                    _repository = new PostgreRepository();
                    break;
                case Enums.DataBaseProvider.MSSQL:
                    //_context = new MSSQLDBContext();
                    break;
                default:
                    _repository = new PostgreRepository();
                    break;
            }
        }
        public void StartUp(string connectionString)
        {
            _connectionString = connectionString;
            SetContext();
            _repository.StartUp(connectionString);
        }
        
        public void CreateUser(UserToCreate user)
        {
            if(!_repository.CreateUser(user))
            {
                Logger?.Error("Unable to create user");
            }
            Logger?.Debug("the user has been successfully added");
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var properties = _repository.GetAllProperties();
            if (properties is null)
            {
                Logger?.Error("Unable to get properties");
                return Enumerable.Empty<Property>();
            }
            Logger?.Debug($"{nameof(GetAllProperties)}: success");
            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var properties = _repository.GetUserProperties(userLogin);
            if (properties is null)
            {
                Logger?.Error("Unable to get properties");
                return Enumerable.Empty<UserProperty>();
            }
            Logger?.Debug($"{nameof(GetUserProperties)}: success");
            return properties;
        }

        public bool IsUserExists(string userLogin)
        {
            return _repository.IsUserExists(userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!_repository.UpdateUserProperties(properties, userLogin))
            {
                Logger?.Error("Unable to update user Properties");
            }
            Logger?.Debug("the user properties has been successfully updated");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var permissions = _repository.GetAllPermissions();
            if (permissions is null)
            {
                Logger?.Error("Unable to get permissions");
                return Enumerable.Empty<Permission>();
            }
            Logger?.Debug($"{nameof(GetAllPermissions)}: success");
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!_repository.AddUserPermissions(userLogin, rightIds))
            {
                Logger?.Error("Unable to add user Permissions");
            }
            Logger?.Debug("the user Permissions has been successfully added");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!_repository.RemoveUserPermissions(userLogin, rightIds))
            {
                Logger?.Error("Unable to remove user Permissions");
            }
            Logger?.Debug("the user Permissions has been successfully removed");
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var permissions = _repository.GetUserPermissions(userLogin);
            if (permissions is null)
            {
                Logger?.Error("Unable to get user permissions");
                return Enumerable.Empty<string>();
            }
            Logger?.Debug($"{nameof(GetAllPermissions)}: success");
            return permissions;
        }

        public ILogger? Logger { get; set; }
    }
}