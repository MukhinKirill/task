using System.Reflection;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _dataContext; // Контекст данных для взаимодействия с базой данных
        private DataManager _dataManager; // Менеджер данных для работы с данными
        private string _providerName = "POSTGRE"; // Название поставщика базы данных
        static string requestRightGroupName = "Request"; // Название группы прав для запросов
        static string itRoleRightGroupName = "Role"; // Название группы прав для ролей
        static string split = ":"; // Разделитель для разделения идентификаторов

        public const string DefaultConnectionString = // Стандартная строка подключения к базе данных
            "Host=localhost;Port=5432;Database=ConnectorDb;Username=root;Password=myPassword";

        // Метод для инициализации подключения к базе данных
        public void StartUp(string connectionString)
        {
            var dbContextFactory = new DbContextFactory(DefaultConnectionString);
            _dataContext = dbContextFactory.GetContext(_providerName);
            _dataManager = new DataManager(dbContextFactory, _providerName);
        }

        // Метод для создания нового пользователя
        public void CreateUser(UserToCreate user)
        {
            if (_dataManager.GetUser(user.Login) is null)
            {
                // Создание нового пользователя
                User newUser = new User();
                newUser.Login = user.Login;

                // Заполнение значений по умолчанию
                newUser.TelephoneNumber = "88005553535";
                newUser.FirstName = "Bob";
                newUser.LastName = "Rider";
                newUser.MiddleName = "Sir";
                try
                {
                    newUser.IsLead = bool.Parse(user.Properties.FirstOrDefault(x => x.Name == "isLead").Value);
                }
                catch
                {
                    newUser.IsLead = false;
                }

                // Добавление пользователя в базу данных
                _dataContext.Users.Add(newUser);
                _dataContext.Passwords.Add(new Sequrity() { UserId = user.Login, Password = user.HashPassword });
                _dataContext.SaveChanges();
                Logger.Debug(
                    $"Added user {user.Login} with default properties"); // Запись сообщения в лог о создании пользователя
            }
            else
            {
                Logger.Error(
                    $"User with login {user.Login} already exists"); // Запись сообщения в лог об ошибке, если пользователь уже существует
            }
        }

        // Метод для получения всех свойств пользователя
        public IEnumerable<Property> GetAllProperties()
        {
            var properties = new List<Property>();

            var userProperties = typeof(User).GetProperties();
            properties.Add(new Property("Password", "Password"));

            var filteredProperties = userProperties
                .Where(prop => prop.Name.ToLower() != "login")
                .Select(prop => new Property(prop.Name, prop.Name));

            properties.AddRange(filteredProperties);

            return properties.AsEnumerable();
        }

        // Метод для получения свойств конкретного пользователя
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _dataManager.GetUser(userLogin);
            var properties = new List<UserProperty>();

            if (user != null)
            {
                var userProperties = typeof(User).GetProperties();

                var filteredProperties = userProperties
                    .Where(prop => prop.Name.ToLower() != "login")
                    .Select(prop => new UserProperty(prop.Name, prop.GetValue(user)?.ToString()));

                properties.AddRange(filteredProperties);

                Logger.Debug(
                    $"Returned properties of {userLogin}"); // Запись сообщения в лог о возврате свойств пользователя
            }
            else
            {
                Logger.Warn(
                    $"Couldn't find user {userLogin}"); // Запись предупреждения в лог, если пользователь не найден
            }

            return properties;
        }

        // Метод для проверки существования пользователя
        public bool IsUserExists(string userLogin)
        {
            try
            {
                var user = _dataManager.GetUser(userLogin);
                if (user is not null)
                {
                    Logger.Debug(
                        $"It's glad to hear that {userLogin} is not fired"); // Запись сообщения в лог о наличии пользователя
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                Logger.Error(
                    "Can't check if user exists"); // Запись сообщения об ошибке в лог, если не удалось проверить существование пользователя
                return false;
            }
        }

        // Метод для обновления свойств пользователя
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using (var transaction = _dataContext.Database.BeginTransaction())
            {
                try
                {
                    var user = _dataManager.GetUser(userLogin);
                    var password = _dataContext.Passwords.FirstOrDefault(x => x.UserId == userLogin);

                    properties = properties.Where(x => x.Name.ToLower() != "login");

                    foreach (UserProperty property in properties)
                    {
                        if (property.Name.ToLower() == "password" && password != null)
                        {
                            password.Password = property.Value;
                            password.UserId = userLogin;
                            _dataContext.Passwords.Update(password);
                        }
                        else
                        {
                            PropertyInfo propToUpdate = typeof(User).GetProperty(property.Name);

                            if (propToUpdate != null)
                            {
                                propToUpdate.SetValue(user,
                                    Convert.ChangeType(property.Value, propToUpdate.PropertyType));
                            }
                        }
                    }

                    _dataContext.Users.Update(user);
                    _dataContext.SaveChanges();

                    transaction.Commit();
                    Logger.Debug(
                        $"{userLogin} is updated"); // Запись сообщения в лог об успешном обновлении пользователя
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error(
                        $"Error updating user properties for {userLogin}: {ex.Message}"); // Запись сообщения об ошибке в лог при обновлении пользовательских свойств
                    throw;
                }
            }
        }

        // Метод для получения всех разрешений
        public IEnumerable<Permission> GetAllPermissions()
        {
            var itRolesPermissions = _dataContext.ITRoles
                .Select(x => new Permission(x.Id.ToString(), x.Name, itRoleRightGroupName)).ToList();
            var requestRightsPermissions = _dataContext.RequestRights
                .Select(x => new Permission(x.Id.ToString(), x.Name, requestRightGroupName)).ToList();

            var permissions = itRolesPermissions.Union(requestRightsPermissions).ToList();

            Logger.Debug(
                "Loaded all permissions in system"); // Запись сообщения в лог о загрузке всех разрешений в системе

            return permissions;
        }


        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = _dataManager.GetUser(userLogin);
            foreach (var rightId in rightIds)
            {
                try
                {
                    var splitted = rightId.Split(split);
                    if (splitted[0] == itRoleRightGroupName)
                    {
                        var role = _dataContext.ITRoles.FirstOrDefault(x => x.Id.ToString() == splitted[1]);
                        if (role != null)
                        {
                            _dataContext.UserITRoles.Add(
                                new UserITRole() { RoleId = role.Id ?? 0, UserId = user.Login });
                            _dataContext.SaveChanges();
                        }
                        else
                        {
                            Logger.Error($"Can't find IT roles like {rightId}");
                        }
                    }

                    if (splitted[0] == requestRightGroupName)
                    {
                        var role = _dataContext.RequestRights.FirstOrDefault(x => x.Id.ToString() == splitted[1]);
                        if (role != null)
                        {
                            _dataContext.UserRequestRights.Add(new UserRequestRight()
                                { RightId = role.Id ?? 0, UserId = user.Login });
                            _dataContext.SaveChanges();
                        }
                        else
                        {
                            Logger.Error($"Can't find request rights like {rightId}");
                        }
                    }
                }
                catch
                {
                    Logger.Error($"Error while adding new permissions for {userLogin}");
                }
            }
        }

        /// <summary>
        /// Удаление разрешений
        /// </summary>
        /// <param name="userLogin">логин пользователя</param>
        /// <param name="rightIds">id - шники прав</param>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = _dataManager.GetUser(userLogin);
            foreach (var rightId in rightIds)
            {
                try
                {
                    var splitted = rightId.Split(split);
                    var groupName = splitted[0];
                    var roleId = int.Parse(splitted[1]);

                    if (groupName == itRoleRightGroupName)
                    {
                        var userRole =
                            _dataContext.UserITRoles.FirstOrDefault(x => x.RoleId == roleId && x.UserId == userLogin);
                        if (userRole != null)
                        {
                            _dataContext.UserITRoles.Remove(userRole);
                            _dataContext.SaveChanges();
                            Logger.Debug($"Removed IT role {roleId} from user {userLogin}");
                        }
                        else
                        {
                            Logger.Error($"Can't find IT role with ID {roleId} for user {userLogin}");
                        }
                    }

                    if (groupName == requestRightGroupName)
                    {
                        var userRequestRight =
                            _dataContext.UserRequestRights.FirstOrDefault(x =>
                                x.RightId == roleId && x.UserId == userLogin);
                        if (userRequestRight != null)
                        {
                            _dataContext.UserRequestRights.Remove(userRequestRight);
                            _dataContext.SaveChanges();
                            Logger.Debug($"Removed request right {roleId} from user {userLogin}");
                        }
                        else
                        {
                            Logger.Error($"Can't find request right with ID {roleId} for user {userLogin}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error while removing permissions for user {userLogin}: {ex.Message}");
                }
            }
        }


        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var requestRights = _dataManager.GetCRequestRightsByUser(userLogin)
                .Select(x => $"{requestRightGroupName}{split}{x.RightId}");
            var itRoles = _dataManager.GetITRolesByUser(userLogin)
                .Select(x => $"{itRoleRightGroupName}{split}{x.RoleId}");

            var permissions = requestRights.Concat(itRoles);

            Logger.Debug($"Loaded {userLogin} permissions");

            return permissions;
        }


        public ILogger Logger { get; set; }
    }
}