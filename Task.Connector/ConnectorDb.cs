using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private Context _context;
        
        public void StartUp(string connectionString)
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>().UseNpgsql("Host=localhost;Port=5432;Database=AvanpostDb;Username=phenirain;Password=ru13aaogh06(;").Options;
            _context = new Context(options);
        }

        public async void CreateUser(UserToCreate user)
        {
            try
            {
                User newUser = new User
                {
                    Login = user.Login,
                    LastName = user.Properties.FirstOrDefault(p => p.Name == "LastName")?.Value ?? "",
                    FirstName = user.Properties.FirstOrDefault(p => p.Name == "FirstName")?.Value ?? "",
                    MiddleName = user.Properties.FirstOrDefault(p => p.Name == "MiddleName")?.Value ?? "",
                    TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "TelephoneNumber")?.Value ?? "",
                    IsLead = user.Properties.FirstOrDefault(p => p.Name == "IsLead")?.Value == "True" ? true : false,
                };

                _context.Add(newUser);
                await _context.SaveChangesAsync();
                Logger.Debug($"User with Login: {user.Login} was created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create user with Login: {user.Login}. Error: {ex.Message}");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var properties = typeof(User).GetProperties().Where(p => p.Name != "Login");
                var usersProperties = properties.Select(p => new Property(p.Name, "")).ToList();
                usersProperties.Add(new Property("Password", ""));
                Logger.Debug($"All properties were successfully retrieved in count: {usersProperties.Count()}");
                return usersProperties;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get all properties. Error: {ex.Message}");
                return new List<Property>();
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = GetUserByLogin(userLogin);
                var userProperties = new List<UserProperty>();
                foreach (var property in GetAllProperties())
                {
                    if (property.Name == "Password") continue;
                    var propertyValue = user.GetType().GetProperty(property.Name)?.GetValue(user)?.ToString() ?? "";
                    userProperties.Add(new UserProperty(property.Name, propertyValue));
                }
                Logger.Debug($"User`s properties with login: {userLogin} were successfully retrieved in count: {userProperties.Count}");
                return userProperties;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get properties for user with Login: {userLogin}. Error: {ex.Message}");
                return new List<UserProperty>();
            }
        }

        public bool IsUserExists(string userLogin)
        {
            return _context.Users.FirstOrDefault(u => u.Login == userLogin) != null;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = GetUserByLogin(userLogin);
                foreach (var property in properties)
                {
                    user.GetType().GetProperty(property.Name)?.SetValue(user, property.Value);
                }
                _context.SaveChanges();
                Logger.Debug($"User`s properties with login: {userLogin} were successfully updated");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to update user properties for user with Login: {userLogin}. Error: {ex.Message}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var requestRights = _context.RequestRights.ToList();
                var itRoles = _context.ItRoles.ToList();
                var permissions = new Permission[requestRights.Count + itRoles.Count];
                int i = 0;
                for (int j = i; i < requestRights.Count; i++)
                {
                    permissions[i] = new Permission(requestRights[i].Id.ToString(), requestRights[i].Name, "");
                }
                for (int j = 0; j < itRoles.Count; j++)
                {
                    permissions[i + j] = new Permission(itRoles[j].Id.ToString() + i, itRoles[j].Name, "");
                } 
                Logger.Debug($"All permissions were successfully retrieved in count: {permissions.Length}");
                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get all permissions. Error: {ex.Message}");
                return new List<Permission>();
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var user = GetUserByLogin(userLogin);
                var (userRequests, userRoles) = GetRequestRightsByIds(userLogin, rightIds);
                _context.UserRequestRights.AddRange(userRequests);
                _context.UsersItRoles.AddRange(userRoles);
                _context.SaveChanges();
                Logger.Debug($"Permissions for user with Login: {userLogin} were successfully added");       
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to add permissions for user with Login: {userLogin}. Error: {ex.Message}");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                GetUserByLogin(userLogin);
                foreach (var rightDesc in rightIds)
                {
                    var helpString = rightDesc.Split(':');
                    var permission = helpString[0];
                    var permissionId = helpString[1];
                    if (permission == "Role")
                    {
                        var role = _context.UsersItRoles.FirstOrDefault(r => r.ItRoleId == int.Parse(permissionId) && r.UserId == userLogin);
                        if (role != null)
                        {
                            _context.UsersItRoles.Remove(role);
                        }
                        else
                        {
                            Logger.Error($"User Role with id {permissionId} not found");
                        }
                    }
                    else
                    {
                        var userRequestRight = _context.UserRequestRights.FirstOrDefault(urr => urr.UserId == userLogin && urr.RequestRightId == int.Parse(permissionId));
                        if (userRequestRight != null)
                        {
                            _context.UserRequestRights.Remove(userRequestRight);
                        }
                        else
                        {
                            Logger.Error($"User Request Right with right id: {permissionId} not found");
                        }
                    }
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to remove permissions for user with Login: {userLogin}. Error: {ex.Message}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                if (IsUserExists(userLogin))
                {
                    return _context.UserRequestRights.Join(
                            _context.RequestRights,
                            userRequestRight => userRequestRight.RequestRightId,
                            requestRight => requestRight.Id,
                            (userRequestRight, requestRight) => new { userRequestRight.UserId, requestRight.Name }
                        )
                        .Where(userRequest =>
                            !string.IsNullOrEmpty(userRequest.UserId) && !string.IsNullOrEmpty(userRequest.Name))
                        .Select(userRequest => userRequest.Name);
                }
                else
                {
                    throw new Exception($"User with Login: {userLogin} does not exist");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get permissions for user with Login: {userLogin}. Error: {ex.Message}");
                return new List<string>();
            }
        }

        private User GetUserByLogin(string userLogin)
        {
            try
            {
                return _context.Users.First(u => u.Login == userLogin);
            }
            catch (Exception ex)
            {
                throw new Exception($"User with Login: {userLogin} not found");
            }
        }

        private (IEnumerable<UserRequestRight>, IEnumerable<UserItRole>) GetRequestRightsByIds(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var userRequestRights = new List<UserRequestRight>();
                var userItRoles = new List<UserItRole>();
                foreach (var rightDesc in rightIds)
                {
                    var helpString = rightDesc.Split(':');
                    var permission = helpString[0];
                    var permissionId = helpString[1];
                    if (permission == "Role")
                    {
                        var role = _context.ItRoles.FirstOrDefault(r => r.Id == int.Parse(permissionId));
                        if (role != null)
                        {
                            userItRoles.Add(new UserItRole
                            {
                                UserId = userLogin,
                                ItRoleId = role.Id
                            });
                        }
                        else
                        {
                            Logger.Error($"Role with id {permissionId} not found");
                        }
                    }
                    else
                    {
                        var right = _context.RequestRights.FirstOrDefault(r => r.Id == int.Parse(permissionId));
                        if (right != null)
                        {
                            userRequestRights.Add(new UserRequestRight
                            {
                                UserId = userLogin,
                                RequestRightId = right.Id
                            });
                        }
                        else
                        {
                            Logger.Error($"Right with id: {permissionId} was not found");
                        }
                    }
                }

                return (userRequestRights, userItRoles);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to map rightIds into user right for user with login: {userLogin}");
                return (new List<UserRequestRight>(), new List<UserItRole>());
            }
        }

        public ILogger Logger { get; set; }
    }
}