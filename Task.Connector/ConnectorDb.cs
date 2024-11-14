using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private AppDbContext Context;
        public void StartUp(string connectionString)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            Context = new AppDbContext(options);
            Logger.Debug("Startup - nice");
            
        }

        public void CreateUser(UserToCreate user)
        {
            var userNew = new User()
            {
                Login = user.Login,
                LastName = string.Empty,
                FirstName = string.Empty,
                MiddleName = string.Empty,
                TelephoneNumber = string.Empty,
                IsLead = false
            };

            foreach (var property in user.Properties)
            {
                switch (property.Name)
                {
                    case "FirstName":
                        userNew.FirstName = property.Value;
                        break;

                    case "LastName":
                        userNew.LastName = property.Value;
                        break;

                    case "MiddleName":
                        userNew.MiddleName = property.Value;
                        break;

                    case "TelephoneNumber":
                        userNew.TelephoneNumber = property.Value;
                        break;

                    case "IsLead":
                        
                        if (bool.TryParse(property.Value, out var isLead))
                        {
                            userNew.IsLead = isLead;
                        }
                        break;

                    default:
                        break;
                }
            }
            
            Context.Users.Add(userNew);

            var password = new Password()
            {
                UserId = user.Login,  
                PasswordHash = user.HashPassword              };

            Context.Passwords.Add(password);

            Context.SaveChanges();
            Logger.Debug("User created");
        }

        public IEnumerable<Property> GetAllProperties()
        {
            //Если правильно понял - не обращаем внимание на таблмцу с миграциями
            //Также не до конца понял что должно быть в Value - будет просто строка

            var tableNames = Context.Model.GetEntityTypes()
        .Where(t => t.GetSchema() == "TestTaskSchema")
        .Select(t => t.GetTableName())
        .ToList()
        .Where(t => t != "_MigrationHistory")
        .ToList();
            var properties = tableNames.Select(tableName =>
            {
                var description = $"Table for {tableName}";

                return new Property(tableName, description);
            }).ToList();
            Logger.Debug("Properties showed");
            return properties;

        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = Context.Users
        .FirstOrDefault(u => u.Login == userLogin);

            var userProperties = user.GetType()
        .GetProperties()
        .Where(p => p.CanRead && p.Name != "Login")
        .Select(p => new UserProperty(p.Name, p.GetValue(user)?.ToString() ?? "Clear")) 
        .ToList();
            Logger.Debug("UserProperties showed");
            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            var user = Context.Users
        .FirstOrDefault(u => u.Login == userLogin); 

            return user != null;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = Context.Users
        .FirstOrDefault(u => u.Login == userLogin);

            if (user == null)
            {
                Logger.Error("User null");
            }

            foreach (var property in properties)
            {
                switch (property.Name)
                {
                    case "LastName":
                        user.LastName = property.Value;
                        break;
                    case "FirstName":
                        user.FirstName = property.Value;
                        break;
                    case "MiddleName":
                        user.MiddleName = property.Value;
                        break;
                    case "TelephoneNumber":
                        user.TelephoneNumber = property.Value;
                        break;
                    case "IsLead":
                        
                        if (bool.TryParse(property.Value, out bool isLead))
                        {
                            user.IsLead = isLead;
                        }
                        else
                        {
                            throw new Exception("Invalid value for IsLead");
                        }
                        break;
                    default:
                        Logger.Error("Proprtie not found");
                        break;
                }
            }
            Logger.Debug("PropUpdated");
            Context.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var permissions = new List<Permission>();

            var requestRights = Context.RequestRights
                .Select(rr => new Permission(rr.Id.ToString(), rr.Name, "Description"))
                .ToList();

            var itRoles = Context.ItRoles
                .Select(ir => new Permission(ir.Id.ToString(), ir.Name, "Description"))
                .ToList();

            permissions.AddRange(requestRights);
            permissions.AddRange(itRoles);
            Logger.Debug("Props are goted");
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<int> rightIds)
        {
            var user = Context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger.Debug($"User with login {userLogin} not found.");
            }

            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var rightId in rightIds)
                    {
                        bool isRequestRight = Context.RequestRights.Any(r => r.Id == rightId);

                        if (isRequestRight)
                        {
                            bool existsInUserRequestRights = Context.UserRequestRights
                                .Any(ur => ur.UserId == user.Login && ur.RightId == rightId);

                            if (!existsInUserRequestRights)
                            {
                                var userRequestRight = new UserRequestRight
                                {
                                    UserId = user.Login,
                                    RightId = rightId
                                };
                                Context.UserRequestRights.Add(userRequestRight);
                            }
                        }
                        else
                        {
                            bool existsInUserITRoles = Context.UserITRoles
                                .Any(ur => ur.UserId == user.Login && ur.RoleId == rightId);

                            if (!existsInUserITRoles)
                            {
                                var userItRole = new UserITRole
                                {
                                    UserId = user.Login,
                                    RoleId = rightId
                                };
                                Context.UserITRoles.Add(userItRole);
                            }
                        }
                    }

                    Context.SaveChanges();
                    transaction.Commit();
                    Logger.Debug("Permissions successfully added.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"An error occurred while adding permissions: {ex.Message}");
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<int> rightIds)
        {
            var user = Context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger.Error($"User with login {userLogin} not found.");
            }

            var userId = user.Login;

            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var userRequestRightsToRemove = Context.UserRequestRights
                        .Where(urr => urr.UserId == userId && rightIds.Contains(urr.RightId))
                        .ToList();

                    Context.UserRequestRights.RemoveRange(userRequestRightsToRemove);

                    var userRolesToRemove = Context.UserITRoles
                        .Where(ur => ur.UserId == userId && rightIds.Contains(ur.RoleId))
                        .ToList();

                    Context.UserITRoles.RemoveRange(userRolesToRemove);

                    Context.SaveChanges();
                    transaction.Commit();
                    Logger.Debug("Permissions successfully removed.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"An error occurred while removing permissions: {ex.Message}");
                }
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var user = Context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger.Error("User Null");
            }

            var userId = user.Login;

            var rolePermissions = Context.UserITRoles
                .Where(ur => ur.UserId == userId)
                .Join(Context.ItRoles, ur => ur.RoleId, ir => ir.Id, (ur, ir) => ir)
                .Select(ir => ir.Name)  // или ir.Id, если нужно использовать Id
                .ToList();

            var userRequestRights = Context.UserRequestRights
                .Where(urr => urr.UserId == userId)
                .Join(Context.RequestRights, urr => urr.RightId, rr => rr.Id, (urr, rr) => rr)
                .Select(rr => rr.Name)  // или rr.Id, если нужно использовать Id
                .ToList();

            var allPermissions = rolePermissions.Concat(userRequestRights).Distinct().ToList();
            Logger.Debug("Perms are gotten");
            return allPermissions;
        }

        public ILogger Logger { get; set; }
    }
}