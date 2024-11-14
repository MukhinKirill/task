using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using System.Data.Common;
using Task.Integration.Data.DbCommon.DbModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Task.Connector
{
    public partial class ConnectorDb : IConnector
    {
        private DataContext? _dataContext;
        public ILogger Logger { get; set; } = new NullLogger();

        private static string GetConnectionProperty(string connectionString, string propName)
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };
            if (!builder.TryGetValue(propName, out object? res))
                throw new Exception($"{propName} not found");

            return (string)res;
        }
        public void StartUp(string connectionString)
        {
            try
            {
                var provider = GetConnectionProperty(connectionString, "Provider");
                provider = LC.PROVIDERS.FirstOrDefault(x => provider.ToLower().Contains(x.Value)).Key;
                var dbConnectString = GetConnectionProperty(connectionString, "ConnectionString");

                var factory = new DbContextFactory(dbConnectString);
                _dataContext = factory.GetContext(provider);
                Logger.Debug($"Connected to {provider}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error starting up: {ex.Message}");
            }
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                var userEntity = new User()
                {
                    Login = user.Login,
                };
                var password = new Sequrity()
                {
                    UserId = userEntity.Login,
                    Password = user.HashPassword
                };

                foreach (var propInfo in typeof(User).GetProperties())
                {
                    if (LC.NOT_PROPS.Contains($"{propInfo.ReflectedType?.Name}-{propInfo.Name}")) continue;
                    TypeConverter converter = TypeDescriptor.GetConverter(propInfo.PropertyType);
                    var prop = user.Properties.FirstOrDefault(x => x.Name == propInfo.Name);
                    object? value;

                    if (prop == null && propInfo.PropertyType.Name != "String")
                        value = Activator.CreateInstance(propInfo.PropertyType);
                    else if (prop == null && propInfo.PropertyType.Name == "String")
                        value = string.Empty;
                    else
                        value = converter.ConvertFrom(prop.Value);

                    propInfo.SetValue(userEntity, value);
                }

                _dataContext.Users.Add(userEntity);
                _dataContext.Passwords.Add(password);
                _dataContext.SaveChanges();
                Logger.Debug($"Created user {userEntity.Login}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating user {user.Login}: {ex.Message}");
            }
            
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                IEnumerable<Property> properties =
                typeof(User).GetProperties().Concat(typeof(Sequrity).GetProperties())
                .Where(x => !LC.NOT_PROPS.Contains($"{x.ReflectedType?.Name}-{x.Name}"))
                .Select(x => new Property(x.Name, x.PropertyType.Name));

                Logger.Debug($"Found {properties.Count()} properties");
                return properties;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting properties: {ex.Message}");
                return new List<Property>();
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = _dataContext.Users.FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    return new List<UserProperty>();
                }
                var password = _dataContext.Passwords.FirstOrDefault(x => x.UserId == userLogin);

                IEnumerable<UserProperty> properties =
                    typeof(User).GetProperties()
                    .Where(x => !LC.NOT_PROPS.Contains($"{x.ReflectedType?.Name}-{x.Name}"))
                    .Select(x => new UserProperty(x.Name, x.GetValue(user)?.ToString()))
                    //.Concat(
                    //typeof(Sequrity).GetProperties()
                    //.Where(x => !LC.NOT_PROPS.Contains($"{x.ReflectedType?.Name}-{x.Name}"))
                    //.Select(x => new UserProperty(x.Name, x.GetValue(password)?.ToString()))
                    //)
                    .Where(x => x.Value != null);

                Logger.Debug($"Found {properties.Count()} properties for user {userLogin}");
                return properties;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting properties for user {userLogin}: {ex.Message}");
                return new List<UserProperty>();
            }
        }

        public bool IsUserExists(string userLogin)
        {
            var user = _dataContext.Users.FirstOrDefault(x => x.Login == userLogin);
            Logger.Debug($"Found user {userLogin}: {user != null}");
            return user != null;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = _dataContext.Users.FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    return;
                }
                var password = _dataContext.Passwords.FirstOrDefault(x => x.UserId == userLogin);
                foreach (var prop in properties)
                {
                    var propInfoUser = typeof(User).GetProperties()
                        .FirstOrDefault(x => x.Name == prop.Name && !LC.NOT_PROPS.Contains($"{x.ReflectedType?.Name}-{x.Name}"));
                    propInfoUser?.SetValue(user, prop.Value);
                    var propInfoPassword = typeof(Sequrity).GetProperties()
                        .FirstOrDefault(x => x.Name == prop.Name && !LC.NOT_PROPS.Contains($"{x.ReflectedType?.Name}-{x.Name}"));
                    propInfoPassword?.SetValue(password, prop.Value);
                }
                _dataContext.SaveChanges();
                Logger.Debug($"Updated properties for user {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating properties for user {userLogin}: {ex.Message}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            List<Permission> permissions = new List<Permission>();
            permissions.AddRange(_dataContext.ITRoles.Select(x => new Permission(x.Id.ToString(), x.Name, x.CorporatePhoneNumber)));
            permissions.AddRange(_dataContext.RequestRights.Select(x => new Permission(x.Id.ToString(), x.Name, string.Empty)));
            Logger.Debug($"Found {permissions.Count()} permissions");
            
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = _dataContext.Users.FirstOrDefault(x => x.Login == userLogin);
            if (user == null)
            {
                Logger.Warn($"User {userLogin} not found");
                return;
            }
            foreach (var rightId in rightIds)
            {
                var matches = ParsePermission().Matches(rightId);
                var type = matches[0].Value;
                var id = matches[1].Value;
                if (type == LC.itRoleRightGroupName)
                {
                    var role = _dataContext.ITRoles.FirstOrDefault(x => x.Id == int.Parse(id)) ?? throw new Exception($"Role {id} not found");
                    _dataContext.UserITRoles.Add(new UserITRole() { UserId = user.Login, RoleId = int.Parse(id) });
                }
                else if (type == LC.requestRightGroupName ){
                    var right = _dataContext.RequestRights.FirstOrDefault(x => x.Id == int.Parse(id)) ?? throw new Exception($"Right {id} not found");
                    _dataContext.UserRequestRights.Add(new UserRequestRight() { UserId = user.Login, RightId = int.Parse(id) });
                }
            }

            _dataContext.SaveChanges();
            Logger.Debug($"Added {rightIds.Count()} permissions for user {userLogin}");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var user = _dataContext.Users.FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    return;
                }
                foreach (var rightId in rightIds)
                {
                    var matches = ParsePermission().Matches(rightId);
                    var type = matches[0].Value;
                    var id = matches[1].Value;
                    if (type == LC.itRoleRightGroupName)
                    {
                        var role = _dataContext.UserITRoles.FirstOrDefault(x => x.UserId == user.Login && x.RoleId == int.Parse(id)) ?? throw new Exception($"Role {id} not found");
                        _dataContext.UserITRoles.Remove(role);
                    }
                    else if (type == LC.requestRightGroupName)
                    {
                        var right = _dataContext.UserRequestRights.FirstOrDefault(x => x.UserId == user.Login && x.RightId == int.Parse(id)) ?? throw new Exception($"Right {id} not found");
                        _dataContext.UserRequestRights.Remove(right);
                    }
                }
                _dataContext.SaveChanges();
                Logger.Debug($"Removed {rightIds.Count()} permissions for user {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while removing permissions for user {userLogin}: {ex.Message}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                List<string> permissions = new List<string>();
                var user = _dataContext.Users.FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                {
                    Logger.Warn($"User {userLogin} not found");
                    return permissions;
                }
                permissions.AddRange(_dataContext.UserITRoles.Where(x => x.UserId == user.Login).Select(x => $"{LC.itRoleRightGroupName}:{x.RoleId}"));
                permissions.AddRange(_dataContext.UserRequestRights.Where(x => x.UserId == user.Login).Select(x => $"{LC.requestRightGroupName}:{x.RightId}"));
                Logger.Debug($"Found {permissions.Count()} permissions for user {userLogin}");
                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting permissions for user {userLogin}: {ex.Message}");
            }
            return new List<string>();
        }

        [GeneratedRegex("[\\w\\s]+")]
        private static partial Regex ParsePermission();
    }
}