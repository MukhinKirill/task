using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DbContextFactory? _contextFactory;
        private string? _provider;

        public required ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            var innnerConnString = connectionString.GetInnerConnectionString();
            _contextFactory = new DbContextFactory(innnerConnString);
            _provider = connectionString.GetProviderName();
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                var userForDb = new User {
                    Login = user.Login
                };

                var password = new Sequrity
                {
                    Password = user.HashPassword,
                    UserId = user.Login
                };

                userForDb.SetPropertiesToUser(user.Properties);

                using var context = GetDataContext();
                context.Users.Add(userForDb);
                context.Passwords.Add(password);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while creating a user. \nError: {ex}");
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }

        public bool IsUserExists(string userLogin)
        {
            throw new NotImplementedException();
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

        private DataContext GetDataContext()
        {
            if (_contextFactory == null || _provider == null)
            {
                Logger.Error($"Before using {nameof(ConnectorDb)}, you must call the {nameof(StartUp)} method to configure it.");
                throw new Exception("StartUp method wasn't called.");
            }

            return _contextFactory.GetContext(_provider);
        }
    }
}