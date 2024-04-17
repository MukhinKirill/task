using Task.Connector.Extensions;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DbContextFactory _dbContextFactory;
        private string _providerName;
        public void StartUp(string connectionString)
        {
            try
            {
                _dbContextFactory = new DbContextFactory(connectionString.GetDbConnectionString());
                _providerName = connectionString.GetProviderName();
                Logger.Debug($"{DateTime.Now}: Успешная попытка подключения");
            }
            catch (Exception ex)
            {
                Logger.Error($"{DateTime.Now}: Неудачная попытка подключения: {ex.Message}");
            }
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                User newUser = user.SetPropertiesOrDefault();
                Sequrity userLoginData = new Sequrity { UserId = user.Login, Password = user.HashPassword };
                using (DataContext context = _dbContextFactory.GetContext(_providerName))
                {
                    context.Users.Add(newUser);
                    context.Passwords.Add(userLoginData);
                    context.SaveChanges();
                    Logger.Debug($"{DateTime.Now}: Создан пользователь с логином {newUser.Login}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{DateTime.Now}: Ошибка при создании пользователя: {ex.Message}");
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
            using(DataContext context = _dbContextFactory.GetContext(_providerName))
            {
                if (context.Users.Find(userLogin) != null)
                {
                    Logger.Debug($"{DateTime.Now}: пользователь с логином {userLogin} найден в базе данных");
                    return true;
                }
                else
                {
                    Logger.Warn($"{DateTime.Now}: пользователя с логином {userLogin} не существует");
                    return false;
                }
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

        public ILogger Logger { get; set; }
    }
}