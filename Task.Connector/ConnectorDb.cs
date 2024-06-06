using Task.Connector.Models;
using Task.Connector.Repositories;
using Task.Connector.DataHandlers;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Repositories.Factory;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private UserHandler userHandler;
        private PropertyHandler propertyHandler;
        private PermissionHandler permissionHandler;

        private IRepository repository;

        public ILogger Logger { get; set; }

        public ConnectorDb()
        {
            userHandler = new UserHandler();
            propertyHandler = new PropertyHandler();
            permissionHandler = new PermissionHandler();
        }

        public void StartUp(string connectionString)
        {
            repository = RepositoryFactory.CreateRepositoryFrom(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                CheckRepository();
                var data = userHandler.GetDataUser(user);
                repository.AddUser(data.usr, data.pass);
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
                CheckRepository();
                Logger?.Debug($"Получаем все Property...");
                var properties = new List<Property>();
                properties.AddRange(propertyHandler.GetAttributesFromType(typeof(User)));
                properties.AddRange(propertyHandler.GetAttributesFromType(typeof(Password)));
                Logger?.Debug($"Получили все Property.");
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
                CheckRepository();
                var user = repository.GetUserFromLogin(userLogin);
                Logger?.Debug($"Пользователь {userLogin} получен.");
                var properties = userHandler.GetUserPropertiesFromUser(user);
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
                CheckRepository();
                Logger?.Debug($"Проверяем наличие пользователя {userLogin}.");
                return repository.IsUserExists(userLogin);
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
                CheckRepository();
                var user = repository.GetUserFromLogin(userLogin);
                Logger?.Debug($"Пользователь {userLogin} получен.");
                var userProps = userHandler.GetUserPropertiesFromUser(user);
                Logger?.Debug($"UserProperty для {userLogin} получены.");
                foreach (var prop in properties)
                {
                    foreach (var userProp in userProps)
                    {
                        if (prop.Name.Equals(userProp.Name)) userProp.Value = prop.Value;
                    }
                }
                userHandler.SetUserProperties(user, userProps);
                Logger?.Debug($"UserProperty изменены для пользователя {userLogin}.");
                repository.UpdateUser(user);
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
                CheckRepository();
                var roles = repository.GetAllItRoles();
                Logger?.Debug($"Успешно получили все роли.");
                var rights = repository.GetAllItRequestRights();
                Logger?.Debug($"Успешно получили все права.");
                return permissionHandler.GetAllPermissionFrom(roles, rights);

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
                CheckRepository();
                var data = permissionHandler.SortPermissonsToData(userLogin, rightIds);
                Logger?.Debug($"Успешно отсортировали все разрешения.");
                if (data.userItRole != null && data.userItRole.Count != 0) repository.AddRolesToUser(userLogin, data.userItRole);
                if (data.userRequestRights != null && data.userRequestRights.Count != 0) repository.AddRequestRightsToUser(userLogin, data.userRequestRights);
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
                CheckRepository();
                var data = permissionHandler.SortPermissonsToData(userLogin, rightIds);
                Logger?.Debug($"Успешно отсортировали все разрешения.");
                if (data.userItRole != null && data.userItRole.Count != 0) repository.RemoveRolesToUser(userLogin, data.userItRole);
                if (data.userRequestRights != null && data.userRequestRights.Count != 0) repository.RemoveRequestRightsToUser(userLogin, data.userRequestRights);
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
                CheckRepository();
                var roles = repository.GetItRolesFromUser(userLogin);
                Logger?.Debug($"Успешно получили роли для пользователя {userLogin}.");
                var rights = repository.GetItRequestRightsFromUser(userLogin);
                Logger?.Debug($"Успешно получили права для пользователя {userLogin}.");
                var permissions = permissionHandler.GetAllPermissionFrom(roles, rights);
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

        private void CheckRepository()
        {
            if (repository == null) 
                throw new NullReferenceException("Перед этой операцией, необходимо вызвать метод StartUp и указать строку подключения.");
        }

    }
}
