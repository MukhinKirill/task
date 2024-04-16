using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Task.Connector.DataBase;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    /// <summary>
    /// Реализованный интерфейс коннектора
    /// </summary>
    public class ConnectorDb : IConnector
    {
        #region PublicProperies
        public ILogger Logger
        {
            get
            {
                return DbContext.logger;
            }
            set
            {
                DbContext.logger = value;
            }
        }
        #endregion //PublicProperies

        #region IConnectorRealization
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">Строка настроек
        /// Настройки указываются в виде: настройка1='значение1';настройка2='значение2';...
        /// Настройки: Provider, ConnectionString, SchemaName</param>
        public void StartUp(string connectionString)
        {
            DbContext = new(connectionString);
        }

        
        public void CreateUser(UserToCreate user)
        {
            UserBuilder builder = new(DbContext);
            InvokeWithLogger(()=> builder.AddProperty("Login", user.Login));
            InvokeWithLogger(() => builder.AddPassword(user.HashPassword));

            UpdateUserProperties(user.Properties, builder);

            DbContext.User.Add(builder.Build());
            DbContext.SaveChanges();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return DbItemPropertyTools.GetAllProperties(typeof(User))
                .Select(p => new Property(p.Name, p.Description));
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = getUser(userLogin);
            if (user == null)
                return null;
            return DbItemPropertyTools.GetAllPropertiesOnlyObject(user)
                .Select(p => new UserProperty(p.Name, p.Value.ToString()??string.Empty));
        }

        public bool IsUserExists(string userLogin)
        {
            return DbContext.User.Any(i=>i.Login == userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = getUser(userLogin);
            if(user != null)
            {
                UserBuilder builder = new(DbContext, user);
                UpdateUserProperties(properties, builder);
                DbContext.SaveChanges();
            }
        }



        public IEnumerable<Permission> GetAllPermissions()
        {
            var itRoles = DbContext.ItRole
                .Select(i => new Permission(i.Id.ToString(), i.Name, "It role"))
                .ToList();
            return DbContext.RequestRight
                .Select(i => new Permission(i.Id.ToString(), i.Name, "Request right"))
                .ToList()
                .Union(itRoles);
        }

        /// <summary>
        /// Добавляет пользователю права и роли по id/ например ["Role:1", "Request:3"...]
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="rightIds">Права и роли в виде ["Role:1", "Request:3"...]</param>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = getUser(userLogin);
            if(user != null)
            {
                //Установка прав
                var rights = GetRightIds(rightIds);
                user.RequestRights = user.RequestRights
                    .Union(DbContext.RequestRight
                    .Where(i => rights.Contains(i.Id)))
                    .Distinct()
                    .ToList();

                //установка ролей
                var roles = GetRoleIds(rightIds);
                user.Roles = user.Roles
                    .Union(DbContext.ItRole
                    .Where(i => roles.Contains(i.Id)))
                    .Distinct()
                    .ToList();
                DbContext.SaveChanges();
                Logger.Debug("add rights: " + string.Join(", ", rightIds) + $" to {userLogin}");
            }
        }

        /// <summary>
        /// Удаляет у пользователя права и роли по id/ например ["Role:1", "Request:3"...]
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="rightIds">Права и роли в виде ["Role:1", "Request:3"...]</param>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = getUser(userLogin);
            if(user != null)
            {
                //Удаление прав
                var rights = GetRightIds(rightIds);
                user.RequestRights = user.RequestRights
                    .Where(i => !rights.Contains(i.Id))
                    .ToList();

                //Удаление ролей
                var roles = GetRoleIds(rightIds);
                user.Roles = user.Roles
                    .Where(i => !roles.Contains(i.Id))
                    .ToList();

                DbContext.SaveChanges();
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var  user = getUser(userLogin);
            return user?.RequestRights.Select(i => i.Name);
        }

        #endregion //IConnectorRealization

        #region HelperMethods

        /// <summary>
        /// Находит в массиве строк все права
        /// </summary>
        /// <param name="premissions"></param>
        /// <returns></returns>
        private static IEnumerable<int> GetRightIds(IEnumerable<string> premissions)
        {
            var rightRegex = new Regex(@"Request:(?<id>\d+)");
            return premissions.Select(i => rightRegex.Match(i)).Where(i => i.Success).Select(i => Convert.ToInt32(i.Groups["id"].Value));
        }
        /// <summary>
        /// Находит в массиве строк все роли
        /// </summary>
        /// <param name="premissions"></param>
        /// <returns></returns>
        private static IEnumerable<int> GetRoleIds(IEnumerable<string> premissions)
        {
            var roleRegex = new Regex(@"Role:(?<id>\d+)");
            return premissions.Select(i => roleRegex.Match(i)).Where(i => i.Success).Select(i => Convert.ToInt32(i.Groups["id"].Value));
        }

        /// <summary>
        /// Возвращает пользователя с подгруженными правами и ролями
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        private User? getUser(string login)
        {
            var user = DbContext.User
                .Include(i => i.RequestRights)
                .Include(i => i.Roles)
                .FirstOrDefault(i => i.Login == login);
            if (user == null)
                Logger?.Warn($"user with login {login} not found");
            return user;
        }

        /// <summary>
        /// Обновляет свойства у пользователя
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="builder"></param>
        private void UpdateUserProperties(IEnumerable<UserProperty> properties, UserBuilder builder)
        {
            foreach (var property in properties)
            {
                InvokeWithLogger(() =>
                {
                    switch (property.Name)
                    {
                        case "ItRole":
                            builder.AddItRole(property.Value);
                            break;
                        case "RequestRight":
                            builder.AddRequestRight(property.Value);
                            break;
                        default:
                            builder.AddProperty(property.Name, property.Value);
                            break;
                    }
                });
            }
            Logger.Debug("update properties: " + string.Join(", ", properties
                .Select(i => i.Name + "=" + i.Value)) + $" to {builder.Build().Login}");
        }
        #endregion //HelperMethods

        #region PrivateFields
        private Context? _dbContext;
        private Context DbContext
        {
            get 
            {
                if (_dbContext == null)
                    throw new NullReferenceException("База данных не инициализирована. Пожалуйста вызовите StartUp для инициализации");
                return _dbContext;
            }
            set 
            {
                _dbContext = value;
            }
        }

        private void InvokeWithLogger(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Logger?.Warn(e.Message);
            }
        }
        #endregion //PrivateFields
    }
}