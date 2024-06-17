﻿using Task.Connector.Database;
using Task.Connector.Services.Permission;
using Task.Connector.Services.User;
using Task.Connector.Services.UserPermission;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataBaseContext _db;
        private IUserService _userService;
        private IPermissionService _permissionService;
        private IUserPermission _userPermissionService;

        public void StartUp(string connectionString)
        {
            _db = new DataBaseContext(connectionString);
            if (!_db.Connected)
            {
                throw new InvalidOperationException("No database connection available");
            }

            _userService = new UserService();
            _permissionService = new PermissionService();
            _userPermissionService = new UserPermissionService();
        }

        public void CreateUser(UserToCreate user)
            => _userService.CreateUser(user);
        public IEnumerable<Property> GetAllProperties()
            => _userService.GetAllProperties();
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
            => _userService.GetUserProperties(userLogin);
        public bool IsUserExists(string userLogin)
            => _userService.IsUserExists(userLogin);
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
            => _userService.UpdateUserProperties(properties, userLogin);

        public IEnumerable<Permission> GetAllPermissions()
            => _permissionService.GetAllPermissions();

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
            => _userPermissionService.AddUserPermissions(userLogin, rightIds);
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
            => _userPermissionService.RemoveUserPermissions(userLogin, rightIds);
        public IEnumerable<string> GetUserPermissions(string userLogin)
            => _userPermissionService.GetUserPermissions(userLogin);

        public ILogger Logger { get; set; }
    }
}