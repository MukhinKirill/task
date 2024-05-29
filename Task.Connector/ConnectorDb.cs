using System.Text.RegularExpressions;
using Task.Connector.DataBase;
using Task.Connector.DataBase.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        
        private DataContext _dataContext;
        private UserRepository _userRepository;
        private PermissionsRepository _permissionsRepository;
        
        private const string RequestRightGroupName = "Request";
        private const string ItRoleRightGroupName = "Role";
        private const string RightSeparator = ":";
        
        public void StartUp(string fullConnectionString)
        {
            var connectionString = ParseConnectionString(fullConnectionString, out var provider);
            
            _dataContext = new DataContext(connectionString, provider);
            _userRepository = new UserRepository(_dataContext);
            _permissionsRepository = new PermissionsRepository(_dataContext);
        }
        
        /// <summary>
        /// Распарсить строку подключения
        /// </summary>
        private string ParseConnectionString(string connectionString, out string provider)
        {
            var pattern =  @"(\w+)='([^']*)'";
            var matches = Regex.Matches(connectionString, pattern);

            var dictionary = matches.ToDictionary(x => x.Groups[1].Value, v => v.Groups[2].Value);

            provider = dictionary["Provider"].Contains("Postgre") ? "POSTGRE" : "MSSQL";
        
            return dictionary["ConnectionString"];
        }
        
        /// <summary>
        /// Создать пользователя с набором свойств по умолчанию.
        /// </summary>
        public void CreateUser(UserToCreate userToCreate)
        {
            Logger.Debug($"Создание пользователя {userToCreate.Login}.");
            
            try
            {
                if (_userRepository.IsUserExists(userToCreate.Login))
                {
                    Logger.Warn("Не удаось создать пользователя: Пользователь с таким именем уже существует");
                    return;
                }

                var user = new User(userToCreate.Login, userToCreate.Properties);
                var password = new Security(userToCreate.Login, userToCreate.HashPassword);
                
                _userRepository.CreateUser(user, password);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Получить все свойства пользователя.
        /// </summary>
        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug($"Получение всех свойств пользователя.");

            try
            {
                return _userRepository.GetAllProperties().Select(x => new Property(x, ""));
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Получить все значения свойств пользователя.
        /// </summary>
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug($"Получение значения всех свойств пользователя {userLogin}.");
            
            try
            {
                return _userRepository.GetUserProperties(userLogin);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Проверить существования пользователя
        /// </summary>
        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Проверка существования пользователя {userLogin}.");
            
            try
            {
                return _userRepository.IsUserExists(userLogin);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Установить значения свойств пользователя
        /// </summary>
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug($"Установка значения свойств пользователя {userLogin}.");
            
            try
            {
                if (!_userRepository.IsUserExists(userLogin))
                {
                    Logger.Warn("Не удаось обновить пользователя: Пользователь с таким именем уже существует");
                    return;
                }
                
                var userProperties = new Dictionary<string, object>();
                
                foreach (var property in properties)
                {
                    var lowerInvariant = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                    
                    if (lowerInvariant == "isLead")
                    {
                        userProperties.Add(lowerInvariant, Convert.ToBoolean(property.Value));
                    }
                    
                    userProperties.Add(lowerInvariant, property.Value);
                }
                
                _userRepository.UpdateUser(userLogin, userProperties);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Получить все права в системе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug($"Получить все права в системе");
            
            try
            {
                return _permissionsRepository.GetAllPermissions();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Добавить права пользователю в системе
        /// </summary>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Добавить права пользователю в системе {userLogin}.");
            
            try
            {
                if (!_userRepository.IsUserExists(userLogin))
                {
                    Logger.Warn("Не удаось обновить права пользователя: Пользователь с таким именем уже существует");
                    return;
                }
                
                var roles = ParseRights(rightIds, out var requestRights);

                _permissionsRepository.AddUserRoles(userLogin.Trim('\''), roles);
                _permissionsRepository.AddUserRequestRights(userLogin.Trim('\''), requestRights);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Удалить права пользователю в системе
        /// </summary>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Удалить права пользователю в системе {userLogin}.");
            
            try
            {
                if (!_userRepository.IsUserExists(userLogin))
                {
                    Logger.Warn("Не удаось обновить права пользователя: Пользователь с таким именем уже существует");
                    return;
                }
                
                var roles = ParseRights(rightIds, out var requestRights);

                _permissionsRepository.RemoveUserRoles(userLogin.Trim('\''), roles);
                _permissionsRepository.RemoveUserRequestRights(userLogin.Trim('\''), requestRights);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Распрасить права
        /// </summary>
        private static IEnumerable<int> ParseRights(IEnumerable<string> rightIds, out List<int> requestRights)
        {
            var roles = new List<int>();
            requestRights = new List<int>();
                
            foreach (var right in rightIds)
            {
                var s = right.Split(RightSeparator);
                switch (s[0])
                {
                    case RequestRightGroupName:
                        requestRights.Add(int.Parse(s[1]));
                        break;
                        
                    case ItRoleRightGroupName:
                        roles.Add(int.Parse(s[1]));
                        break;
                }
            }

            return roles;
        }

        /// <summary>
        /// Получить права пользователя в системе
        /// </summary>
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Получить права пользователя в системе {userLogin}.");
            
            try
            {
                return _permissionsRepository.GetUserPermissions(userLogin.Trim('\''))
                    .Select(x => string.Format(x, RequestRightGroupName, RightSeparator, ItRoleRightGroupName));
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                throw;
            }
        }
    }
}