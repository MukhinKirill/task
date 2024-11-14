using System;
using System.Linq;
using System.Reflection;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private TestavanpostContext _dbContext;
        public ILogger Logger { get; set; }

        // Пустой конструктор
        public ConnectorDb()
        {
        }

        public void StartUp(string connectionString)
        {
            if (connectionString.ToLower().Contains("mssql")) { return; }
            _dbContext = new TestavanpostContext(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            if (IsUserExists(user.Login))
            {
                Logger.Warn($"Пользователь с логином {user.Login} уже существует.");
                return;
            }

            // Создаем нового пользователя
            var newUser = new User
            {
                Login = user.Login,
                LastName = GetPropertyValue(user.Properties, "LastName"),
                FirstName = GetPropertyValue(user.Properties, "FirstName"),
                MiddleName = GetPropertyValue(user.Properties, "MiddleName"),
                TelephoneNumber = GetPropertyValue(user.Properties, "TelephoneNumber"),
                IsLead = bool.TryParse(GetPropertyValue(user.Properties, "IsLead"), out bool isLeadValue) ? isLeadValue : false
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            // Здесь можно дополнительно сохранить свойство пароля в базу данных
            _dbContext.Passwords.Add(new Password { UserId = newUser.Login, Password1 = user.HashPassword });
            _dbContext.SaveChanges();

            Logger.Debug($"Пользователь {user.Login} успешно создан.");
        }

        public bool IsUserExists(string userLogin)
        {
            return _dbContext.Users.Any(u => u.Login == userLogin);
        }
        public bool IsUserExists(string userLogin, out User? user)
        {
            user = _dbContext.Users.FirstOrDefault(u => u.Login == userLogin);
            return user == null;
        }

        //Чтобы в дальнейшем могли тянуть не только наименования свойств
        public IEnumerable<Property> GetAllProperties()
        {
            var userProperties = typeof(User).GetProperties().Where(up => up.Name != "Login")
            .Select(up => new Property(up.Name, ""))
            .ToList();

            // Объединяем свойства и пароль в один список
            userProperties.Add(new Property("Password", ""));

            return userProperties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (IsUserExists(userLogin, out var user))
            {
                Logger.Warn($"Пользователь {userLogin} не найден.");
                return Enumerable.Empty<UserProperty>();
            }

            var userProperties = typeof(User).GetProperties().Where(up => up.Name != "Login")
            .Select(up => new UserProperty(up.Name, up.GetValue(user).ToString()))
            .ToList();

            return userProperties; 
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (IsUserExists(userLogin, out var user))
            {
                Logger.Warn($"Пользователь {userLogin} не найден.");
                return;
            }

            var userProperties = GetUserProperties(userLogin);

            foreach (var property in properties)
            {
                var userProperty = userProperties.FirstOrDefault(up => up.Name == property.Name);
                if (userProperty != null)
                {
                    user.GetType().GetProperty(property.Name).SetValue(user, property.Value);
                }
            }
            _dbContext.SaveChanges();
            Logger.Debug($"Свойства пользователя {userLogin} успешно обновлены.");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var requestRights = _dbContext.RequestRights.ToList();
            var itRoles = _dbContext.ItRoles.ToList();

            return requestRights.Select(right => new Permission(right.Id.ToString(), right.Name, ""))
            .Concat(itRoles.Select(role => new Permission(role.Id.ToString(), role.Name, "")));
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (IsUserExists(userLogin, out var user))
            {
                Logger.Warn($"Пользователь {userLogin} не найден.");
                return;
            }

            foreach (var rightId in rightIds)
            {
                var right = rightId.Split(':');
                if (right[0] == "Role")
                    _dbContext.UserItroles.Add(new UserItrole { UserId = user.Login, RoleId = int.Parse(right[1]) });
                else
                    _dbContext.UserRequestRights.Add(new UserRequestRight { UserId = user.Login, RightId = int.Parse(right[1]) });

            }
            _dbContext.SaveChanges();
            Logger.Debug($"Права добавлены пользователю {userLogin}.");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (IsUserExists(userLogin, out var user))
            {
                Logger.Warn($"Пользователь {userLogin} не найден.");
                return;
            }

            foreach (var rightId in rightIds)
            {
                var right = rightId.Split(':');

                if (right[0] == "Role")
                {
                    var userRole = _dbContext.UserItroles.FirstOrDefault(ur => ur.UserId == user.Login && ur.RoleId == int.Parse(right[1]));
                    if (userRole != null)
                    {
                        _dbContext.UserItroles.Remove(userRole);
                    }
                }
                else
                {
                    var userRight = _dbContext.UserRequestRights.FirstOrDefault(ur => ur.UserId == user.Login && ur.RightId == int.Parse(right[1]));
                    if (userRight != null)
                    {
                        _dbContext.UserRequestRights.Remove(userRight);
                    }
                }
            }
            _dbContext.SaveChanges();
            Logger.Debug($"Права удалены у пользователя {userLogin}.");
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (IsUserExists(userLogin, out var user))
            {
                Logger.Warn($"Пользователь {userLogin} не найден.");
                return Enumerable.Empty<string>();
            }

            return (IEnumerable<string>)_dbContext.UserRequestRights
               .Where(ur => ur.UserId == user.Login)
               .Select(ur => ur.RightId.ToString())
           .ToList();
        }

        private string GetPropertyValue(IEnumerable<UserProperty> properties, string propertyName)
        {
            var property = properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            return property?.Value ?? string.Empty;
        }
    }
}