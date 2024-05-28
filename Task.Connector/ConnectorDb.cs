using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security;
using System.Text.RegularExpressions;
using Task.Connector.ModelsDb;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        MyContext myContext;
        public void StartUp(string connectionString)
        {
            var matchs = Regex.Match(connectionString, @"ConnectionString='(?<ConnectionString>.*)';Provider='(?<Provider>.*)';SchemaName='(?<SchemaName>.*)'");

            var connectionStr = matchs.Groups["ConnectionString"];
            var provider = matchs.Groups["Provider"];
            var schemaName = matchs.Groups["SchemaName"];

            myContext = GetContext(provider.ToString(), connectionStr.ToString(), schemaName.ToString());
        }  

        // Возвращает контекст БД
        public MyContext GetContext(string providerName, string connectionString, string schemaName)
        {
            DbContextOptionsBuilder<MyContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MyContext>();
            if (providerName.Contains("Postgre"))
            {
                dbContextOptionsBuilder.UseNpgsql(connectionString);
                return new MyContext(dbContextOptionsBuilder.Options, schemaName);
            }
            if (providerName.Contains("SqlServer"))
            {
                dbContextOptionsBuilder.UseSqlServer(connectionString);
                return new MyContext(dbContextOptionsBuilder.Options, schemaName);
            }

            throw new Exception("Неопределенный провайдер - " + providerName);
        }

        // Создать пользователя с набором свойств по умолчанию
        public void CreateUser(UserToCreate user)
        {
            if (user is null)
            {
                Logger.Warn("Пользователь не создан");
                return;
            }

            bool.TryParse(user.Properties.FirstOrDefault(x => x.Name == nameof(User.IsLead))?.Value, out var isLead);
            User user1 = new User
            {
                Login = user.Login,
                LastName = user.Properties.FirstOrDefault(x => x.Name == nameof(User.LastName))?.Value ?? "",
                FirstName = user.Properties.FirstOrDefault(x => x.Name == nameof(User.FirstName))?.Value ?? "",
                MiddleName = user.Properties.FirstOrDefault(x => x.Name == nameof(User.MiddleName))?.Value ?? "",
                TelephoneNumber = user.Properties.FirstOrDefault(x => x.Name == nameof(User.TelephoneNumber))?.Value ?? "",
                IsLead = isLead
            };

            myContext.Users.Add(user1);
            myContext.SaveChanges();
            Logger.Debug("Пользователь создан успешно");
        }

        // Метод позволяет получить все свойства пользователя(смотри Описание системы), пароль тоже считать свойством
        public IEnumerable<Property> GetAllProperties()
        {
            var properties = new List<Property>()
            {
                new Property(nameof(User.FirstName), "Имя"),
                new Property(nameof(User.LastName), "Фамилия"),
                new Property(nameof(User.MiddleName), "Отчество"),
                new Property(nameof(User.TelephoneNumber), "Номер телефона"),
                new Property(nameof(User.IsLead), "Руководитель"),
                new Property(nameof(UserPassword.Password), "Пароль")
            };

            return properties;
        }

        // Получить все значения свойств пользователя
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = myContext.Users.FirstOrDefault(x => x.Login == userLogin);

            if (user is null)
            {
                Logger.Warn("Пользователь не найден");
                return new List<UserProperty>(); 
            }

            var userProperties = new List<UserProperty>()
            {
                new UserProperty(nameof(User.FirstName), user.FirstName),
                new UserProperty(nameof(User.LastName), user.LastName),
                new UserProperty(nameof(User.MiddleName), user.MiddleName),
                new UserProperty(nameof(User.TelephoneNumber), user.TelephoneNumber),
                new UserProperty(nameof(User.IsLead), user.IsLead.ToString())
            };

            Logger.Debug("Получены все значения свойств пользователя");
            return userProperties;
        }

        // Проверка существования пользователя
        public bool IsUserExists(string userLogin)
        {         
           return myContext.Users.Any(x => x.Login == userLogin);
        }

        // Метод позволяет устанавливать значения свойств пользователя
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {           
            var user = myContext.Users.FirstOrDefault(x => x.Login == userLogin);

            if (user is null)
            {
                Logger.Warn("Пользователь не найден");
                return;
            }

            foreach (var property in properties) 
            {
                if (property.Name == nameof(user.FirstName)) 
                    user.FirstName = property.Value;
                if (property.Name == nameof(user.LastName)) 
                    user.LastName = property.Value;
                if (property.Name == nameof(user.MiddleName)) 
                    user.MiddleName = property.Value;
                if (property.Name == nameof(user.TelephoneNumber))
                    user.TelephoneNumber = property.Value;
                if (property.Name == nameof(user.IsLead))
                {
                    if (bool.TryParse(property.Value, out var isLead))
                        user.IsLead = isLead; 
                }
            }

            myContext.Users.Update(user);
            myContext.SaveChanges();
            Logger.Debug("Значения свойств пользователя установлены успешно");
        }

        // Получить все права в системе 
        public IEnumerable<Permission> GetAllPermissions()
        {
            
            var roles = myContext.ItRoles.ToList();
            var rights = myContext.RequestRights.ToList();

            var permissions = new List<Permission>();

            foreach (var role in roles)
            {
                permissions.Add(new Permission(role.Id.ToString(), role.Name, "ItRoles"));
            }

            foreach (var right in rights)
            {
                permissions.Add(new Permission(right.Id.ToString(), right.Name, "RequestRights"));
            }

            return permissions;
        }

        // Добавить права пользователю в системе
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin)) 
            {
                Logger.Warn("Пользователь не найден");
                return;
            }

            foreach (var right in rightIds) 
            {
                var rightData = RightPars(right);

                string rightTip = rightData[0];
                int id = Convert.ToInt32(rightData[1]);

                if (rightTip.Equals("Role"))
                {
                    UserItrole userItrole = new UserItrole
                    { UserId = userLogin, RoleId = id };
                  
                    myContext.UserItroles.Add(userItrole);
                    myContext.SaveChanges();
                    Logger.Debug("Права пользователю добавлены успешно");
                }
                else if (rightTip.Equals("Request"))
                {
                    UserRequestRight userRequestRight = new UserRequestRight
                    { UserId = userLogin, RightId = id };

                    myContext.UserRequestRights.Add(userRequestRight);
                    myContext.SaveChanges();
                    Logger.Debug("Права пользователю добавлены успешно");
                }
                else 
                {
                    Logger.Warn("Ошибка в типе прав добовляемых пользователю");
                }
            }
        }

        // Удалить права пользователю в системе
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Warn("Пользователь не найден");
                return;
            }

            foreach (var right in rightIds) 
            {
                var rightData = RightPars(right);

                string rightTip = rightData[0];
                int id = Convert.ToInt32(rightData[1]);
                
                if (rightTip.Equals("Role"))
                {
                    var userRole = myContext.UserItroles.FirstOrDefault(x => x.UserId == userLogin && x.RoleId == id);
                    if (userRole is null)
                    { 
                        Logger.Warn("Не найдена роль у пользователя");
                        return; 
                    }

                    myContext.UserItroles.Remove(userRole);
                    myContext.SaveChanges();
                    Logger.Debug("Права пользователя удалены успешно");
                }
                else if (rightTip.Equals("Request"))
                {
                    var userRequestRight = myContext.UserRequestRights.FirstOrDefault(x => x.UserId == userLogin && x.RightId == id);
                    if (userRequestRight is null)
                    {
                        Logger.Warn("Не найдена роль у пользователя");
                        return;
                    }
                    myContext.UserRequestRights.Remove(userRequestRight);
                    myContext.SaveChanges();
                    Logger.Debug("Права пользователя удалены успешно");
                }
                else
                {
                    Logger.Warn("Ошибка в типе прав удаляемых у пользователя");
                }
            }
        }

        // Получить права пользователя в системе
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Warn("Пользователь не найден");
                return Enumerable.Empty<string>();
            }

            var userRequestRightIds = myContext.UserRequestRights
                .Where(x => x.UserId == userLogin)
                .Select(x => x.RightId)
                .ToList();

            var userPermissions = myContext.RequestRights
                .Where(x => userRequestRightIds.Contains(x.Id))
                .Select(x => x.Name)
                .ToList();

            Logger.Debug("Права пользователя получены успешно");
            return userPermissions;
        }

        /// <summary>
        /// Проверка на коррктоность переданной строки и разделение ее на имя и id
        /// </summary>
        /// <param name="rightIds"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string[] RightPars(string rightIds) 
        {
            string[] rightIdsArray = rightIds.Split(":");

            if (rightIdsArray.Length != 2 || !int.TryParse(rightIdsArray[1], out int rightIdValue))                 
                throw new ArgumentException("Передана некорректная строка");
            else
                return rightIdsArray;
        }

        public ILogger Logger { get; set; }
    }
}