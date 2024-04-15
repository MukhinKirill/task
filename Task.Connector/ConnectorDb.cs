using Microsoft.EntityFrameworkCore;
using Task.Connector.Helpers;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private string? _connectionString;
        private DataContext _dbContext;
        public Exception _excep { get; private set; }
        public ILogger Logger { get; set; }
        
        // Group of rights for requests
        private readonly string _requestRightGroupName = "Request";
        // Group of rights for IT roles
        private readonly string _itRoleRightGroupName = "Role";

        // Method to initialize the connection
        public void StartUp(string connectionString)
        {
            try
            {
                _connectionString = Helpers.Connector.DefaultConnectionString(connectionString);
                if (string.IsNullOrEmpty(_connectionString))
                    throw new ArgumentException("Connection string integrity violation!", nameof(connectionString));

                var provider = Helpers.Connector.GetProvider(connectionString);
                if (string.IsNullOrEmpty(provider))
                    throw new ArgumentException("Failed to determine the database provider", nameof(connectionString));

                _dbContext = new DbContextFactory(_connectionString).GetContext(provider);
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
            }
        }

        // Method to create a user
        public void CreateUser(UserToCreate user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Login))
                    throw new ArgumentException("User information is missing", nameof(user));

                var password = new Sequrity()
                {
                    UserId = user.Login,
                    Password = user.HashPassword,
                };

                var newUser = new User()
                {
                    Login = user.Login,
                    FirstName = GetValueOrDefault(user.Properties, nameof(User.FirstName), "Test1"),
                    MiddleName = GetValueOrDefault(user.Properties, nameof(User.MiddleName), "Test2"),
                    LastName = GetValueOrDefault(user.Properties, nameof(User.LastName), "Test3"),
                    TelephoneNumber = GetValueOrDefault(user.Properties, nameof(User.TelephoneNumber), "TestPhone"),
                    IsLead = GetValueOrDefault(user.Properties, nameof(User.IsLead), "false") == "true"
                };

                _dbContext.Users.Add(newUser);
                _dbContext.Passwords.Add(password);
                _dbContext.SaveChanges();
            }
            catch(Exception ex) 
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
            }
        }

        // Method to retrieve a value from user properties or use a default value
        private string GetValueOrDefault(IEnumerable<UserProperty> properties, string propertyName, string defaultValue)
        {
            return properties.FirstOrDefault(x => x.Name.ToLower() == propertyName.ToLower())?.Value ?? defaultValue;
        }

        // Method to get all properties of a user
        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var allProperties = typeof(User).GetProperties().Where(x => x.Name != nameof(User.Login));
                var allPropertiesName = allProperties.Select(x => new Property(x.Name, string.Empty))
                    .Append(new Property(nameof(Sequrity.Password), string.Empty));
                return allPropertiesName;
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
                return null!;
            }
        }

        // Method to update user properties
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Login == userLogin);
                if (user is null)
                {
                    throw new Exception($"User with this login not found\r\n");
                }

                user.FirstName = properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.FirstName).ToLower())?.Value ?? user.FirstName;
                user.MiddleName = properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.MiddleName).ToLower())?.Value ?? user.MiddleName;
                user.LastName = properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.LastName).ToLower())?.Value ?? user.LastName;
                user.TelephoneNumber = properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.TelephoneNumber).ToLower())?.Value ?? user.TelephoneNumber;
                string IsLead = user.IsLead.ToString().ToLower();
                IsLead = properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.IsLead).ToLower())?.Value.ToLower() ?? IsLead;
                user.IsLead = IsLead == "true";

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
            }
        }

        // Method to check if a user exists
        public bool IsUserExists(string userLogin)
        {
            try
            {
                bool result = _dbContext.Users.AsNoTracking().Any(u => u.Login == userLogin);
                
                return result;
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
                return false;
            }
        }

        // Method to get all properties of a user
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = _dbContext.Users.AsNoTracking().FirstOrDefault(x => x.Login == userLogin);
                if (user is null)
                {
                    throw new Exception($"User with this login not found\r\n");
                }
                
                var allProperties = typeof(User).GetProperties().Where(x => x.Name != nameof(User.Login));
                var resultProperties = new List<UserProperty>();

                foreach (var item in allProperties)
                {
                    resultProperties.Add(new UserProperty(item.Name, item.GetValue(user)?.ToString() ?? ""));
                }

                return resultProperties;
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
                return null!;
            }
        }

        // Method to get all permissions
        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {

                var requestRights = _dbContext.RequestRights.AsNoTracking().Select(x => new Permission(x.Id!.Value.ToString() ?? "", x.Name, string.Empty)).ToList();
                var itRoles = _dbContext.ITRoles.AsNoTracking().Select(x => new Permission(x.Id!.Value.ToString() ?? "", x.Name, string.Empty)).ToList();
                var resultPermissions = requestRights.Union(itRoles);

                return resultPermissions;
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
                return null!;
            }
        }

        // Method to add permissions to a user
        public void AddUserPermissions(string userLogin, IEnumerable<string> RightID)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                    throw new Exception($"User with this login not found");

                if (!RightID.Any())
                    throw new Exception($"List of permissions is not specified");

                foreach (var item in RightID)
                {
                    var rightId = item.Split(':');
                    if (!rightId.Any() || rightId.Length != 2)
                    {
                        Logger.Warn($"AddUserPermissions - Warning\r\nRole/Permission: {item} has an incorrect format and will be skipped");
                        continue;
                    }

                    if (rightId[0].Equals(_itRoleRightGroupName))
                    {
                        if (!_dbContext.UserITRoles.Any(x => x.RoleId == Convert.ToInt32(rightId[1].Trim()) && x.UserId == userLogin))
                        {
                            _dbContext.UserITRoles.Add(new() { RoleId = Convert.ToInt32(rightId[1].Trim()), UserId = userLogin });
                        }
                        else
                        {
                            Logger.Warn($"AddUserPermissions - Warning\r\nRole: {rightId[1].Trim()} already exists and will be skipped");
                        }
                    }
                    else
                    {
                        if (!_dbContext.UserRequestRights.Any(x => x.RightId == Convert.ToInt32(rightId[1].Trim()) && x.UserId == userLogin))
                        {
                            _dbContext.UserRequestRights.Add(new() { RightId = Convert.ToInt32(rightId[1].Trim()), UserId = userLogin });
                        }
                        else
                        {
                            Logger.Warn($"AddUserPermissions - Warning\r\nPermission: {rightId[1].Trim()} already exists and will be skipped");
                        }
                    }
                }
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
            }
        }

        // Method to remove permissions from a user
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> RightID)
        {
            try
            {
                var user = _dbContext.Users.AsNoTracking().FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                    throw new Exception($"User with this login not found");

                if (!RightID.Any())
                    throw new Exception($"List of permissions is not specified");

                foreach (var item in RightID)
                {
                    var rightId = item.Split(':');
                    if (!rightId.Any() || rightId.Length != 2)
                    {
                        Logger.Warn($"RemoveUserPermissions - Warning\r\nRole/Permission: {item} has an incorrect format and will be skipped");
                        continue;
                    }
                    if (rightId[0].Equals(_itRoleRightGroupName))
                    {
                        if (_dbContext.UserITRoles.Any(x => x.RoleId == Convert.ToInt32(rightId[1].Trim()) && x.UserId == userLogin))
                        {
                            _dbContext.UserITRoles.Remove(new UserITRole() { RoleId = Convert.ToInt32(rightId[1]), UserId = userLogin });
                        }
                        else
                        {
                            Logger.Warn($"RemoveUserPermissions - Warning\r\nRole: {rightId[1].Trim()} not found");
                        }
                    }
                    else
                    {
                        if (_dbContext.UserRequestRights.Any(x => x.RightId == Convert.ToInt32(rightId[1].Trim()) && x.UserId == userLogin))
                        {
                            _dbContext.UserRequestRights.Remove(new UserRequestRight() { RightId = Convert.ToInt32(rightId[1]), UserId = userLogin });
                        }
                        else
                        {
                            Logger.Warn($"RemoveUserPermissions - Warning\r\nPermission: {rightId[1].Trim()} not found");
                        }
                    }
                }
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
            }
        }

        // Method to get permissions of a user
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                var user = _dbContext.Users.AsNoTracking().FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                    throw new Exception($"User with this login not found");

                var userRequestRights = _dbContext.UserRequestRights.AsNoTracking()
                    .Where(x => x.UserId == userLogin)
                    .Select(x => x.RightId.ToString());

                var userItRoles = _dbContext.UserITRoles.AsNoTracking()
                    .Where(x => x.UserId == userLogin)
                    .Select(x => x.RoleId.ToString());

                var resultPermissions = userRequestRights.Union(userItRoles);
                
                return resultPermissions;
            }
            catch (Exception ex)
            {
                _excep = ex;
                Logger.Error($"{_excep.Message}\r\n\r\n{_excep.StackTrace}");
                return null!;
            }
        }
    }
}
