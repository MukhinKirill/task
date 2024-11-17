using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Task.Connector.Contexts;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }

        public async void StartUp(string connectionString)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                bool isAvalaible = await db.Database.CanConnectAsync();
                if (isAvalaible) Logger?.Debug("Database context initialized.");
                else Logger?.Debug("Database context couldn't be initialized.");
            }
        }

        public void CreateUser(UserToCreate user)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                if (IsUserExists(user.Login))
                {
                    Logger.Warn($"The user with login {user.Login} already exists.");
                    return;
                }

                var newUser = new User
                {
                    Login = user.Login,
                    FirstName = user.Properties.FirstOrDefault(p => p.Name == "FirstName")?.Value ?? "FirstName",
                    LastName = user.Properties.FirstOrDefault(p => p.Name == "LastName")?.Value ?? "LastName",
                    MiddleName = user.Properties.FirstOrDefault(p => p.Name == "MiddleName")?.Value ?? "MiddleName",
                    TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "TelephoneNumber")?.Value ?? "TelephoneNumber",
                    IsLead = user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value == "false",
                };

                db.Users.Add(newUser);

                var password = new Password
                {
                    UserId = user.Login,
                    Password1 = user.HashPassword,
                };

                db.Passwords.Add(password);

                db.SaveChanges();

                Logger.Debug($"The new user has been created - {user.Login}");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                PropertyInfo[] properties = typeof(User).GetProperties();

                if (properties == null)
                {
                    Logger?.Error($"Users properties could not be found.");
                    return Enumerable.Empty<Property>();
                }

                var userProperties = properties
                .Select(p => new Property(p.Name.ToString(), "Description"))
                .ToList();


                Logger?.Debug($"Users have the following properties: {userProperties.Count}");
                return userProperties;
            }
        }

        public  IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                if (!IsUserExists(userLogin))
                {
                    Logger.Error($"The user with login {userLogin} could not be found.");
                    return Enumerable.Empty<UserProperty>();
                }

                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);

                List<UserProperty> userPropertyValues = new List<UserProperty>();

                PropertyInfo[] properties = typeof(User).GetProperties();

                foreach (var property in properties)
                {
                    if (property.Name != "Login")
                    {
                        object value = property.GetValue(user);
                        userPropertyValues.Add(new UserProperty(property.Name, value?.ToString()));
                    }
                }

                Logger?.Debug($"{userLogin}: {userPropertyValues.Count}");
                return userPropertyValues;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);
                return user != null;
            }
        }

        public  void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                if (!IsUserExists(userLogin))
                {
                    Logger.Error($"The user with login {userLogin} could not be found.");
                    return;
                }

                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);

                foreach (var property in properties)
                {
                    var userPropertyInfo = user?.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);

                    if (userPropertyInfo != null && userPropertyInfo.CanWrite)
                    {
                        var currentValue = userPropertyInfo.GetValue(user)?.ToString();

                        if (currentValue != property.Value)
                        {
                            userPropertyInfo.SetValue(user, Convert.ChangeType(property.Value, userPropertyInfo.PropertyType)); 
                        }
                    }
                }

                db.SaveChanges(); 

                Logger?.Debug($"The user properties have been updated");
            }
        }

        public  IEnumerable<Permission> GetAllPermissions()
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                List<Permission> permissions = new List<Permission>();

                var requestRights = db.RequestRights
                .Select(rr => new Permission(rr.Id.ToString(), rr.Name, "Description"))
                .ToList();

                permissions.AddRange(requestRights);

                var itRoles = db.ItRoles
                    .Select(ir => new Permission(ir.Id.ToString(), ir.Name, "Description"))
                    .ToList();

                permissions.AddRange(itRoles);
                
                Logger?.Debug($"The permissions have been loaded");

                return permissions;
            }
        }

        public  void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                if (!IsUserExists(userLogin))
                {
                    Logger.Error($"The user with login {userLogin} could not be found.");
                    return;
                }

                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);

                List<int> allRights = new List<int>();
                List<int> allRoles = new List<int>();


                foreach (var rightId in rightIds) 
                {
                    var right = rightId.Split(':');
                    string rightType = right[0];
                    int rightIdNumber = int.Parse(right[1]);

                    if (rightType.StartsWith("Role", StringComparison.OrdinalIgnoreCase))
                    { 
                        allRoles.Add(rightIdNumber);
                    }
                    else if (rightType.StartsWith("Right", StringComparison.OrdinalIgnoreCase))
                    {
                        allRights.Add(rightIdNumber);
                    }
                    else
                    {
                        Logger.Error($"Incorrect data");
                        return;
                    }
                }

                List<int> all_db_rights = db.RequestRights.Select(rr => rr.Id).ToList();
                List<int> all_db_roles = db.ItRoles.Select(rr => rr.Id).ToList();

                if (!(Validation_FK<int>(all_db_rights, allRights, "rightids") || Validation_FK<int>(all_db_roles, allRoles, "roleids")))
                {
                    return;
                }

                foreach (var role in allRoles)
                {
                    var currentRoleIds = db.UserItroles
                            .Where(ur => ur.UserId == userLogin)
                            .Select(ur => ur.RoleId)
                            .ToList();

                    bool roleExists = currentRoleIds.Contains(role);

                    if (!roleExists)
                    {
                        db.UserItroles.Add(new UserItrole { UserId = userLogin, RoleId = role });
                        Logger?.Debug($"Role {role} has been added to user {userLogin}.");
                    }
                    else
                    {
                        Logger?.Warn($"The role {role} for user {userLogin} already exists.");
                    }
                }

                foreach (var right in allRights)
                {
                    var currentRighteIds = db.UserRequestRights
                            .Where(ur => ur.UserId == userLogin)
                            .Select(ur => ur.RightId)
                            .ToList();

                    bool rightExists = currentRighteIds.Contains(right);

                    if (!rightExists)
                    {
                        db.UserRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = right });
                        Logger?.Debug($"Right {right} has been added to user {userLogin}.");
                    }
                    else
                    {
                        Logger?.Warn($"The right {right} for user {userLogin} already exists.");
                    }
                }

                db.SaveChanges();
                Logger?.Debug($"The permissions for user {userLogin} have been added successfully.");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                if (!IsUserExists(userLogin))
                {
                    Logger.Warn($"The user with login {userLogin} could not be found.");
                    return;
                }

                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);

                foreach (var rightId in rightIds)
                {
                    var right = rightId.Split(':');
                    string rightType = right[0];
                    int rightIdNumber = int.Parse(right[1]);

                    if (rightType.StartsWith("Role", StringComparison.OrdinalIgnoreCase))
                    {
                        var currentRoleIds = db.UserItroles
                            .Where(ur => ur.UserId == userLogin)
                            .Select(ur => ur.RoleId)
                            .ToList();

                        bool roleExists = currentRoleIds.Contains(rightIdNumber);

                        if (roleExists)
                        {
                            db.UserItroles.Remove(new UserItrole { UserId = userLogin, RoleId = rightIdNumber });
                            Logger?.Debug($"Role {rightIdNumber} has been removed from user {userLogin}.");
                        }
                        else
                        {
                            Logger?.Warn($"The role {rightIdNumber} for user {userLogin} doen not exist.");
                        }
                    }
                    else
                    {
                        var currentRighteIds = db.UserRequestRights
                            .Where(ur => ur.UserId == userLogin)
                            .Select(ur => ur.RightId)
                            .ToList();

                        bool rightExists = currentRighteIds.Contains(rightIdNumber);

                        if (rightExists)
                        {
                            db.UserRequestRights.Remove(new UserRequestRight { UserId = userLogin, RightId = rightIdNumber });
                            Logger?.Debug($"Right {rightIdNumber} has been removed from user {userLogin}.");
                        }
                        else
                        {
                            Logger?.Warn($"The right {rightIdNumber} for user {userLogin} does not exist.");
                        }
                    }
                }
                db.SaveChanges();
                Logger?.Debug($"The permissions for user {userLogin} have been added successfully.");
            }
        }

        public  IEnumerable<string> GetUserPermissions(string userLogin)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                if (!IsUserExists(userLogin))
                {
                    Logger.Warn($"The user with login {userLogin} could not be found.");
                    return new List<string>(); 
                }

                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);

                var requestRights = db.UserRequestRights
                                      .Where(ur => ur.UserId == userLogin)
                                      .Select(ur => ur.RightId.ToString());

                var itRoles = db.UserItroles
                                 .Where(ur => ur.UserId == userLogin)
                                 .Select(ur => ur.RoleId.ToString());

                var permissions = requestRights.Concat(itRoles).ToList();

                Logger?.Debug($"The user with login {userLogin} has the following permissions: {permissions.Count}");
                return permissions;
            }
        }
        public bool Validation_FK<T>(List<T> db_obj, List<T> data, string entity_name)
        {
            List<string> error_objs = new List<string>();

            foreach (var obj in data)
            {
                if (!db_obj.Contains(obj)) 
                {
                    error_objs.Add(obj.ToString());
                }
            }

            if (error_objs.Count > 0) {
                Logger.Error($"Following {entity_name} not found in: {String.Join(", ", error_objs.ToArray())}");
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}