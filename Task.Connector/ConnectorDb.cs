using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Task.Connector.DB.Models;
using Task.Connector.DB;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        // Логгер для вывода сообщений
        public ILogger Logger { get; set; }

        // Контекст базы данных
        private ApplicationContext appContext;

        public void StartUp(string connectionString)
        {
            appContext = new ApplicationContext(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            // Создаем нового пользователя с данными из входного объекта user
            var newUser = new User
            {
                Login = user.Login,
                FirstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "firstName",
                LastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "lastName",
                MiddleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "middleName",
                TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ?? "telephoneNumber",
                IsLead = user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value == "true",
            };

            // Добавляем пользователя в базу данных
            appContext.Users.Add(newUser);

            // Создаем запись пароля для пользователя
            var passwordEntry = new Password
            {
                UserId = user.Login,
                Pass = user.HashPassword,
            };

            // Добавляем пароль в базу данных
            appContext.Passwords.Add(passwordEntry);

            // Сохраняем изменения в базе данных
            appContext.SaveChanges();

            // Логируем успешное создание пользователя
            Logger.Debug($"Новый пользователь с логином - {user.Login} был создан!");
        }

        public IEnumerable<Property> GetAllProperties()
        {
            // Получаем первого пользователя из контекста
            User user = appContext.Users.FirstOrDefault();
            if (user == null)
            {
                return new List<Property>();
            }

            // Находим пароль, связанный с пользователем
            Password password = appContext.Passwords.FirstOrDefault(p => p.UserId == user.Login);
            if (password == null)
            {
                return new List<Property>();
            }

            // Получаем свойство пароля и все свойства пользователя
            PropertyInfo passwordProperty = password.GetType().GetProperty("Pass");
            PropertyInfo[] userProperties = user.GetType().GetProperties();

            // Заменяем свойство Login пользователя на Pass, если индекс найден
            int loginIndex = Array.IndexOf(userProperties, user.GetType().GetProperty("Login"));
            if (loginIndex >= 0 && passwordProperty != null)
            {
                userProperties[loginIndex] = passwordProperty;
            }

            // Создаем список всех свойств пользователя
            var properties = userProperties.Select(property => new Property(property.Name, " ")).ToList();

            // Логируем успешное получение всех свойств пользователя
            Logger.Debug($"Все свойства пользователя с логином - {user.Login} были получены");

            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            // Ищем пользователя по логину
            var user = appContext.Users.FirstOrDefault(u => u.Login == userLogin);

            // Логируем ошибку, если пользователь не найден
            if (user == null)
            {
                Logger.Error($"Пользователь с логином - {userLogin} не существует");
                return Enumerable.Empty<UserProperty>();
            }

            // Получаем свойства пользователя, исключая первичный ключ
            var userProperties = appContext.Model.FindEntityType(typeof(User))?
                .GetProperties()
                .Where(p => !p.IsPrimaryKey()) // Исключаем первичный ключ
                .Select(p => new UserProperty(p.GetColumnName(), p.PropertyInfo?.GetValue(user)?.ToString() ?? string.Empty))
                ?? Enumerable.Empty<UserProperty>();

            // Логируем успешное получение свойств пользователя
            Logger.Debug($"Свойства пользователя с логином - {userLogin} были получены");

            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            if (appContext.Users.Any(user => user.Login == userLogin) == true) 
            {
                Logger.Debug($"Пользователь с логином - {userLogin} существует");
                return true;
            }
            else
            {
                Logger.Debug($"Пользователь с логином - {userLogin} не существует");
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            // Ищем пользователя по логину
            var user = appContext.Users.FirstOrDefault(u => u.Login == userLogin);

            // Если пользователь не найден, завершаем выполнение
            if (user == null)
            {
                Logger.Warn($"Пользователь с логином - {userLogin} не существует");
                return;
            }

            // Словарь с действиями по обновлению свойств пользователя
            var propertyActions = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "lastname", value => user.LastName = value },
                { "firstname", value => user.FirstName = value },
                { "middlename", value => user.MiddleName = value },
                { "telephonenumber", value => user.TelephoneNumber = value },
                { "islead", value => user.IsLead = Convert.ToBoolean(value) }
            };

            // Обновляем свойства пользователя на основе переданных данных
            foreach (var property in properties)
            {
                if (propertyActions.TryGetValue(property.Name, out var action))
                {
                    action(property.Value);
                }
                else
                {
                    throw new ArgumentException($"Неизвестное свойство: {property.Name}");
                }
            }

            Logger.Debug($"Свойства пользователя с логином - {userLogin} обновлены");
            // Сохраняем изменения в базе данных
            appContext.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            // Получаем список прав и ролей из базы данных
            var requestRights = appContext.RequestRights.ToList();
            var itRoles = appContext.ItRoles.ToList();

            // Формируем список разрешений для прав
            var requestRightsPermission = requestRights.Select(requestRight => new Permission
            (
                Guid.NewGuid().ToString(),
                requestRight.Name,
                ""
            ));

            // Формируем список разрешений для ролей
            var itRolesPermission = itRoles.Select(requestRight => new Permission
            (
                Guid.NewGuid().ToString(),
                requestRight.Name,
                ""
            ));

            Logger.Debug("Все разрешения были получены.");

            return requestRightsPermission.Union(itRolesPermission);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            // Проверка существования пользователя по логину
            bool userExist = IsUserExists(userLogin);
            if (userExist)
            {
                foreach (var permission in rightIds)
                {
                    // Разделение строки права для обработки
                    string[] words = permission.Split(':', StringSplitOptions.RemoveEmptyEntries);

                    // Добавление роли пользователю
                    if (permission.StartsWith("Role"))
                    {
                        int roleId = int.Parse(words[1]);
                        appContext.UserITRoles.Add(new UserITRole { UserId = userLogin, RoleId = roleId });
                        appContext.SaveChanges();
                    }
                    // Добавление запроса прав пользователю
                    if (permission.StartsWith("Request"))
                    {
                        int requestId = int.Parse(words[1]);
                        appContext.UserRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = requestId });
                        appContext.SaveChanges();
                    }
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            // Проверка, что логин пользователя указан
            if (string.IsNullOrEmpty(userLogin))
            {
                Logger.Error("Логин пользователя не может быть пустым.");
                return;
            }

            // Проверка, что переданы идентификаторы прав
            if (rightIds == null || !rightIds.Any())
            {
                Logger.Error("Не указаны идентификаторы прав.");
                return;
            }

            // Преобразуем и фильтруем права для удаления
            var userRequestRights = rightIds
                .Select(rightId => rightId.Split(":"))
                .Where(parts => parts.Length >= 2 && int.TryParse(parts[1], out _))
                .Select(parts => new UserRequestRight
                {
                    UserId = userLogin,
                    RightId = int.Parse(parts[1])
                })
                .ToList();

            // Удаляем права пользователя из базы данных
            appContext.UserRequestRights.RemoveRange(userRequestRights);
            appContext.SaveChanges();

            // Логируем успешное удаление прав
            Logger.Debug("Все права пользователя были удалены.");
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            // Получаем все права пользователя по логину
            var userPermissions = appContext.UserRequestRights.Where(ur => ur.UserId == userLogin).Select(ur => ur.RightId.ToString()).ToList();

            if (userPermissions.Any())
            {
                Logger.Debug($"Все права пользователя - {userLogin} были получены.");
                return userPermissions;
            }
            else
            {
                Logger.Warn($"У пользользователя - {userLogin} нет прав");
                return userPermissions;
            }          
        }
    }
}