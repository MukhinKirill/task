using Task.Connector.Exceptions;
using Task.Connector.Factories;
using Task.Connector.Interfaces;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Connectors
{
    public class ConnectorDb : IConnectorDb
    {
        public ILogger Logger { get; set; }

        private IConnectorDb _connector;

        public void StartUp(string connectionString)
        {
            if (Logger is null)
                throw new ArgumentNullException("Logger dot't register");
            
            var configuration = new ConnectionConfiguration(connectionString);
            _connector = ConnectorFactory.GetConnector(configuration.Provider);

            Logger.Debug($"Use connector provider for {configuration.Provider}");

            _connector.StartUp(connectionString);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Add user permissions by login - {userLogin}");

            try
            {
                _connector.AddUserPermissions(userLogin, rightIds);
            }
            catch (UserNotFoundException ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public void CreateUser(UserToCreate user)
        {
            Logger.Debug("Creating user");

            try
            {
                _connector.CreateUser(user);
            }
            catch (UserLoginNotUniqueException ex)
            {
                Logger.Warn(ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug("Get all permissions");
            
            try
            {
                return _connector.GetAllPermissions();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Get all properties");

            try
            {
                return _connector.GetAllProperties();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug("Get all permissions");

            try
            {
                return _connector.GetUserPermissions(userLogin);
            }
            catch (UserNotFoundException ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug($"Get user properties by login - {userLogin}");

            try
            {
                return _connector.GetUserProperties(userLogin);
            }
            catch (UserNotFoundException ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Check user exists by login - {userLogin}");

            try
            {
                return _connector.IsUserExists(userLogin);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Remove user permissions by login - {userLogin}");

            try
            {
                _connector.RemoveUserPermissions(userLogin, rightIds);
            }
            catch (UserNotFoundException ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug($"Update user properties by login - {userLogin}");

            try
            {
                _connector.UpdateUserProperties(properties, userLogin);
            }
            catch (UserNotFoundException ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                throw;
            }
        }

        public void Dispose()
        {
            Logger.Debug("Dispose connector");

            _connector.Dispose();
        }

        ~ConnectorDb()
        {
            Dispose();
        }
    }
}
