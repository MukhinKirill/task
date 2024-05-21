using Task.Connector.Factories;
using Task.Connector.Interfaces;
using Task.Connector.Repositories;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        //Обычно использутся ILogger<T> из базового набора
        public required ILogger Logger { get; set; }

        private IUserRepository _userRepository;
        private IPermissionRepository _permissionRepository;

        //Обычно используют DI для получения зависимостей, поэтому предусмотрен такой варинат использования
        public ConnectorDb(IUserRepository userRepository = null, IPermissionRepository permissionRepository = null) {

            _userRepository = userRepository;
            _permissionRepository = permissionRepository;
        }

        //Нарушение принципа инверсии зависимостей, Connector не должен сам для себя инициализировать свою зависимость.
        //+ для инициализации лучше использовать ctor
        //Исправить ничего здесь не могу, тк нет доступа к библиотекам
        //Лучшим решением будет сделать класс регистратор, который будет запускаться в стартапе программы и регистрировать нужный dbContext в DI, а после прокидывать его в конструктор
        //Контекст в коннекторе не нужен, тк вся логика содержится в репозиториях
        public void StartUp(string connectionString)
        {
            Logger.Debug("Started Connector setup");
            try
            {
                if (_userRepository == null || _permissionRepository == null)
                {
                    var contextFactory = new ContextFactoryFacade();
                    var context = contextFactory.GetContext(connectionString);
                    _userRepository = new UserRepository(Logger, context);
                    _permissionRepository = new PermissionRepository(Logger, context);
                }
                else
                {
                    Logger.Warn("The necessary data has already been received through the constructor. Don't use this method!");
                }
            }
            catch (ArgumentException e)
            {
                Logger.Error(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }

            Logger.Debug("Connector setuped");
        }

        #region User
        public void CreateUser(UserToCreate user)
        {
            Logger.Debug("Adding new user");
            CheckStartUp();
            try
            {
                _userRepository.CreateUser(user);
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e.Message);
                throw;
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"Unable to create user! {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }
        }
        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Getting a list of all user properties");
            CheckStartUp();
            try
            {
                return _userRepository.GetAllProperties();
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }
        }
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug($"Getting a list of user properties with login {userLogin}");
            CheckStartUp();
            try
            {
                return _userRepository.GetUserProperties(userLogin);
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }
        }
        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Checking the existence of a user with a login {userLogin}");
            CheckStartUp();
            try
            {
                return _userRepository.IsUserExists(userLogin);
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }
        }
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug($"Updating the user properties with login {userLogin}");
            CheckStartUp();
            try
            {
                _userRepository.UpdateUserProperties(properties, userLogin);
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }

        }
        #endregion

        #region Permission
        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug($"Getting a permissions list");
            CheckStartUp();
            try
            {
                return _permissionRepository.GetAllPermissions();
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Adding permissions to user with a login {userLogin}");
            CheckStartUp();
            try
            {
                if (!_userRepository.IsUserExists(userLogin))
                    throw new ArgumentException($"The user with login {userLogin} does not exist!");
                _permissionRepository.AddUserPermissions(userLogin, rightIds);
            }
            catch (ArgumentException e)
            {
                Logger.Error(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Removing user permissions with a login {userLogin}");
            CheckStartUp();
            try
            {
                if (!_userRepository.IsUserExists(userLogin))
                    throw new ArgumentException($"The user with login {userLogin} does not exist!");

                _permissionRepository.RemoveUserPermissions(userLogin, rightIds);
            }
            catch (ArgumentException e)
            {
                Logger.Error(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Getting user permissions with a login {userLogin}");
            CheckStartUp();
            try
            {
                if (!_userRepository.IsUserExists(userLogin))
                    throw new ArgumentException($"The user with login {userLogin} does not exist!");
                return _permissionRepository.GetUserPermissions(userLogin);
            }
            catch (ArgumentException e)
            {
                Logger.Error(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown exception! {e.Message}");
                throw;
            }
        }

        #endregion

        private void CheckStartUp()
        {
            if(_userRepository == null || _permissionRepository == null)
            {
                var error = "The connector has not been initialized! Use a constructor or method StartUp!";
                Logger.Error(error);
                throw new NullReferenceException(error);
            }
        }
    }
}