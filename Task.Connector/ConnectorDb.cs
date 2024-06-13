using System;
using System.Collections.Generic;
using Task.Connector.Services;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        private IUserService _userService;
        private ISequrityService _sequrityService;
        private IPermissionService _permissionService;

        public void StartUp(string connectionString)
        {
            try
            {
                _userService = new UserService(connectionString);
                _sequrityService = new SequrityService(connectionString);
                _permissionService = new PermissionService(connectionString);
                Logger?.Debug("Сервисы успешно запущены.");
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось запустить сервисы: {e.Message}");
            }
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                if (!IsUserExists(user.Login))
                {
                    _userService.CreateUser(user);
                    _sequrityService.CreateSequrity(user.Login, user.HashPassword);
                    Logger?.Debug($"Пользователь {user.Login} успешно создан.");
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось создать пользователя {user.Login}: {e.Message}");
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                return _userService.IsUserExists(userLogin);
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось проверить существование пользователя {userLogin}: {e.Message}");
                return false;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var allProperties = _userService.GetAllProperties();
                return allProperties;
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось получить все свойства: {e.Message}");
                return new List<Property>();
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                if (IsUserExists(userLogin))
                {
                    var userProperties = _userService.GetUserProperties(userLogin);
                    return userProperties;
                }
                else
                {
                    return new List<UserProperty>();
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось получить свойства пользователя {userLogin}: {e.Message}");
                return new List<UserProperty>();
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                if (IsUserExists(userLogin))
                {
                    _userService.UpdateUserProperties(properties, userLogin);
                    Logger?.Debug($"Свойства пользователя {userLogin} успешно обновлены.");
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось обновить свойства пользователя {userLogin}: {e.Message}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                return _permissionService.GetAllPermissions();
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось получить все разрешения: {e.Message}");
                return new List<Permission>();
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                if (IsUserExists(userLogin))
                {
                    _permissionService.AddUserPermissions(userLogin, rightIds);
                    Logger?.Debug($"Разрешения для пользователя {userLogin} успешно добавлены.");
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось добавить разрешения для пользователя {userLogin}: {e.Message}");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                if (IsUserExists(userLogin))
                {
                    _permissionService.RemoveUserPermissions(userLogin, rightIds);
                    Logger?.Debug($"Разрешения для пользователя {userLogin} успешно удалены.");
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось удалить разрешения для пользователя {userLogin}: {e.Message}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                if (IsUserExists(userLogin))
                {
                    return _permissionService.GetUserPermissions(userLogin);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Logger?.Error($"Не удалось получить разрешения для пользователя {userLogin}: {e.Message}");
                return null;
            }
        }
    }
}
