using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Connector.Helpers;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DbContextFactory? _contextFactory;
        private string? _provider;

        public required ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            var innnerConnString = connectionString.GetInnerConnectionString();
            _contextFactory = new DbContextFactory(innnerConnString);
            _provider = connectionString.GetProviderName();
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                var userForDb = new User {
                    Login = user.Login
                };

                var password = new Sequrity
                {
                    Password = user.HashPassword,
                    UserId = user.Login
                };

                userForDb.SetPropertiesToUser(user.Properties);

                using var context = GetDataContext();
                context.Users.Add(userForDb);
                context.Passwords.Add(password);
                context.SaveChanges();

                Logger.Debug($"Created user with login {user.Login}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while creating a user. \nError: {ex}");
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return UserHelper.AllProperties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                using var context = GetDataContext();

                var user = context.Users.Find(userLogin);

                if (user == null)
                    return Array.Empty<UserProperty>();

                return new UserProperty[]
                {
                    new("firstName", user.FirstName),
                    new("lastName", user.LastName),
                    new("middleName", user.MiddleName),
                    new("telephoneNumber", user.TelephoneNumber),
                    new("isLead", user.IsLead.ToString())
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while getting user properties. \nError: {ex}");
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                using var context = GetDataContext();

                var user = context.Users.Find(userLogin);

                return user != null;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while checking user existance. \nError: {ex}");
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                using var context = GetDataContext();

                var user = context.Users.Find(userLogin) 
                    ?? throw new Exception($"User with login {userLogin} not found.");
                
                user.SetPropertiesToUser(properties);

                context.Users.Update(user);
                context.SaveChanges();
                Logger.Debug($"User with login {user.Login} updated.");
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while updating user properties. \nError: {ex}");
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                using var context = GetDataContext();

                var itRolePermisions = context.ITRoles
                    .Select(x => new Permission(x.Id.ToString() ?? "", x.Name, string.Empty))
                    .ToList();

                var requestRightPermisions = context.RequestRights
                    .Select(x => new Permission(x.Id.ToString() ?? "", x.Name, string.Empty))
                    .ToList();

                return itRolePermisions.Concat(requestRightPermisions).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while getting all permissions. \nError: {ex}");
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                using var context = GetDataContext();

                var user = context.Users.Find(userLogin) 
                    ?? throw new Exception($"User with login {userLogin} not found.");

                var permissions = rightIds.Select(rightId => rightId.ExtractPermissionIdAndType());

                var userITRoles = permissions
                    .Where(x => x.Type == PermissionType.ItRole)
                    .Select(x => new UserITRole() { RoleId = x.Id, UserId = userLogin })
                    .ToList();

                var userRequestRights = permissions
                    .Where(x => x.Type == PermissionType.RequestRight)
                    .Select(x => new UserRequestRight() { RightId = x.Id, UserId = userLogin })
                    .ToList();

                context.UserITRoles.AddRange(userITRoles);
                context.UserRequestRights.AddRange(userRequestRights);
                context.SaveChanges();

                Logger.Debug($"Some permissions added for the user with the login {user.Login}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while adding user permissions. \nError: {ex}");
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                using var context = GetDataContext();

                var user = context.Users.Find(userLogin)
                    ?? throw new Exception($"User with login {userLogin} not found.");

                var permissions = rightIds.Select(rightId => rightId.ExtractPermissionIdAndType());

                var userITRoles = permissions
                    .Where(x => x.Type == PermissionType.ItRole)
                    .Select(x => new UserITRole() { RoleId = x.Id, UserId = userLogin })
                    .ToList();

                var userRequestRights = permissions
                    .Where(x => x.Type == PermissionType.RequestRight)
                    .Select(x => new UserRequestRight() { RightId = x.Id, UserId = userLogin })
                    .ToList();

                context.UserITRoles.RemoveRange(userITRoles);
                context.UserRequestRights.RemoveRange(userRequestRights);
                context.SaveChanges();

                Logger.Debug($"Some user permissions with the login {user.Login} was removed.");
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while removing user permissions. \nError: {ex}");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                using var context = GetDataContext();

                var user = context.Users.Find(userLogin)
                    ?? throw new Exception($"User with login {userLogin} not found.");

                var itRolesPermissions = context.UserITRoles
                    .Where(x => x.UserId == userLogin)
                    .Select(x => $"Role:{x.RoleId}")
                    .ToList();

                var requestRightsPermissions = context.UserRequestRights
                    .Where(x => x.UserId == userLogin)
                    .Select(x => $"Request:{x.RightId}")
                    .ToList();

                return itRolesPermissions.Concat(requestRightsPermissions).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while getting user permissions. \nError: {ex}");
                throw;
            }
        }

        private DataContext GetDataContext()
        {
            if (_contextFactory == null || _provider == null)
            {
                Logger.Error($"Before using {nameof(ConnectorDb)}, you must call the {nameof(StartUp)} method to configure it.");
                throw new Exception("StartUp method wasn't called.");
            }

            return _contextFactory.GetContext(_provider);
        }
    }
}