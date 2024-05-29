using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

using Task.Connector.Extensions;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DbContextFactory _dbContextFactory;
        private string _provider;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            _dbContextFactory = new DbContextFactory(connectionString.GetDbConnectionString());
            _provider = connectionString.GetDbProvider();
        }

        public void CreateUser(UserToCreate user)
        {
            var userData = user.SetOrDefaultProperties();
            var userCredentials = new Sequrity()
            {
                UserId = user.Login,
                Password = user.HashPassword
            };
            try
            {
                using (var context = _dbContextFactory.GetContext(_provider))
                {
                    context.Users.Add(userData);
                    context.Passwords.Add(userCredentials);
                    context.SaveChanges();
                }

                Logger.Debug($"The user \"{user.Login}\" has been successfully created!");
            }
            catch(Exception e)
            {
                Logger.Error($"Failed to create user \"{user.Login}\"!\n" +
                             $"The following error occurred: {e.Message}\n" +
                             $"The inner exception: {e.InnerException?.Message}");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug($"A list of user properties has been requested.");
            return new Property[]
            {
                new Property("First name", "Имя"),
                new Property("Middle name", "Отчество"),
                new Property("Last name", "Фамилия"),
                new Property("Telephone number", "Номер телефона"),
                new Property("Is lead", "Является ли лидером"),
                new Property("Password", "Пароль")
            };
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
    }
}