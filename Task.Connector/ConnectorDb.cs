using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Task.Connector.Utils;
using System.Security.Permissions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Task.Connector.Managers;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DbContextFactory dbContextFactory;
        private DataContext dbContext;
        private UserManager userManager;
        private PropertyManager propertyManager;
        private PermissionManager permissionManager;

        public void StartUp(string connectionString)
        {
            try
            {
                ConnectionParser parser = new ConnectionParser();

                string providerName = parser.GetConnectionProvaider(connectionString);
                connectionString = parser.DBConnectionString(connectionString);

                dbContextFactory = new DbContextFactory(connectionString);
                dbContext = dbContextFactory.GetContext(providerName);

                userManager = new UserManager(dbContext, Logger);
                propertyManager = new PropertyManager(dbContext, Logger);
                permissionManager = new PermissionManager(dbContext, Logger);

                Logger?.Debug("Соединение с базой данных установленно успешно!");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при установке соединения: {ex.Message}!");
                throw;
            }
        }

        public void CreateUser(UserToCreate user)
        {
            Logger?.Debug("Создание нового пользователя.");

            if (IsUserExists(user.Login))
            {
                Logger?.Error($"Пользователь с логином '{user.Login}' уже существует!");
                throw new Exception($"Пользователь с логином '{user.Login}' уже существует!");
            }

            userManager.CreateUser(user);
        }

        public bool IsUserExists(string userLogin)
        {
            return userManager.IsUserExists(userLogin);
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return propertyManager.GetAllProperties();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger?.Debug("Получение всех свойств пользователя.");

            if (!IsUserExists(userLogin))
            {
                Logger?.Error($"Пользователь с логином '{userLogin}' не существует!");
                throw new Exception($"Пользователь с логином '{userLogin}' не существует!");
            }

            return propertyManager.GetUserProperties(userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger?.Debug("Обновление свойств пользователя.");

            if (!IsUserExists(userLogin))
            {
                Logger?.Error($"Пользователь с логином '{userLogin}' не существует!");
                throw new Exception($"Пользователь с логином '{userLogin}' не существует!");
            }

            propertyManager.UpdateUserProperties(properties, userLogin);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger?.Debug("Получение всех прав в системе.");

            return permissionManager.GetAllPermissions();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger?.Debug("Добавление прав пользователю.");

            if (!IsUserExists(userLogin))
            {
                Logger?.Error($"Пользователь с логином '{userLogin}' не существует!");
                throw new Exception($"Пользователь с логином '{userLogin}' не существует!");
            }

            permissionManager.AddUserPermissions(userLogin, rightIds);
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger?.Debug("Удаление прав пользователя.");

            if (!IsUserExists(userLogin))
            {
                Logger?.Error($"Пользователь с логином '{userLogin}' не существует!");
                throw new Exception($"Пользователь с логином '{userLogin}' не существует!");
            }

            permissionManager.RemoveUserPermissions(userLogin, rightIds);
        }


        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger?.Debug("Получение прав пользователя.");

            if (!IsUserExists(userLogin))
            {
                Logger?.Error($"Пользователь с логином '{userLogin}' не существует!");
                throw new Exception($"Пользователь с логином '{userLogin}' не существует!");
            }

            return permissionManager.GetUserPermissions(userLogin);
        }

        public ILogger Logger { get; set; }
    }
}