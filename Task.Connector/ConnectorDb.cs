using Task.Connector.Common.Exceptions;
using Task.Connector.Entities;
using Task.Connector.Persistence;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector, IDisposable
    {
        private bool _disposed;
        private DataContext _context = null!;
        private UserRepository _userRepository = null!;
        public ILogger Logger { get; set; } = null!;

        public void StartUp(string connectionString)
        {
            DataContextFactory factory = new("Server=localhost;Port=7900;Database=Avanpost;Username=Avanpost;Password=Avanpost;", Logger);
            _context = factory.GetContext();
            _userRepository = new(_context);
        }

        public void CreateUser(UserToCreate user)
        {
            Logger.Debug("Try to create a new user");

            try
            {
                UserModel userModel = new(
                user,
                _userRepository.GetCountUsers());

                string isLeadString = userModel.IsLead ? "User is a lead" : "User is not a lead";
                Logger.Debug($"Attempt to create a user with login '{userModel.Login}' and other properties: firstName - '{userModel.FirstName}'; middleName - '{userModel.MiddleName}'; lastName - '{userModel.LastName}'; telephoneNumber - '{userModel.TelephoneNumber}'. {isLeadString}");

                _userRepository.Create(userModel);

                Logger.Debug($"User {userModel.Login} was created");
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Get all properties");
            return UserModel.GetPropertiesName();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug($"Try get user properties for user '{userLogin}'");
            try
            {
                UserModel? user = _userRepository.GetUserByLogin(userLogin);
                if (user is null)
                {
                    Logger.Warn($"User '{userLogin}' not found");
                    throw new UserNotFoundException(userLogin);
                }

                return user.GetProperties();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Try check exists user '{userLogin}'");

            try
            {
                bool userExists = _userRepository.CheckUserExists(userLogin);

                if (userExists)
                {
                    Logger.Debug($"User '{userLogin}' exists");
                }
                else
                {
                    Logger.Warn($"User '{userLogin}' not found");
                }

                return userExists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }

        ~ConnectorDb()
        {
            Dispose(false);
        }
    }
}