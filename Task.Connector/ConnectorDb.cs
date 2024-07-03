using Microsoft.EntityFrameworkCore;
using Task.Connector.Context;
using Task.Connector.Entities;
using Task.Connector.Errors;
using Task.Connector.Interfaces;
using Task.Connector.Services;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Property = Task.Integration.Data.Models.Models.Property;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        
        private DatabaseContext? _context;
        
        private IUserInterface _userService;

        private IPermissionService _permissionService;

        public ConnectorDb()
        {
            
        }
        
        public void StartUp(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>().UseNpgsql(connectionString);
            _context = new DatabaseContext(optionsBuilder.Options);
            if(_context is null) Error.Throw(Logger, new NullReferenceException("База данных не инициализирована"));
            
            _userService = new UserService(_context!, Logger);
            _permissionService = new PermissionService(_context!, Logger);
        }
        
        public void CreateUser(UserToCreate user)
        {
            _userService.CreateUser(user);
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return _userService.GetAllProperties();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            return _userService.GetUserProperties(userLogin);
        }

        public bool IsUserExists(string userLogin)
        {
            return _userService.IsUserExists(userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            _userService.UpdateUserProperties(properties, userLogin);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return _permissionService.GetAllPermissions();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            _permissionService.AddUserPermissions(userLogin, rightIds);
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            _permissionService.RemoveUserPermissions(userLogin, rightIds);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            return _permissionService.GetPermissionByUserLogin(userLogin);
        }
    }
}