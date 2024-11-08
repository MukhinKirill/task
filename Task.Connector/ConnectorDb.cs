using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

using Microsoft.EntityFrameworkCore;

using System.Data;
using System.Reflection;
using System.Text.Json;
using Task.Connector.Helpers.Connection;
using Task.Connector.Helpers.Permission;
using Task.Connector.Helpers.Property;
using Task.Connector.Services;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }

        private DataContext _context;

        private IUserService _userService;
        private IPermissionService _permissionService;
        private IPropertyService _propertyService;

        public void StartUp(string connectionString)
        {
            _context = ConnectionHelper.GetContext(connectionString);
            _userService = new UserService(_context);
            _permissionService = new PermissionService(_context);
            _propertyService = new PropertyService();
        }
        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            Logger.Debug($"Input parameters: {userLogin}");
            try
            {
                return _userService.IsUserExists(userLogin);
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
        public void CreateUser(UserToCreate user)
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            Logger.Debug($"Input parameters: {JsonSerializer.Serialize(user)}");
            try
            {
                _userService.CreateUser(user);
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            try
            {
                return _propertyService.GetAllProperties();
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            Logger.Debug($"Input parameters: {userLogin}");
            try
            {
                return _userService.GetUserProperties(userLogin);
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            Logger.Debug($"Input parameters: userLogin: {userLogin}, properties: {JsonSerializer.Serialize(properties)}");
            try
            {
                _userService.UpdateUserProperties(properties, userLogin);
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            try
            {
                return _permissionService.GetAllPermissions();
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            Logger.Debug($"Input parameters: {userLogin}");
            try
            {
                return _userService.GetUserPermissions(userLogin);
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            Logger.Debug($"Input parameters: userLogin: {userLogin}, rightIds:{JsonSerializer.Serialize(rightIds)}");
            try
            {
                _userService.AddUserPermissions(userLogin, rightIds);
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Perform {MethodBase.GetCurrentMethod().Name}");
            Logger.Debug($"Input parameters: userLogin: {userLogin}, rightIds:{JsonSerializer.Serialize(rightIds)}");
            try
            {
                _userService.RemoveUserPermissions(userLogin, rightIds);
            }
            catch (Exception ex) { Logger.Error(ex.Message); throw; }
        }
    }
}