using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _context;

        public ConnectorDb() {}

        public void StartUp(string connectionString)
        {
            var connectionStringPattern = @"ConnectionString='(?<ConnectionString>[^']*)';Provider='(?<Provider>[^']*)'";
            var match = Regex.Match(connectionString, connectionStringPattern);

            if (!match.Success) throw new ArgumentException("Invalid connection string format", nameof(connectionString));

            var actualConnectionString = match.Groups["ConnectionString"].Value;
            var provider = match.Groups["Provider"].Value.ToLower().Contains("postgresql") ? "POSTGRE" : "MSSQL";

            var factory = new DbContextFactory(actualConnectionString);
            _context = factory.GetContext(provider);
        }

        public void CreateUser(UserToCreate user)
        {
            Logger.Debug("Creating user.");
            //Операция добавления пользователя и его пароля должны происходить
            //как единое целое, по-этому нужна транзакция
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    
                    var newUser = MapToUser(user);
                    _context.Users.Add(newUser);
                    _context.SaveChanges();

                    var newPassword = new Sequrity
                    {
                        //Шифрование пароля
                        Password = HashPassword(user.HashPassword),
                        UserId = newUser.Login
                    };

                    _context.Passwords.Add(newPassword);
                    _context.SaveChanges();

                    transaction.Commit();
                    Logger.Debug("User created successfully.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"Error creating user: {ex.Message}");
                    throw;
                }

            }
        }
        private User MapToUser(UserToCreate userToCreate)
        {
            Logger.Debug("Mapping user properties.");
            var user = new User
            {
                Login = userToCreate.Login,
                LastName = string.Empty,
                FirstName = string.Empty,
                MiddleName = string.Empty,
                TelephoneNumber = string.Empty,
                IsLead = false
            };

            MapProperties(user, userToCreate.Properties);
            Logger.Debug("User properties mapped successfully.");
            return user;
        }

        private void MapProperties(User user, IEnumerable<UserProperty> properties)
        {
            var propertyMap = new Dictionary<string, Action<string>>
            {
                { "lastName", value => user.LastName = value ?? string.Empty },
                { "firstName", value => user.FirstName = value ?? string.Empty },
                { "middleName", value => user.MiddleName = value ?? string.Empty },
                { "telephoneNumber", value => user.TelephoneNumber = value ?? string.Empty },
                { "isLead", value => user.IsLead = bool.Parse(value ?? "false") }

            };

            foreach (var property in properties)
            {
                if (propertyMap.TryGetValue(property.Name, out var setProperty))
                {
                    setProperty(property.Value);
                }
            }
        }

        private string HashPassword(string password)
        {
            // тут должен хешироваться пароль
            // (стоит добавить соль для каждого пароля!)
            return password;
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Getting all properties.");
            //Допустим нужно выдать список свойств сущности пользователя
            var properties = new List<Property>();

            var userType = typeof(User);
            var propertiesInfo = userType.GetProperties();

            foreach (var propertyInfo in propertiesInfo)
            {
                var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null && propertyInfo.Name!="Login")
                {
                    properties.Add(new Property(propertyInfo.Name, columnAttribute.Name));
                }
            }

            properties.Add(new Property("password", "Password"));
            Logger.Debug("All properties retrieved successfully.");
            return properties;

        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug("Getting user properties.");
            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger.Warn("User not found.");
                return Enumerable.Empty<UserProperty>();
            }
            var password = _context.Passwords.FirstOrDefault(p => p.UserId == userLogin);
            //TODO: тоже можно написать маппер
            var properties = new UserProperty[]
            {
                new UserProperty("lastName", user.LastName),
                new UserProperty("firstName", user.FirstName),
                new UserProperty("middleName", user.MiddleName),
                new UserProperty("telephoneNumber", user.TelephoneNumber),
                new UserProperty("isLead", user.IsLead.ToString()),
            };
            Logger.Debug("User properties retrieved successfully.");
            return properties;
        }

        public bool IsUserExists(string userLogin)
        {
            Logger.Debug("Checking if user exists.");
            var exists = _context.Users.Any(u => u.Login == userLogin);
            Logger.Debug($"User exists: {exists}");
            return exists;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug("Updating user properties.");
            var user = _context.Users.FirstOrDefault(u => u.Login==userLogin);
            if (user == null)
            {
                Logger.Error("User not found.");
                throw new Exception("User not found");
            }
            MapProperties(user, properties);
            _context.SaveChanges();
            Logger.Debug("User properties updated successfully.");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug("Getting all permissions.");
            var permissions = _context.RequestRights
                .Select(r => new Permission(r.Id.ToString(), r.Name, "Request")).ToList()
                .Concat(_context.ITRoles.Select(r => new Permission(r.Id.ToString(), r.Name, "Role")));
            Logger.Debug("All permissions retrieved successfully.");
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug("Adding user permissions.");
            foreach (var rightId in rightIds)
            {
                var idSplitted = rightId.Split(':');

                if (!int.TryParse(idSplitted[1], out int permissionId))
                {
                    Logger.Error("Invalid id format.");
                    throw new Exception("Invalid id format");
                }
                if (idSplitted[0]=="Request")
                {
                    //проверка на наличие уже существующей связи
                    if (_context.UserRequestRights.Any(urr => urr.UserId == userLogin && urr.RightId == permissionId)) 
                        continue;

                    var userRequestRight = new UserRequestRight
                    {
                        UserId = userLogin,
                        RightId = permissionId
                    };
                    _context.UserRequestRights.Add(userRequestRight);
                } 
                else if (idSplitted[0] == "Role")
                {
                    if (_context.UserITRoles.Any(uir => uir.UserId == userLogin && uir.RoleId == permissionId))
                        continue;

                    var userItRole = new UserITRole
                    {
                        UserId = userLogin,
                        RoleId = permissionId
                    };
                    _context.UserITRoles.Add(userItRole);
                }
                else
                {
                    Logger.Error("Invalid permission.");
                    throw new Exception("Invalid permission");
                }
            }
            _context.SaveChanges();        
            Logger.Debug("User permissions added successfully.");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug("Removing user permissions.");
            foreach (var rightId in rightIds)
            {
                var idSplitted = rightId.Split(':');

                if (!int.TryParse(idSplitted[1], out int permissionId))
                {
                    Logger.Error("Invalid id format.");
                    throw new Exception("Invalid id format");
                }


                if (idSplitted[0] == "Request")
                {
                    var userRequestRight = _context.UserRequestRights.FirstOrDefault(urr => urr.UserId == userLogin && urr.RightId==permissionId);
                    if (userRequestRight != null) _context.UserRequestRights.Remove(userRequestRight);
                }
                else if (idSplitted[0] == "Role")
                {
                    var userItRole = _context.UserITRoles.FirstOrDefault(uit => uit.UserId == userLogin && uit.RoleId == permissionId);
                    if (userItRole != null) _context.UserITRoles.Remove(userItRole);
                }
                else
                {
                    Logger.Error("Invalid permission.");
                    throw new Exception("Invalid permission");
                }
            }
            _context.SaveChanges();
            Logger.Debug("User permissions removed successfully.");
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug("Getting user permissions.");
            //Допустим необходимо вернуть id прав
            var userRequestRights = _context.UserRequestRights
                .Where(urr=>urr.UserId== userLogin)
                .Select(urr=>urr.RightId.ToString()).ToList();

            var userItRoles = _context.UserITRoles
                .Where(uit=> uit.UserId== userLogin)
                .Select (uit=>uit.RoleId.ToString()).ToList();
            Logger.Debug("User permissions retrieved successfully.");
            return userRequestRights.Concat(userItRoles);
        }

        public ILogger Logger { get; set; }
    }
}