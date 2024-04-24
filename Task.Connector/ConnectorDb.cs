using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _db;

        public void StartUp(string connectionString)
        {
            var connectionInfo = new ConnectionInfo(connectionString);
            var dbContextFactory = new DbContextFactory(connectionInfo.ConnectionString);
            _db = dbContextFactory.GetContext(connectionInfo.Provider);
        }

        public void CreateUser(UserToCreate user)
        {
            if (IsUserExists(user.Login))
            {
                Logger.Warn($"Пользователь с логином {user.Login} уже существует");
                return;
            }

            _db.Passwords.Add(new Sequrity()
            {
                Password = user.HashPassword,
                UserId = user.Login,
            });

            var toAdd = CreateUser(user.Properties, user.Login);

            _db.Users.Add(toAdd);
            _db.SaveChanges();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var properties = new List<Property>();
            var userProperties = typeof(User).GetProperties();
            
            foreach (var property in userProperties)
            {
                if (property.Name.ToLower() != "login")
                {
                    var userProperty = new Property(property.Name, property.Name);
                    properties.Add(userProperty);
                }
            }
            
            properties.Add(new Property("Password", "Пароль пользователя"));

            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Warn($"Пользователя с логином {userLogin} не существует");
                return Enumerable.Empty<UserProperty>();
            }

            var user = _db.Users.FirstOrDefault(x => x.Login == userLogin);
            var result = new List<UserProperty>();

            var userProperties = typeof(User).GetProperties();

            foreach (PropertyInfo prop in userProperties)
            {
                if (prop.Name.ToLower() != "login")
                {
                    var userProperty = new UserProperty(prop.Name, prop.GetValue(user)?.ToString());

                    result.Add(userProperty);
                }
            }

            return result;
        }

        public bool IsUserExists(string userLogin)
        {
            return _db.Users.Any(x => x.Login == userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Warn($"Пользователя с логином {userLogin} не существует");
                return;
            }

            var user = _db.Users.FirstOrDefault(x => x.Login == userLogin);

            UpdateUserProperties(user, properties);

            _db.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var itRoles = _db.ITRoles
                .Select(x => new Permission(x.Id.ToString(), x.Name, "It role"))
                .ToList();

            return _db.RequestRights
                .Select(x => new Permission(x.Id.ToString(), x.Name, "Request right"))
                .ToList()
                .Union(itRoles);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Warn($"Пользователя с логином {userLogin} не существует");
                return;
            }

            foreach (string rightId in rightIds)
            {
                var id = GetPermissonId(rightId);

                if (IsRoleId(rightId))
                {
                    if (!IsItRoleExists(id))
                    {
                        Logger.Warn($"ItRole с id={id} не сущетсвует");
                    }
                    else
                    {
                        var toAdd = new UserITRole
                        {
                            RoleId = id,
                            UserId = userLogin,
                        };

                        _db.UserITRoles.Add(toAdd);             
                    }
                }

                if (IsRequestRightId(rightId))
                {
                    if (!IsRequestRightExists(id))
                    {
                        Logger.Warn($"RequestRight с id={id} не сущетсвует");
                    }
                    else
                    {
                        var toAdd = new UserRequestRight
                        {
                            RightId = id,
                            UserId = userLogin,
                        };

                        _db.UserRequestRights.Add(toAdd);
                    }
                }
            }

            _db.SaveChanges();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Warn($"Пользователя с логином {userLogin} не существует");
                return;
            }

            foreach (string rightId in rightIds)
            {
                var id = GetPermissonId(rightId);

                if (IsRoleId(rightId))
                {
                    if (!IsItRoleExists(id))
                    {
                        Logger.Warn($"ItRole с id={id} не сущетсвует");
                    }
                    else
                    {
                        var toRemove = _db.UserITRoles
                            .FirstOrDefault(x => x.UserId == userLogin && x.RoleId == id);

                        _db.UserITRoles.Remove(toRemove); 
                    }
                }

                if (IsRequestRightId(rightId))
                {
                    if (!IsRequestRightExists(id))
                    {
                        Logger.Warn($"RequestRight с id={id} не сущетсвует");
                    }
                    else
                    {
                        var toRemove = _db.UserRequestRights
                            .FirstOrDefault(x => x.UserId == userLogin && x.RightId == id);

                        _db.UserRequestRights.Remove(toRemove);                        
                    }
                }
            }

            _db.SaveChanges();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Warn($"Пользователя с логином {userLogin} не существует");
                return Enumerable.Empty<string>();
            }

            var permissions = new List<string>();

            var roleIds = _db.UserITRoles
                .Where(x => x.UserId == userLogin)
                .Select(x => x.RoleId);  

            var roles = _db.ITRoles
                .Where(x => roleIds.Any(i => i == x.Id))
                .Select(x => x.Name);

            permissions.AddRange(roles);

            var requestRightIds = _db.UserRequestRights
                .Where(x => x.UserId == userLogin)
                .Select(x => x.RightId);

            var requestRights = _db.RequestRights
                .Where(x => requestRightIds.Any(i => i == x.Id))
                .Select (x => x.Name);

            permissions.AddRange(requestRights);

            return permissions;
        }

        public ILogger Logger { get; set; }

        private User CreateUser(IEnumerable<UserProperty> properties, string login)
        {
            var user = new User();

            user.Login = login;
            user.LastName = GetUserPropertyValue(properties, nameof(user.LastName)) ?? string.Empty;
            user.FirstName = GetUserPropertyValue(properties, nameof(user.FirstName)) ?? string.Empty;
            user.MiddleName = GetUserPropertyValue(properties, nameof(user.MiddleName)) ?? string.Empty;
            user.TelephoneNumber = GetUserPropertyValue(properties, nameof(user.TelephoneNumber)) ?? string.Empty;
            
            var isLead = GetUserPropertyValue(properties, nameof(user.IsLead));
            user.IsLead = isLead is null ? default(bool) : bool.Parse(isLead);

            return user;
        }

        private void UpdateUserProperties(User user, IEnumerable<UserProperty> properties)
        {
            user.LastName = GetUserPropertyValue(properties, nameof(user.LastName)) ?? user.LastName;
            user.FirstName = GetUserPropertyValue(properties, nameof(user.FirstName)) ?? user.FirstName;
            user.MiddleName = GetUserPropertyValue(properties, nameof(user.MiddleName)) ?? user.MiddleName;
            user.TelephoneNumber = GetUserPropertyValue(properties, nameof(user.TelephoneNumber)) ?? user.TelephoneNumber;

            var isLead = GetUserPropertyValue(properties, nameof(user.IsLead));
            user.IsLead = isLead is null ? user.IsLead : bool.Parse(isLead);
        }

        private string? GetUserPropertyValue(IEnumerable<UserProperty> properties, string name)
        {
            return properties.FirstOrDefault(x => x.Name.ToLower() == name.ToLower())?.Value;
        }

        private bool IsItRoleExists(int itRoleId)
        {
            return _db.ITRoles.Any(x => x.Id == itRoleId);
        }

        private bool IsRequestRightExists(int requestRightId)
        {
            return _db.RequestRights.Any(x => x.Id == requestRightId);
        }

        private bool IsRoleId(string rightId)
        {
            return rightId.Split(":")[0] == "Role";
        }

        private bool IsRequestRightId(string rightId)
        {
            return rightId.Split(":")[0] == "Request";
        }

        private int GetPermissonId(string rightId)
        {
            return int.Parse(rightId.Split(":")[1]);
        }
    }
}