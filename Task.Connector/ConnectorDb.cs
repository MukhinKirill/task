using System.Reflection;
using Task.Connector.Data;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private TestDbContext _dbContext;
        public ILogger Logger { get; set; }
        public void StartUp(string connectionString)
        {
            _dbContext = new TestDbContext(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            User newUser = new User
            {
                Login = user.Login,
                LastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "",
                FirstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "",
                MiddleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "",
                TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ?? ""
            };

            var isLead = user.Properties.FirstOrDefault(p => p.Name == "isLead");
            if (isLead != null)
            {
                newUser.IsLead = Convert.ToBoolean(isLead.Value);
            }
            else
            {
                newUser.IsLead = false;
            }

            PasswordEntity userPassword = new PasswordEntity
            {
                UserId = user.Login,
                Password = user.HashPassword
            };

            _dbContext.Users.Add(newUser);
            _dbContext.Passwords.Add(userPassword);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            User user = _dbContext.Users.First();
            PasswordEntity password = _dbContext.Passwords.FirstOrDefault(p => p.UserId == user.Login);
            PropertyInfo passwordPropertyInfo = password?.GetType().GetProperty("Password");
            PropertyInfo[] userPropertyInfo = user?.GetType().GetProperties();
            int loginIndex = Array.IndexOf<PropertyInfo>(userPropertyInfo, user.GetType().GetProperty("Login"));
            userPropertyInfo[loginIndex] = passwordPropertyInfo;
            var properties = new List<Property>();
            foreach(var property in userPropertyInfo)
            {    
                properties.Add(new Property(property.Name, " "));
            }
            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            User user = _dbContext.Users.Find(userLogin);
            
            if (user != null)
            {
                return new List<UserProperty>
                {
                    new UserProperty("lastName", user.LastName),
                    new UserProperty("firstName", user.FirstName),
                    new UserProperty("middleName", user.MiddleName),
                    new UserProperty("telephoneNumber", user.TelephoneNumber),
                    new UserProperty("isLead", user.IsLead.ToString())
                };
            }
            return new List<UserProperty>();
        }

        public bool IsUserExists(string userLogin)
        {
            return _dbContext.Users.Any(u => u.Login == userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            User user = _dbContext.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user != null)
            {
                foreach (var property in properties)
                {
                    switch (property.Name.ToLower())
                    {
                        case "lastname":
                            user.LastName = property.Value;
                            break;
                        case "firstname":
                            user.FirstName = property.Value;
                            break;
                        case "middlename":
                            user.MiddleName = property.Value;
                            break;
                        case "telephonenumber":
                            user.TelephoneNumber = property.Value;
                            break;
                        case "islead":
                            user.IsLead = Convert.ToBoolean(property.Value);
                            break;
                        default:
                            throw new ArgumentException($"Unknown property: {property.Name}");
                    }
                }
                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var permissions = new List<Permission>();
            var requestRights = _dbContext.RequestRights.ToList();
            var roles = _dbContext.ItRoles.ToList();
            foreach(var request in requestRights)
            {
                permissions.Add(new Permission(request.Id.ToString(), request.Name, "Request"));
            }
            foreach (var role in roles)
            {
                permissions.Add(new Permission(role.Id.ToString(), role.Name, "Role"));
            }
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            bool userExist = IsUserExists(userLogin);
            if (userExist)
            {
                foreach(var permission in rightIds)
                {
                    string[] words = permission.Split(':', StringSplitOptions.RemoveEmptyEntries);

                    if (permission.StartsWith("Role"))
                    {
                        int roleId = int.Parse(words[1]);
                        _dbContext.UserItroles.Add(new UserItrole { UserId = userLogin, RoleId = roleId });
                        _dbContext.SaveChanges();
                    }
                    if (permission.StartsWith("Request"))
                    {
                        int requestId = int.Parse(words[1]);
                        _dbContext.UserRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = requestId});
                        _dbContext.SaveChanges();
                    }
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            bool userExist = IsUserExists(userLogin);
            if (userExist)
            {
                foreach (var permission in rightIds)
                {
                    string[] words = permission.Split(':', StringSplitOptions.RemoveEmptyEntries);

                    if (permission.StartsWith("Role"))
                    {
                        int roleId = int.Parse(words[1]);
                        _dbContext.UserItroles.Remove(new UserItrole { UserId = userLogin, RoleId = roleId });
                        _dbContext.SaveChanges();
                    }
                    if (permission.StartsWith("Request"))
                    {
                        int requestId = int.Parse(words[1]);
                        _dbContext.UserRequestRights.Remove(new UserRequestRight { UserId = userLogin, RightId = requestId });
                        _dbContext.SaveChanges();
                    }
                }
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            bool userExist = IsUserExists(userLogin);
            var permissions = new List<string>();
            if (userExist)
            {
                permissions.AddRange(_dbContext.UserItroles.Where(ur => ur.UserId == userLogin).Select(ur => $"Role:{ur.RoleId}"));
                permissions.AddRange(_dbContext.UserRequestRights.Where(ur => ur.UserId == userLogin).Select(ur => $"Request:{ur.RightId}"));
            }
            return permissions;
        }

    }
}