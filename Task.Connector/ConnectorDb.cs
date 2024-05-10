using Task.Connector.Domain.Models;
using Task.Connector.Infrastructure.Context;
using Task.Connector.Infrastructure.DataModels;
using Task.Connector.Infrastructure.Repository;
using Task.Connector.Infrastructure.Repository.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    /// <summary>
    /// Коннектор.
    /// </summary>
    public class ConnectorDb : IConnector
    {
        private AvanpostContext _context;
        private IUserRepository _userRepository;
        private IPermissionRepository _permissionRepository;
        public ILogger Logger { get; set; }
        
        /// <summary>
        /// Конфигурация коннектора через строку подключения.
        /// </summary>
        /// <param name="connectionString">Строка подключения к БД.</param>
        public void StartUp(string connectionString)
        {
            var contextFactory = new AvanpostContextFactory(connectionString, Logger);
            _context = contextFactory.GetContext("POSTGRE");
            _userRepository = new UserRepository(_context, Logger);
            _permissionRepository = new PermissionRepository(_context, Logger);
            
            Logger.Debug("Connector created!");
        }

        /// <summary>
        /// Создать пользователя с набором свойств по умолчанию.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        public void CreateUser(UserToCreate user)
        {
            try
            {
                var userDataModel = new UserDataModel(user);
                
                var isLeadString = userDataModel.IsLead ? "lead" : "not lead";
                Logger.Debug($"Creating {isLeadString}. Properties: " +
                             $"login - '{userDataModel.Login}'; " +
                             $"firstName - '{userDataModel.FirstName}'; " +
                             $"middleName - '{userDataModel.MiddleName}'; " +
                             $"lastName - '{userDataModel.LastName}'; " +
                             $"telephoneNumber - '{userDataModel.TelephoneNumber}'");

                _userRepository.Create(userDataModel);

                Logger.Debug($"Created user {userDataModel.Login}");
            }
            catch (Exception e)
            {
                Logger.Error($"User creating error {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Проверка существования пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        public bool IsUserExists(string userLogin)
        {
            try
            {
                var isExists = _userRepository.IsExists(userLogin);
                Logger.Debug($"Check user {userLogin}. Result {isExists}");
                return isExists;
            }
            catch (Exception e)
            {
                Logger.Error($"User check error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Метод позволяет получить все свойства пользователя.
        /// </summary>
        /// <returns>Свойства пользователя IEnumerable&lt;Property&gt;.</returns>
        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                Logger.Debug("Get all properties");
                var properties = UserDataModel.GetProperties();
                return properties;
            }
            catch (Exception e)
            {
                Logger.Error($"Get all properties error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получить все значения свойств пользователя.
        /// </summary>
        /// <param name="userLogin">Логин пользователя.</param>
        /// <returns>Свойства пользователя IEnumerable&lt;UserProperty&gt;.</returns>
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                Logger.Debug("Get user properties");

                var user = _userRepository.GetUserModelByLogin(userLogin);
                if (user == null) throw new Exception("User not found");
                
                var properties = user.GetUserProperties();
                return properties;
            }
            catch (Exception e)
            {
                Logger.Error($"Get user properties error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Метод позволяет устанавливать значения свойств пользователя.
        /// </summary>
        /// <param name="properties">Свойства пользователя.</param>
        /// <param name="userLogin">Логин пользователя.</param>
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                Logger.Debug("Update user properties");

                var user = _userRepository.GetUserModelByLogin(userLogin);
                if (user == null) throw new Exception("User not found");

                foreach (var property in properties)
                {
                    var test = typeof(UserDataModel).GetProperty(property.Name);
                    if (test == null) throw new Exception("User property not found");
                    
                    test.SetValue(user, property.Value);
                }
                
                _userRepository.Update(user);
                
            }
            catch (Exception e)
            {
                Logger.Error($"Update user properties error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        ///  Получить все права в системе.
        /// </summary>
        /// <returns>Права пользователя IEnumerable&lt;Permission&gt;.</returns>
        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                Logger.Debug("Get all permissions");
                return _permissionRepository.GetAllPermissions();
            }
            catch (Exception e)
            {
                Logger.Error($"Get all permissions error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Добавить права пользователю в системе.
        /// </summary>
        /// <param name="rightIds">Права пользователя.</param>
        /// <param name="userLogin">Логин пользователя.</param>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger.Debug("Starting adding user permissions");

                var isExists = _userRepository.IsExists(userLogin);
                if (!isExists) throw new Exception("User not found");
                
                var roles = new List<UserItRole>();
                var requests = new List<UserRequestRight>();

                GetCollections(userLogin, rightIds, roles, requests);
                
                _permissionRepository.AddRequestPermissions(requests);
                _permissionRepository.AddRolePermissions(roles);
                    
                Logger.Debug("End adding user permissions");
            }
            catch (Exception e)
            {
                Logger.Error($"Adding user permissions error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Удалить права пользователю в системе.
        /// </summary>
        /// <param name="rightIds">Права пользователя.</param>
        /// <param name="userLogin">Логин пользователя.</param>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger.Debug("Starting removing user permissions");

                var isExists = _userRepository.IsExists(userLogin);
                if (!isExists) throw new Exception("User not found");
                
                var roles = new List<UserItRole>();
                var requests = new List<UserRequestRight>();
                
                GetCollections(userLogin, rightIds, roles, requests);
                
                _permissionRepository.RemoveRequestPermissions(requests);
                _permissionRepository.RemoveRolePermissions(roles);
                    
                Logger.Debug("End removing user permissions");
            }
            catch (Exception e)
            {
                Logger.Error($"Removing user permissions error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получить права пользователя в системе.
        /// </summary>
        /// <param name="userLogin">Логин пользователя.</param>
        /// <returns>Права пользователя IEnumerable&lt;string&gt;.</returns>
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                Logger.Debug("Get All User Permissions");
                var permissions = _permissionRepository.GetUserPermissions(userLogin);
                
                return permissions.Select(p => $"{p.Name}:{p.Id}");
            }
            catch (Exception e)
            {
                Logger.Error($"Get All User Permissions error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получить роли и утверждения.
        /// </summary>
        /// <param name="userLogin">Логин пользователя.</param>
        /// <param name="rightIds">Права пользователя.</param>
        /// <param name="roles">Роли пользователя.</param>
        /// <param name="requests">Утверждения пользователя.</param>
        private void GetCollections(string userLogin, IEnumerable<string> rightIds, ICollection<UserItRole> roles, ICollection<UserRequestRight> requests)
        {
            foreach (var rightId in rightIds)
            {
                var values = rightId.Split(':', 2);

                switch (values[0])
                {
                    case "Role":
                        roles.Add(new UserItRole()
                        {
                            UserId = userLogin,
                            RoleId = int.Parse(values[1])
                        });
                        break;

                    case "Request":
                        requests.Add(new UserRequestRight()
                        {
                            UserId = userLogin,
                            RightId = int.Parse(values[1])
                        });
                        break;
                }
            }
        }
    }
}