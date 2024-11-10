using Task.Connector.Helpers;
using Task.Connector.Services;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private IUserService _userService;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            var context = DbConnectionHelper.GetContext(connectionString);
            _userService = new UserService(context);
        }

        public void CreateUser(UserToCreate user)
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");
                
                _userService.CreateUser(user);
                
                Logger.Debug($"End method {methodName}.");
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");

                var properties = _userService.GetAllProperties();
                
                Logger.Debug($"End method {methodName}.");

                return properties;
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");

                var userProperties = _userService.GetUserProperties(userLogin);
                
                Logger.Debug($"End method {methodName}.");

                return userProperties;
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");

                var result = _userService.IsUserExists(userLogin);
                
                Logger.Debug($"End method {methodName}.");

                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");

                _userService.UpdateUserProperties(properties, userLogin);
                
                Logger.Debug($"End method {methodName}.");
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");

                var permissions = _userService.GetAllPermissions();
                
                Logger.Debug($"End method {methodName}.");

                return permissions;
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");

                _userService.AddUserPermissions(userLogin, rightIds);
                
                Logger.Debug($"End method {methodName}.");
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");

                _userService.RemoveUserPermissions(userLogin, rightIds);
                
                Logger.Debug($"End method {methodName}.");
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            const string methodName = nameof(CreateUser);
            
            try
            {
                Logger.Debug($"Start method {methodName}.");

                var userPermissions = _userService.GetUserPermissions(userLogin);
                
                Logger.Debug($"End method {methodName}.");

                return userPermissions;
            }
            catch (Exception e)
            {
                Logger.Error($"Error in method {methodName}.\n{e}");
                throw;
            }
        }
    }
}