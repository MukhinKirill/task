using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Model;
using static Task.Connector.Context.Context;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }

        // context
        private DatabaseContext _context;

        public void StartUp(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            optionsBuilder.UseSqlServer(connectionString);
            _context = new DatabaseContext(optionsBuilder.Options);

            Logger.Debug("Database context initialized");
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                var newUser = new User { Login = user.Login };
                
                _context.Users.Add(newUser);
                _context.SaveChanges();
               
                Logger.Debug($"User {user.Login} created");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create user {user.Login}: {ex.Message}");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var properties = _context.Users.SelectMany(u => u.UserRequestRights)
                                               .Select(ur => new Property(ur.RequestRight.Name, "Description"))
                                               .Distinct()
                                               .ToList();

                Logger.Debug("Retrieved all properties");
                
                return properties;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to retrieve all properties: {ex.Message}");
                
                return Enumerable.Empty<Property>();
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = _context.Users.Include(u => u.UserRequestRights)
                                         .ThenInclude(ur => ur.RequestRight)
                                         .FirstOrDefault(u => u.Login == userLogin);

                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    
                    return Enumerable.Empty<UserProperty>();
                }

                var userProperties = user.UserRequestRights.Select(ur => new UserProperty(ur.RequestRight.Name, "Value"))
                                                           .ToList();

                Logger.Debug($"Retrieved properties for user {userLogin}");
                
                return userProperties;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to retrieve properties for user {userLogin}: {ex.Message}");
                
                return Enumerable.Empty<UserProperty>();
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                var exists = _context.Users.Any(u => u.Login == userLogin);
                
                Logger.Debug($"Checked existence for user {userLogin}: {exists}");
                
                return exists;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to check existence for user {userLogin}: {ex.Message}");
                
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = _context.Users.Include(u => u.UserRequestRights)
                                         .FirstOrDefault(u => u.Login == userLogin);

                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    
                    return;
                }

                foreach (var property in properties)
                {
                    var userRequestRight = user.UserRequestRights.FirstOrDefault(ur => ur.RequestRight.Name == property.Name);
                    
                    if (userRequestRight != null)
                    {
                        userRequestRight.Value = property.Value;
                    }
                }

                _context.SaveChanges();

                Logger.Debug($"Updated properties for user {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to update properties for user {userLogin}: {ex.Message}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var permissions = _context.RequestRights.Select(rr => new Permission(rr.Id.ToString(), rr.Name, "RequestRight Description"))
                                                        .Union(_context.ItRoles.Select(ir => new Permission(ir.Id.ToString(), ir.Name, "ItRole Description")))
                                                        .ToList();
                
                Logger.Debug("Retrieved all permissions");
                
                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to retrieve all permissions: {ex.Message}");
                
                return Enumerable.Empty<Permission>();
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
                
                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    
                    return;
                }

                foreach (var rightId in rightIds)
                {
                    if (int.TryParse(rightId, out int id))
                    {
                        var requestRight = _context.RequestRights.Find(id);
                        
                        if (requestRight != null)
                        {
                            user.UserRequestRights.Add(new UserRequestRight { UserId = user.Id, RequestRightId = id });
                        }
                    }
                }

                _context.SaveChanges();
                
                Logger.Debug($"Added permissions to user {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to add permissions to user {userLogin}: {ex.Message}");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var user = _context.Users.Include(u => u.UserRequestRights)
                                         .FirstOrDefault(u => u.Login == userLogin);

                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    
                    return;
                }

                foreach (var rightId in rightIds)
                {
                    if (int.TryParse(rightId, out int id))
                    {
                        var userRequestRight = user.UserRequestRights.FirstOrDefault(ur => ur.RequestRightId == id);
                        
                        if (userRequestRight != null)
                        {
                            user.UserRequestRights.Remove(userRequestRight);
                        }
                    }
                }

                _context.SaveChanges();

                Logger.Debug($"Removed permissions from user {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to remove permissions from user {userLogin}: {ex.Message}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                var user = _context.Users.Include(u => u.UserRequestRights)
                                         .ThenInclude(ur => ur.RequestRight)
                                         .FirstOrDefault(u => u.Login == userLogin);

                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    
                    return Enumerable.Empty<string>();
                }

                var permissions = user.UserRequestRights.Select(ur => ur.RequestRight.Name).ToList();
                
                Logger.Debug($"Retrieved permissions for user {userLogin}");
                
                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to retrieve permissions for user {userLogin}: {ex.Message}");
                
                return Enumerable.Empty<string>();
            }
        }
    }
}