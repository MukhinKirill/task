using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Repository.Interfaces;
using Task.Repository.Repository;
using Task.Utilities;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private IRepository _repository;
        public void StartUp(string connectionString)
        {
            try
            {
                switch (ConnectionStringService.GetProvider(connectionString))
                {
                    case Provider.MSSQL:
                        _repository = new MsSqlRepository(connectionString);
                    break;
                    default:
                        throw new InvalidOperationException($"Provider {ConnectionStringService.GetProvider(connectionString).ToString()} not supported");
                };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void CreateUser(UserToCreate user)
        {
            if(user == null || user.Login == null) return;
            try {
                User newUser = new User();
                newUser.Parse(user);

                _repository.CreateUser(newUser);
                Logger.Debug($"User {newUser.Login} created");
            } 
            catch(Exception ex) 
            {
                Logger.Error($"Fail to create user {user.Login}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }

        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var props = typeof(User).GetProperties().Where(x => x.Name != nameof(User.Login));
                var res = props.Select(x => new Property(x.Name, string.Empty)).Where(x => x.Name != "Login");

                return res.Append(new Property("Password", string.Empty));
            }
            catch (Exception ex)
            {
                Logger.Error($"Fail to get properties. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                return _repository.GetUserProperties(userLogin);
            }
            catch(Exception ex)
            {
                Logger.Error($"Fail to get properties for user: {userLogin}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                return _repository.IsUserExists(userLogin);
            }
            catch (Exception ex)
            {
                Logger.Error($"Fail to check if user exists for: {userLogin}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                _repository.UpdateUserProperties(properties, userLogin);

                Logger.Debug($"Updated the properties for user: {userLogin}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Fail to update properties for user: {userLogin}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                IEnumerable<Permission> permissions = _repository.GetAllPermissions();
                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Fail to get all permissions. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                _repository.AddUserPermissions(userLogin, rightIds);

                Logger.Debug($"Added the permissions for user: {userLogin}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Fail to add permissions for user: {userLogin}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                _repository.RemoveUserPermissions(userLogin, rightIds);

                Logger.Debug($"Removed the permissions for user: {userLogin}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Fail to remove permissions for user: {userLogin}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try 
            {
                IEnumerable<string> permissions = _repository.GetUserPermissions(userLogin);
                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Fail to get permissions for user: {userLogin}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            };
        }

        public ILogger Logger { get; set; }
    }
}