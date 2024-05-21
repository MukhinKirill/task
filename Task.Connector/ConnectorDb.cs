using System.Data.Common;
using Task.Connector.Mapper;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public required ILogger Logger { get; set; }

        //из-за невозможности использования конструтора, здесь нет readonly.
        private DataContext _context;

        //Можно заменить на json файл и брать строки из него. Специально статика, чтобы не выделялось место под эти данные в каждом экземляре класса
        private static IDictionary<string, string> _userPropertyDescriptions = new Dictionary<string, string>()
            {
                {"LastName", "Users name" },
                {"FirstName", "Users first name" },
                {"MiddleName", "Users middle name" },
                {"TelephoneNumber", "Users phone number" },
                {"IsLead", "Is User a lead" },
                {"Password", "Users Password" }
            };

        //Нарушение принципа инверсии зависимостей, Connector не должен сам для себя инициализировать свою зависимость.
        //+ для инициализации лучше использовать ctor
        //Исправить ничего здесь не могу, тк нет доступа к библиотекам
        //Лучшим решением будет сделать класс регистратор, который будет запускаться в стартапе программы и регистрировать нужный dbContext в DI
        public void StartUp(string connectionString)
        {
           Logger.Debug("Started Connector setup");

            if(connectionString == string.Empty)
            {
                var error = "Connection string is empty!";
                Logger.Error(error);
                throw new ArgumentException(error);
            }

            var stringBuilder = new DbConnectionStringBuilder()
            {
                ConnectionString = connectionString
            };

            if(!stringBuilder.TryGetValue("ConnectionString", out var dbConnectionStr))
            {
                var error = "Connection string does not contain database connection string!";
                Logger.Error(error);
                throw new ArgumentException(error);
            }

            var contextFactory = new DbContextFactory((string)dbConnectionStr);

            if (!stringBuilder.TryGetValue("Provider", out var providerName))
            {
                var error = "Connection string has not provider!";
                Logger.Error(error);
                throw new ArgumentException(error);
            }

            if(((string)providerName).Contains("Postgre", StringComparison.OrdinalIgnoreCase))
            {
                providerName = "POSTGRE";
            }
            else if(((string)providerName).Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                providerName = "MSSQL";
            }
            else
            {
                var error = "Unknown provider!";
                Logger.Error(error);
                throw new ArgumentException(error);
            }

            _context = contextFactory.GetContext((string)providerName);

            Logger.Debug("Connector setuped");
        }

        public void CreateUser(UserToCreate user)
        {
            Logger.Debug("Adding new user");
            if (_context.Users.Any(u => u.Login == user.Login))
            {
                Logger.Error($"The user with login {user.Login} is exist!");
                return;
            }
            try
            {
                var entity = UserMapper.CreateUserFromUserProps(user.Properties);
                entity.Login = user.Login;

                _context.Users.Add(entity);
                _context.Passwords.Add(
                    new()
                    {
                        UserId = user.Login,
                        Password = user.HashPassword
                    }
                );

                _context.SaveChanges();
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
            var props = new List<Property>();
            var propsNames = typeof(User).GetProperties().Select(p => p.Name).ToList();
            propsNames.Add("Password");
            foreach (var name in propsNames)
            {
                if (name == "Login") continue;

                if(!_userPropertyDescriptions.TryGetValue(name, out var description))
                {
                    Logger.Warn($"No description for the property {name}");
                    description = string.Empty;
                }

                props.Add(new(name, description));
            }
            return props;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug($"Getting a list of user properties with login {userLogin}");
            var user = _context.Users.FirstOrDefault(x => x.Login == userLogin);
            if (user == null)
            {
                Logger.Error($"The user with login {userLogin} does not exist!");
                return Array.Empty<UserProperty>();
            }
            return typeof(User).GetProperties().Where(p => p.Name != "Login").Select(p => new UserProperty(p.Name, p.GetValue(user).ToString()));
        }

        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Checking the existence of a user with a login {userLogin}");
            return _context.Users.Any(u => u.Login == userLogin);
        }
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug($"Updating the user properties with login {userLogin}");
            if (userLogin == string.Empty)
                throw new ArgumentException("Login is empty!", nameof(userLogin));

            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);

            if (user == null)
            {
                Logger.Error($"The user with login {userLogin} does not exist!");
                return;
            }
            
            foreach (var property in properties)
            {
                if (UserMapper.SetUserProperty(user, property.Name, property.Value))
                    Logger.Warn($"The property with name {property.Name} does not exist!");
            }
            _context.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug($"Getting a permissions list");
            var permissions = new List<Permission>();
            permissions.AddRange(_context.RequestRights.Select(r => new Permission(r.Id.ToString(), r.Name, "RequestRight")));
            permissions.AddRange(_context.ITRoles.Select(r => new Permission(r.Id.ToString(), r.Name, "ItRole")));
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Adding permissions to user with a login {userLogin}");
            if (!_context.Users.Any(u => u.Login == userLogin))
            {
                Logger.Error($"The user with login {userLogin} does not exist!");
                return;
            }

            var userRoles = _context.UserITRoles.Where(ur => ur.UserId == userLogin).Select(r => r.RoleId).ToArray();
            var userRights = _context.UserRequestRights.Where(ur => ur.UserId == userLogin).Select(r => r.RightId).ToArray();
            foreach (var right in rightIds)
            {

                var data = right.Split(":");
                if (data.Length != 2 || !int.TryParse(data[1], out var rightId))
                {
                    Logger.Warn($"The right of this type {right} cannot be recognized ");
                    continue;
                }

                if (data[0] == "Role")
                {
                    if (userRoles.Any(r => r == rightId))
                    {
                        Logger.Warn($"The role with id {rightId} for this user is exist!");
                        continue;
                    }
                    _context.UserITRoles.Add(new UserITRole() { UserId = userLogin, RoleId = rightId });
                }
                else if (data[0] == "Request")
                {
                    if (userRights.Any(r => r == rightId)) 
                    {
                        Logger.Warn($"The right with id {rightId} for this user is exist!");
                        continue;
                    }
                    _context.UserRequestRights.Add(new UserRequestRight() { UserId = userLogin, RightId = rightId });
                }
                else
                {
                    Logger.Warn($"The right of this type {right} cannot be recognized ");
                    continue;
                }
            }
            _context.SaveChanges();

        }

        //Здесь лучше использовать ExecuteDelete() вместо создания массивов, но версия ef core заблокирована из-за зависимостей
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Removing user permissions with a login {userLogin}");
            if (!_context.Users.Any(u => u.Login == userLogin))
            {
                Logger.Error($"The user with login {userLogin} does not exist!");
                return;
            }

            var rights = new List<UserRequestRight>();
            var roles = new List<UserITRole>();
            foreach (var right in rightIds)
            {

                var data = right.Split(":");
                if (data.Length != 2 || !int.TryParse(data[1], out var rightId))
                {
                    Logger.Warn($"The right of this type {right} cannot be recognized ");
                    continue;
                }

                if (data[0] == "Role")
                {
                    var dbRole = _context.UserITRoles.FirstOrDefault(r => r.UserId == userLogin && r.RoleId == rightId);
                    if (dbRole != null) roles.Add(dbRole);

                }
                else if (data[0] == "Request")
                {
                    var dbRight = _context.UserRequestRights.FirstOrDefault(r => r.UserId == userLogin && r.RightId == rightId);
                    if (dbRight != null) rights.Add(dbRight);
                }
                else
                {
                    Logger.Warn($"The right of this type {right} cannot be recognized ");
                    continue;
                }
            }

            _context.UserRequestRights.RemoveRange(rights);
            _context.UserITRoles.RemoveRange(roles);
            _context.SaveChanges();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Getting user permissions with a login {userLogin}");
            if (!_context.Users.Any(u => u.Login == userLogin))
            {
                var error = $"The user with login {userLogin} does not exist!";
                Logger.Error(error);
                throw new NullReferenceException(error);
            }
            var permissions = new List<string>();
            permissions.AddRange(_context.UserITRoles.Where(ur => ur.UserId == userLogin).Select(ur => $"Role:{ur.RoleId}"));
            permissions.AddRange(_context.UserRequestRights.Where(ur => ur.UserId == userLogin).Select(ur => $"Request:{ur.RightId}"));
            return permissions;
        }
    }
}