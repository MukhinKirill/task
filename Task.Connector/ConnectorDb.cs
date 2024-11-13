using System.ComponentModel;
using System.Reflection;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }

        private ApplicationContext _context;

        
        /// <summary>
        /// Конфигурация коннектора через строку подключения 
        /// </summary>
        /// <param name="connectionString"></param>
        public void StartUp(string connectionString)
        {
            try
            {
                _context = new ApplicationContext(connectionString);

            }
            catch (Exception ex)
            {

            }
            
        }

        /// <summary>
        /// Создать пользователя с набором свойств по умолчанию
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateUser(UserToCreate user)
        {
            var newUser = new User
            {
                Login = user.Login,
                LastName = user.Properties.FirstOrDefault(p => p.Name == "Винокуров")?.Value ?? string.Empty,
                FirstName = user.Properties.FirstOrDefault(p => p.Name == "Роман")?.Value ?? string.Empty,
                MiddleName = user.Properties.FirstOrDefault(p => p.Name == "Владимирович")?.Value ?? string.Empty,
                TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "89224537467")?.Value ?? string.Empty,
                IsLead = bool.TryParse(user.Properties?.FirstOrDefault(p => p.Name == "isLead")?.Value, out var parsedIsLead) && parsedIsLead
            };

            _context.User.Add(newUser);
            _context.SaveChanges();

            Logger.Debug("The user has been successfully created");
        }

        /// <summary>
        /// Метод позволяет получить все свойства пользователя 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<Property> GetAllProperties()
        {
            var user = _context.User.FirstOrDefault();

            if (user == null)
            {
                return Enumerable.Empty<Property>();
            }

            var userProperties = user.GetType().GetProperties();

            var properties = userProperties
                .Select(property => new Property(property.Name, property.GetCustomAttribute<DescriptionAttribute>()?.Description))
                .ToList();

            Logger.Debug("The all properties have been successfully obtained.");

            return properties;
        }

        /// <summary>
        /// Получить все значения свойств пользователя  
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {

            var user = _context.User.FirstOrDefault(u => u.Login == userLogin);

            if (user == null)
            {
                return Enumerable.Empty<UserProperty>();
            }

            var userType = user.GetType();
            var properties = userType.GetProperties();

            var userProperties = properties
                .Where(property => property.Name != nameof(User.Login)) 
                .Select(property => new UserProperty(
                    property.Name,
                    property.GetValue(user)?.ToString() ?? string.Empty
                ))
                .ToList();


            Logger.Debug("The user's properties have been successfully obtained.");

            return userProperties;
        }

        /// <summary>
        ///  Проверка существования пользователя
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsUserExists(string userLogin)
        {
            var user = _context.User.FirstOrDefault(u => u.Login == userLogin);

            if (user == null)
            {
                Logger.Debug($"The user with the username {userLogin} was not found.");
            }
            else
            {
                Logger.Debug($"A user with the username {userLogin} exists.");
            }

            return user != null;
        }

        /// <summary>
        /// Метод позволяет устанавливать значения свойств пользователя 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="userLogin"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _context.User.FirstOrDefault(u => u.Login == userLogin);

            if (user == null)
            {
                Logger.Error($"A user with the username {userLogin} exists.");
                throw new Exception($"A user with the username {userLogin} exists.");
            }

            foreach (var property in properties)
            {
                var propInfo = user.GetType().GetProperty(property.Name);

                if (propInfo == null) continue;
                var propertyType = propInfo.PropertyType;

                var convertedValue = Convert.ChangeType(property.Value, propertyType);

                propInfo.SetValue(user, convertedValue);
            }

            var passwordProperty = properties.FirstOrDefault(p => p.Name == "Password");
            if (passwordProperty != null)
            {
                var password = _context.Passwords.FirstOrDefault(p => p.UserId == user.Login);

                if (password == null)
                {
                    password = new Passwords { UserId = user.Login };
                    _context.Passwords.Add(password);
                }

                password.Password = passwordProperty.Value;
            }

            Logger.Debug($"A user with the username {userLogin} successfully added.");

            _context.SaveChanges();
        }

        /// <summary>
        ///  Получить все права в системе (смотри Описание системы клиента)  
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<Permission> GetAllPermissions()
        {
            var requestRights = _context.RequestRights.ToList()
                .Select(right => new Permission(
                    right.Id.ToString(),
                    right.Name,

                    description: null! 
                ))
                .ToList();

            var itRoles = _context.ItRole.ToList()
                .Select(right => new Permission(
                    right.Id.ToString(),
                    right.Name,

                    description: null!
                ))
                .ToList();

            Logger.Debug($"All rights in the system have been successfully obtained");

            return requestRights.Concat(itRoles);
        }

        /// <summary>
        /// Добавить права пользователю в системе 
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="rightIds"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var currentUser = _context.User.FirstOrDefault(u => u.Login == userLogin);

            if (currentUser != null)
            {
                var idsToAdd = rightIds
                    .Select(idStr => idStr.Split(':')[1])
                    .Select(int.Parse)
                    .ToList();

                var existingRights = _context.UsersRequestRight
                    .Where(urr => urr.UserId == userLogin)
                    .Select(urr => urr.RightId)
                    .ToHashSet();

                var rightsToAdd = idsToAdd.Except(existingRights).ToList();

                foreach (var rightId in rightsToAdd)
                {
                    _context.UsersRequestRight.Add(new UserRequestRight
                    {
                        UserId = userLogin,
                        RightId = rightId
                    });
                }

                Logger.Debug($"New rights to the user with {userLogin} have been successfully added.");

                _context.SaveChanges();
            }
            else
            {
                Logger.Error($"A user with the username {userLogin} not exists.");
                throw new Exception($"A user with the username {userLogin} not exists.");
            }
        }

        /// <summary>
        /// Удаляет права у пользователя в системе 
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="rightIds"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var currentUser = _context.User.FirstOrDefault(u => u.Login == userLogin);

            if (currentUser != null)
            {
                var idsToRemove = rightIds
                    .Select(idStr => idStr.Split(":")[1])
                    .Select(int.Parse)
                    .ToList();

                _context.UsersRequestRight.RemoveRange(_context.UsersRequestRight.Where(urr => urr.UserId == userLogin));

                foreach (var rightId in idsToRemove)
                {
                    _context.UsersRequestRight.Remove(new UserRequestRight
                    {
                        UserId = userLogin,
                        RightId = rightId
                    });
                }


                Logger.Debug($"New rights to the user with {userLogin} have been successfully deleted.");

                _context.SaveChanges();
            }
            else
            {
                Logger.Error($"A user with the username {userLogin} not exists.");
                throw new Exception($"A user with the username {userLogin} not exists.");
            }
        }

        /// <summary>
        /// Получить права пользователя в системе 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var userRequestRights = _context.UsersRequestRight
                .Where(urr => urr.UserId == userLogin)
                .ToList();

            var permissions = userRequestRights
                .Join(_context.RequestRights,
                    userRequestRight => userRequestRight.RightId,
                    requestRight => requestRight.Id,
                    (userRequestRight, requestRight) => requestRight.Name
                )
                .ToList();


            Logger.Debug($"Rights to the user with {userLogin} have been successfully received.");

            return permissions;
        }

  
    }
}




