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
            }
            catch (Exception ex)
            {
                
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
                    Logger.Debug($"Создан пользователь с логином {newUser.Login}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка при создании пользователя: {ex.Message}");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            IEnumerable<UserProperty> userProperties = new List<UserProperty>();
            try
            {
                using (DataContext context = _dbContextFactory.GetContext(_providerName))
                {
                    User? user = context.Users.Find(userLogin);
                    if (user != null)
                    {
                        userProperties = new List<UserProperty>
                        {
                            new UserProperty("First name", user.FirstName),
                            new UserProperty("Middle name", user.MiddleName),
                            new UserProperty("Last name", user.LastName),
                            new UserProperty("Telephone number", user.TelephoneNumber),
                            new UserProperty("Is lead", user.IsLead.ToString()),
                        };
                        Logger.Debug($"Запрошены свойства пользователя {userLogin}");
                    }
                    else
                    {
                        Logger.Warn($"Запрошены свойства пользователя {userLogin}, но пользователь не был найден");
                        return userProperties;
                    }

                }
            }
            catch(Exception ex)
            {
                Logger.Error($"{DateTime.Now}: Ошибка при запросе свойств пользователя: {ex.Message}");
            }
            return userProperties;
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
            try
            {
                using (DataContext context = _dbContextFactory.GetContext(_providerName))
                {
                    User? user = context.Users.Find(userLogin);
                    if (user != null)
                    {
                        foreach (UserProperty property in properties)
                        {
                            switch(property.Name)
                            {
                                case "First name":
                                    user.FirstName = property.Value;
                                    break;
                                case "Middle name":
                                    user.MiddleName = property.Value;
                                    break;
                                case "Last name":
                                    user.LastName = property.Value;
                                    break;
                                case "Telephone number":
                                    user.TelephoneNumber = property.Value;
                                    break;
                                case "Is lead":
                                    user.IsLead = Convert.ToBoolean(property.Value);
                                    break;
                            }
                        }
                        context.Users.Update(user);
                        context.SaveChanges();
                    }
                    else
                    {
                        Logger.Warn($"Попытка изменить свойства пользователя {userLogin}, но пользователь не был найден");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка при изменении свойств пользователя {userLogin}: {ex.Message}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            List<Permission> allPermissions = new List<Permission>();
            try
            {
                using(DataContext context = _dbContextFactory.GetContext(_providerName))
                {                    foreach (ITRole itRole in context.ITRoles)
                    {
                        allPermissions.Add(new Permission(itRole.Id.ToString(), itRole.Name, "Роль исполнителя"));
                    }
                    foreach (RequestRight requestRight in context.RequestRights)
                    {
                        allPermissions.Add(new Permission(requestRight.Id.ToString(), requestRight.Name, "Право по изменению заявок"));
                    }
                    Logger.Debug("Запрос списка всех прав в системе");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка при запросе всех прав в системе {ex.Message}");
            }
            return allPermissions;
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