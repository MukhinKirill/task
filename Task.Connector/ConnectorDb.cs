using Task.Connector.Models;
using Task.Connector.Repositories;
using Task.Connector.Converters;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Repositories.Factory;
using System.Data.Common;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private UserConverter userConverter;
        private PropertyAttrConverter propConverter;
        private PermissionConverter permissionConverter;

        private IStorage storage;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            storage = RepositoryFactory.CreateRepositoryFrom(connectionString);
            userConverter = new UserConverter();
            propConverter = new PropertyAttrConverter();
            permissionConverter = new PermissionConverter();
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                var data = userConverter.GetDataUser(user);
                storage.AddUser(data.usr, data.pass);
                Logger?.Debug($"Пользователь {user.Login} успешно добавлен.");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при добавлении пользвателя: {user.Login}.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var properties = new List<Property>();
                properties.AddRange(propConverter.GetAttributesFromType(typeof(User)));
                properties.AddRange(propConverter.GetAttributesFromType(typeof(Password)));
                Logger?.Debug($"Получаем все Property.");
                return properties;
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при получении всех Property.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = storage.GetUserFromLogin(userLogin);
                Logger?.Debug($"Пользователь {userLogin} получен.");
                var properties = userConverter.GetUserPropertiesFromUser(user);
                Logger?.Debug($"Property успешно получены.");
                return properties;
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при получении Property у пользователя.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                Logger?.Debug($"Проверяем наличие пользователя {userLogin}.");
                return storage.IsUserExists(userLogin);
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при проверки наличия пользвателя: {userLogin}.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = storage.GetUserFromLogin(userLogin);
                Logger?.Debug($"Пользователь {userLogin} получен.");
                var userProps = userConverter.GetUserPropertiesFromUser(user);
                Logger?.Debug($"UserProperty для {userLogin} получены.");
                foreach (var prop in properties)
                {
                    foreach (var userProp in userProps)
                    {
                        if (prop.Name.Equals(userProp.Name)) userProp.Value = prop.Value;
                    }
                }
                userConverter.SetUserProperties(user, userProps);
                Logger?.Debug($"UserProperty изменены для пользователя {userLogin}.");
                storage.UpdateUser(user);
                Logger?.Debug($"Успешно сохранили UserProperty для пользователя {userLogin}.");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при изменении UserProperties для пользователя: {userLogin}.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var roles = storage.GetAllItRoles();
                Logger?.Debug($"Успешно получили все роли.");
                var rights = storage.GetAllItRequestRights();
                Logger?.Debug($"Успешно получили все права.");
                return permissionConverter.GetAllPermissionFrom(roles, rights);

            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при получении всех разрещений.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var data = permissionConverter.SortPermissonsToData(userLogin, rightIds);
                Logger?.Debug($"Успешно отсортировали все разрешения.");
                if (data.userItRole != null && data.userItRole.Count != 0) storage.AddRolesToUser(userLogin, data.userItRole);
                if (data.userRequestRights != null && data.userRequestRights.Count != 0) storage.AddRequestRightsToUser(userLogin, data.userRequestRights);
                Logger?.Debug($"Успешно добавили в базу данных разрешения.");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при добавлении разрешения для пользвателя: {userLogin}.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var data = permissionConverter.SortPermissonsToData(userLogin, rightIds);
                Logger?.Debug($"Успешно отсортировали все разрешения.");
                if (data.userItRole != null && data.userItRole.Count != 0) storage.RemoveRolesToUser(userLogin, data.userItRole);
                if (data.userRequestRights != null && data.userRequestRights.Count != 0) storage.RemoveRequestRightsToUser(userLogin, data.userRequestRights);
                Logger?.Debug($"Успешно удалил из базу данных разрешения.");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при удалении разрешения для пользвателя: {userLogin}.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                var roles = storage.GetItRolesFromUser(userLogin);
                Logger?.Debug($"Успешно получили роли для пользователя {userLogin}.");
                var rights = storage.GetItRequestRightsFromUser(userLogin);
                Logger?.Debug($"Успешно получили права для пользователя {userLogin}.");
                var permissions = permissionConverter.GetAllPermissionFrom(roles, rights);
                var strings = new List<string>();
                foreach (var perm in permissions) strings.Add(perm.Name);
                Logger?.Debug($"Успешно сформировали список всех разрешений для пользователя {userLogin}.");
                return strings;
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при получении разрешений у пользвателя: {userLogin}.");
                Logger?.Error(ex.Message);
                throw;
            }
        }

    }
}
