using Microsoft.EntityFrameworkCore;
using Task.Connector.Data;
using Task.Connector.Data.Entities;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectionInfo
    {
        public string ConnectionString { get; private set; }
        public string Provider { get; private set; }
        public string SchemaName { get; private set; }

        public static ConnectionInfo Parse(string input)
        {
            var connectionInfo = new ConnectionInfo();

            string[] parts = input.Split("\'");
            connectionInfo.ConnectionString = parts[1];
            connectionInfo.Provider = parts[3];
            connectionInfo.SchemaName = parts[5];

            return connectionInfo;
        }
    }

    public class ConnectorDb : IConnector
    {
        private ConnectionInfo _connectionInfo;
        private TaskDbContext _dbContext;

        public void StartUp(string connectionString)
        {
            try
            {
                _connectionInfo = ConnectionInfo.Parse(connectionString);
                var optionsBuilder = new DbContextOptionsBuilder<TaskDbContext>();
                optionsBuilder.UseNpgsql(_connectionInfo.ConnectionString);

                // Since the unfolded scheme does not include the first 19 characters, we start at character 19.
                var scheme = _connectionInfo.SchemaName.Substring(19);
                _dbContext = new TaskDbContext(optionsBuilder.Options, scheme);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                Logger.Debug($"Start creating user");
                using var transaction = _dbContext.Database.BeginTransaction();
                Logger.Debug("Transaction started for user creation.");

                var newUser = new User
                {
                    Login = user.Login,
                    FirstName = GetProperty(user.Properties, "FirstName") ?? String.Empty,
                    LastName = GetProperty(user.Properties, "LastName") ?? String.Empty,
                    MiddleName = GetProperty(user.Properties, "MiddleName") ?? String.Empty,
                    TelephoneNumber = GetProperty(user.Properties, "TelephoneNumber") ?? String.Empty,
                    IsLead = bool.TryParse(GetProperty(user.Properties, "IsLead"), out bool isLead) && isLead
                };

                var password = new Password
                {
                    PasswordValue = user.HashPassword,
                    UserId = user.Login
                };

                _dbContext.User.Add(newUser);
                _dbContext.Passwords.Add(password);
                _dbContext.SaveChanges();

                Logger.Debug("Transaction complete");
                transaction.Commit();
                Logger.Debug("User created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred while creating user: {ex.Message}");
                Logger.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private string? GetProperty(IEnumerable<UserProperty> properties, string name)
        {
            return properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Starting to get user properties");
            var properties = typeof(User)
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Select(prop => new Property(prop.Name, $"This is user property: {prop.Name}"))
                .ToList();

            var loginProperty = properties.FirstOrDefault(p => p.Name == "Login");

            if (loginProperty == null) return properties;
            properties.Add(new Property("Password", $"This is user password property: {nameof(Password)}"));
            properties.Remove(loginProperty);

            Logger.Debug($"Returning {properties.Count} user properties");
            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug("Starting to get properties for user");
            var user = _dbContext.User.AsNoTracking().FirstOrDefault(user => user.Login == userLogin);

            if (user == null)
            {
                Logger.Warn($"User with login {userLogin} not found");
                return Array.Empty<UserProperty>();
            }

            Logger.Debug($"User {userLogin} found. Preparing property list.");
            var properties = new List<UserProperty>
            {
                new("firstName", user.FirstName),
                new("lastName", user.LastName),
                new("middleName", user.MiddleName),
                new("telephoneNumber", user.TelephoneNumber),
                new("isLead", user.IsLead.ToString())
            };

            Logger.Debug($"Created property list with {properties.Count} items");
            return properties;
        }

        public bool IsUserExists(string userLogin)
        {
            Logger.Debug("Checking is user exist");
            var user = _dbContext.User.AsNoTracking().FirstOrDefault(user => user.Login == userLogin);

            if (user == null)
            {
                Logger.Warn("User with given login not exist.");
                return false;
            }

            Logger.Debug("User exist");
            return true;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug($"Starting to update properties for user");
            var user = _dbContext.User.FirstOrDefault(user => user.Login == userLogin);
            if (user == null)
            {
                Logger.Warn($"User not found. Update aborted.");
                return;
            }

            if (!properties.Any())
            {
                Logger.Warn($"No properties provided for update. Update aborted.");
                return;
            }

            Logger.Debug($"User found. Preparing to update properties.");
            var propertyMap = new Dictionary<string, Action<string>>
            {
                ["firstname"] = value => user.FirstName = value,
                ["middlename"] = value => user.MiddleName = value,
                ["lastname"] = value => user.LastName = value,
                ["telephonenumber"] = value => user.TelephoneNumber = value,
                ["islead"] = value => user.IsLead = bool.TryParse(value, out bool result) && result
            };

            Logger.Debug($"Property map created with {propertyMap.Count} mappings.");
            int updatedPropertiesCount = 0;
            foreach (var property in properties)
            {
                if (propertyMap.TryGetValue(property.Name.ToLower(), out var action))
                {
                    Logger.Debug($"Updating property: {property.Name} = {property.Value}");
                    action(property.Value);
                    updatedPropertiesCount++;
                }
                else
                {
                    Logger.Warn($"Unknown property: {property.Name}. Skipping.");
                }
            }

            Logger.Debug($"Updated {updatedPropertiesCount} properties for user {userLogin}");

            _dbContext.User.Update(user);
            _dbContext.SaveChanges();

            Logger.Debug($"Finished updating properties for user {userLogin}");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var permissions = new List<Permission>();
                var requestRights = _dbContext.RequestRight.AsNoTracking().ToList();
                var itRoles = _dbContext.ItRole.AsNoTracking().ToList();

                foreach (var right in requestRights)
                {
                    var permission = new Permission(right.Id.ToString(), right.Name, $"Permission: {right.Name}");
                    permissions.Add(permission);
                }

                foreach (var role in itRoles)
                {
                    var permission = new Permission(role.Id.ToString(), role.Name, $"Permission: {role.Name}");
                    permissions.Add(permission);
                }

                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"ERROR: occurred while retrieving permissions\n {ex}");
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Adding permissions for user");

            var user = _dbContext.User.AsNoTracking().FirstOrDefault(user => user.Login == userLogin);
            if (user == null)
            {
                Logger.Error($"User with login not found.");
                throw new ArgumentException($"User with login not found.", nameof(userLogin));
            }

            var rolesToAdd = new List<UserItRole>();
            var userRequestRights = new List<UserRequestRight>();

            foreach (var right in rightIds)
            {
                var parts = right.Split(':', 2);
                if (parts.Length != 2 || !int.TryParse(parts[1], out int id))
                {
                    Logger.Warn($"Invalid right format: {right}");
                    continue;
                }

                switch (parts[0].ToLower())
                {
                    case "role":
                        rolesToAdd.Add(new UserItRole
                        {
                            UserId = userLogin,
                            RoleId = id
                        });
                        break;

                    case "request":
                        var requestRight = _dbContext.UserRequestRight.FirstOrDefault(urr => urr.RightId == id);
                        if (requestRight != null)
                        {
                            userRequestRights.Add(requestRight);
                        }
                        else
                        {
                            Logger.Warn($"Request right not found: {id}");
                        }

                        break;

                    default:
                        Logger.Warn($"Unknown right type: {parts[0]}");
                        break;
                }
            }

            if (rolesToAdd.Any())
            {
                _dbContext.UserItRole.AddRange(rolesToAdd);
                Logger.Debug($"Adding {rolesToAdd.Count} roles for user");
            }

            if (userRequestRights.Any())
            {
                _dbContext.UserRequestRight.AddRange(userRequestRights);
                Logger.Debug($"Adding {userRequestRights.Count} request rights for user");
            }

            if (rolesToAdd.Any() || userRequestRights.Any())
            {
                try
                {
                    _dbContext.SaveChanges();
                    Logger.Debug($"Successfully saved changes for user");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error saving changes for user: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Logger.Warn($"No permissions added for user");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Removing permissions for user");

            var user = _dbContext.User.FirstOrDefault(user => user.Login == userLogin);
            if (user == null)
            {
                Logger.Error($"User with login not found.");
                throw new ArgumentException($"User with login not found.", nameof(userLogin));
            }

            var rightsToRemove = new List<UserRequestRight>();
            var rolesToRemove = new List<UserItRole>();

            foreach (var right in rightIds)
            {
                var parts = right.Split(':', 2);
                if (parts.Length != 2 || !int.TryParse(parts[1], out int id))
                {
                    Logger.Warn($"Invalid right format: {right}");
                    continue;
                }

                switch (parts[0].ToLower())
                {
                    case "request":
                        var requestRight = _dbContext.UserRequestRight
                            .FirstOrDefault(urr => urr.RightId == id && urr.UserId == userLogin);
                        if (requestRight != null)
                        {
                            rightsToRemove.Add(requestRight);
                        }
                        else
                        {
                            Logger.Warn($"Request right not found for user: {id}");
                        }

                        break;

                    case "role":
                        var roleRight = _dbContext.UserItRole
                            .FirstOrDefault(uir => uir.RoleId == id && uir.UserId == userLogin);
                        if (roleRight != null)
                        {
                            rolesToRemove.Add(roleRight);
                        }
                        else
                        {
                            Logger.Warn($"Role right not found for user: {id}");
                        }

                        break;

                    default:
                        Logger.Warn($"Unknown right type: {parts[0]}");
                        break;
                }
            }

            if (rightsToRemove.Any())
            {
                _dbContext.UserRequestRight.RemoveRange(rightsToRemove);
                Logger.Debug($"Removing {rightsToRemove.Count} request rights for user {userLogin}");
            }

            if (rolesToRemove.Any())
            {
                _dbContext.UserItRole.RemoveRange(rolesToRemove);
                Logger.Debug($"Removing {rolesToRemove.Count} roles for user {userLogin}");
            }

            if (rightsToRemove.Any() || rolesToRemove.Any())
            {
                try
                {
                    _dbContext.SaveChanges();
                    Logger.Debug($"Successfully saved changes for user {userLogin}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error saving changes for user {userLogin}: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Logger.Warn($"No permissions removed for user {userLogin}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Getting permissions for user: {userLogin}");

            var user = _dbContext.User.AsNoTracking().FirstOrDefault(user => user.Login == userLogin);
            if (user == null)
            {
                Logger.Warn($"User with login '{userLogin}' not found.");
                return Array.Empty<string>();
            }

            try
            {
                var userRequestRight = _dbContext.UserRequestRight.AsNoTracking()
                    .Where(urr => urr.UserId == user.Login)
                    .Include(userRequestRight => userRequestRight.Right)
                    .ToList()
                    .Select(right => right.Right.Name);

                var userItRole = _dbContext.UserItRole.AsNoTracking()
                    .Where(uir => uir.UserId == user.Login)
                    .Include(uir => uir.Role)
                    .ToList()
                    .Select(role => role.Role.Name);

                var allPermissions = userRequestRight.Concat(userItRole).ToList();

                Logger.Debug($"Retrieved {allPermissions.Count} permissions for user {userLogin}");

                return allPermissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred while retrieving permissions for user {userLogin}: {ex.Message}");
                throw;
            }
        }

        public ILogger Logger { get; set; }
    }
}