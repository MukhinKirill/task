using Microsoft.Extensions.DependencyInjection;
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
            connectionString = new ConnectionStringParser().Parse(connectionString);

            try
            {
                ServiceProviderSingleton.RegisterServices(connectionString);

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

                foreach (var property in user.Properties)
                    Logger.Debug($"\t- {property.Name} -> {property.Value}");

                /*if (_userRepository.IsUserExists(user.Login))
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
                }*/
                
                _userRepository.CreateUser(user);
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
                return _userRepository.IsUserExists(userLogin);
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

                    /*var _properties = _userRepository.GetUserProperties(userLogin);
                    var dictProperties = new Dictionary<string, string>();

                    foreach (var property in _properties)
                        dictProperties.Add(property.Name, property.Value);*/

                    _userRepository.UpdateUserProperties(properties, userLogin);

                   /* _properties = _userRepository.GetUserProperties(userLogin);

                    foreach(var property in _properties)
                       if(dictProperties[property.Name] != property.Value)
                            Logger.Debug($"\t - Change {property.Name} \t {dictProperties[property.Name]} -> {property.Value}");*/
                }
                else
                {
                    Logger.Error($"User {userLogin} doesnt exist");
                }
            }
            else
            {
                Logger.Error(new NullReferenceException().Message);
            }
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