using Microsoft.Extensions.DependencyInjection;
using Task.Connector.Extensions;
using Task.Connector.Parsers;
using Task.Connector.Repositories;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private IUserRepository? _userRepository;
        private IPermissionRepository? _permissionRepository;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            try
            {
                ServiceProviderSingleton.RegisterServices(new ConnectionStringParser().Parse(connectionString));

                _userRepository = ServiceProviderSingleton.ServiceProvider?.GetRequiredService<IUserRepository>();
                _permissionRepository = ServiceProviderSingleton.ServiceProvider?.GetRequiredService<IPermissionRepository>();
                
                Logger.Debug("Connector initialized");
            }
            catch(Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public void CreateUser(UserToCreate user)
        {
            if (_userRepository != null)
            {
                Logger.Debug($"Try create user: {user.Login}");

                _userRepository.CreateUser(user);

                foreach (var property in user.Properties)
                    Logger.Debug($"\t- {property.Name} -> {property.Value}");

                if (_userRepository.IsUserExists(user.Login))
                {
                    Logger.Debug($"User {user.Login} created");

                    foreach (var property in _userRepository.GetUserProperties(user.Login))
                    {
                        Logger.Debug($"\t- {property.Name} -> {property.Value}");
                    }
                }
                else
                {
                    Logger.Error("User creating failure");
                }
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            if (_userRepository != null)
            {
                IEnumerable<Property> properties = _userRepository.GetAllProperties();

                Logger.Debug("Get All properties");

                foreach (var property in properties)
                    Logger.Debug($"\t- {property.Name}");

                return _userRepository.GetAllProperties();
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (_userRepository != null)
            {
                if (_userRepository.IsUserExists(userLogin))
                {
                    IEnumerable<UserProperty> userProperties = _userRepository.GetUserProperties(userLogin);

                    Logger.Debug($"Get {userLogin} properties");

                    foreach (var userProperty in userProperties)
                        Logger.Debug($"\t- {userProperty.Name} -> {userProperty.Value}");

                    return _userRepository.GetUserProperties(userLogin);
                }
                else
                {
                    Logger.Error($"User {userLogin} doesnt exist");

                    return null;
                }
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }

        public bool IsUserExists(string userLogin)
        {
            if (_userRepository != null)
            {
                var result = _userRepository.IsUserExists(userLogin);

                Logger.Debug(result ? $"User {userLogin} exist" : $"User {userLogin} doesnt exist");

                return result;
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (_userRepository != null)
            {
                if (_userRepository.IsUserExists(userLogin))
                {
                    Logger.Debug($"Update {userLogin} properties");

                    var oldPropertiesDict = _userRepository.GetUserProperties(userLogin).ConvertToDict();

                    _userRepository.UpdateUserProperties(properties, userLogin);

                    foreach (var property in _userRepository.GetUserProperties(userLogin))
                    {
                        if (oldPropertiesDict[property.Name] != property.Value)
                        {
                            Logger.Debug($"\t- {property.Name}: {oldPropertiesDict[property.Name]} -> {property.Value}");
                        }
                    }
                }
                else
                {
                    Logger.Error($"User {userLogin} doesnt exist");
                }
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            if (_permissionRepository != null)
            {
                Logger.Debug("Get All permissions");

                var permissions = _permissionRepository.GetAllPermissions();

                foreach(var permission in permissions)
                    Logger.Debug($"\t- {permission.Id} -> {permission.Description}");

                return permissions;
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (_permissionRepository != null && _userRepository != null)
            {
                if (_userRepository.IsUserExists(userLogin))
                {
                    Logger.Debug($"Add {userLogin} permissions");

                    foreach(var right in rightIds)
                        Logger.Debug($"\t- {right}");

                    _permissionRepository.AddUserPermissions(userLogin, rightIds);
                }
                else
                {
                    Logger.Error($"User {userLogin} doesnt exist");
                }
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (_permissionRepository != null && _userRepository != null)
            {
                if (_userRepository.IsUserExists(userLogin))
                {
                    Logger.Debug($"Remove {userLogin} permissions");

                    foreach (var right in rightIds)
                        Logger.Debug($"\t- {right}");

                    _permissionRepository.RemoveUserPermissions(userLogin, rightIds);
                }
                else
                {
                    Logger.Error($"User {userLogin} doesnt exist");
                }
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (_permissionRepository != null && _userRepository != null)
            {
                if (_userRepository.IsUserExists(userLogin))
                {
                    Logger.Debug($"Get {userLogin} permissions");

                    var permissions = _permissionRepository.GetUserPermissions(userLogin);

                    foreach (var permission in permissions)
                        Logger.Debug($"\t- {permission}");

                    return permissions;
                }
                else
                {
                    Logger.Error($"User {userLogin} doesnt exist");

                    return null;
                }
            }
            else
            {
                Logger.Error("The class must be initialized before calling methods");

                throw new NullReferenceException();
            }
        }
    }
}