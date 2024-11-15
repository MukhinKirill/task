using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.DbCommon.DbModels;
using Microsoft.IdentityModel.Tokens;
using Task.Connector.Infrastructure.Converters;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _dbContext;
        private List<Property> _clientUserPropertiesFormatted;
        private List<Permission> _allSystemPermissions;
        private InternalUserToClientUserConverter _internalToClientUserConverter;
        private PropertyInfo[] _clientUserModelProperties;
        public void StartUp(string connectionString)
        {
            if(string.IsNullOrEmpty(connectionString))
            {
                string errorMessage = $"{DateTime.Now} - Failed to start up connector: Empty connection string";
                //Logger.Error(errorMessage);
                throw new ArgumentNullException(errorMessage);
            }

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            
            if(connectionString.Contains("SqlServer"))
            {
                optionsBuilder.UseSqlServer(connectionString);
                //Logger.Debug($"{DateTime.Now} - Using SQL server database provider");
            } 
            // Dirty hacky workaround but im losing my mind here lmao
            if(connectionString.Contains("postgres"))
            {
                optionsBuilder.UseNpgsql(connectionString);
                //Logger.Debug($"{DateTime.Now} - Using Npgsql database provider");
            } 
            try
            {
                _dbContext = new DataContext(optionsBuilder.Options);
                //Logger.Debug($"{DateTime.Now} - Database context created");
            }
            catch (Exception e)
            {
                //Logger.Error($"{DateTime.Now} - Failed to create dbcontext : {e.Message}");
                throw;
            }
            
            // Caching property info here because GetUserProperties is slow enough already
            _clientUserModelProperties = typeof(User).GetProperties();

            // This might need to be different if i need the db's column names instead of the model property names
            _clientUserPropertiesFormatted = typeof(User).GetProperties().
                Select(prop => new Property(prop.Name, prop.Name)).
                ToList();
            
            _clientUserPropertiesFormatted.Add(new Property("Password", "Password"));

            // Set up permissions

            var itRoles = _dbContext.ITRoles.Select(
                role => new Permission(
                    "role-"+role.Id,
                    role.Name,
                    string.Empty
                )
            ).ToList();

            var requestRights = _dbContext.RequestRights.Select(
                requestRight => new Permission(
                    "right-"+requestRight.Id,
                    requestRight.Name,
                    string.Empty
                )
            ).ToList();

            _allSystemPermissions = itRoles.Concat(requestRights).ToList();
            
        }
        // TODO logging, error handling, see if Sequrity.id auto generates on the db end
        public void CreateUser(UserToCreate user)
        {
            var clientUser = _internalToClientUserConverter.Convert(user);
            var passwordEntry = new Sequrity();
            passwordEntry.UserId = user.Login;
            passwordEntry.Password = user.HashPassword;
            
            _dbContext.Users.Add(clientUser);
            _dbContext.Passwords.Add(passwordEntry);
            
            Logger.Debug($"{DateTime.Now} - Created user {user.Login}");

            _dbContext.SaveChanges();
        }
        public IEnumerable<Property> GetAllProperties()
        {
            if(!_clientUserPropertiesFormatted.IsNullOrEmpty()) return _clientUserPropertiesFormatted;
            string errorMessage = $"{DateTime.Now} - The list of user properties is null or empty, something went wrong on startup, or startup was not called";
            Logger.Error(errorMessage);
            throw new Exception(errorMessage);
        }
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _dbContext.Users.Find(userLogin);
            if(user is null)
            {
                Logger.Warn($"{DateTime.Now} - Could not find requested user with login {userLogin}");
                return Enumerable.Empty<UserProperty>();
            } 
            var password = _dbContext.Passwords.FirstOrDefault(pass => pass.UserId == userLogin);

            List<UserProperty> properties = new(6);

            foreach (var property in _clientUserModelProperties)
            {
                var name = property.Name;

                var value = property.GetValue(user)?.ToString() ?? string.Empty;

                properties.Add(new UserProperty(name, value));
            }

            properties.Add(new UserProperty("Password", password!.Password));

            return properties;
        }
        public bool IsUserExists(string userLogin)
        {
            return _dbContext.Users.Find(userLogin) is not null;
        }
        // This should handle edge cases if we care about our database (we should, but I don't atm)
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            // I don't use IsUserExists because I don't want to query twice
            var user = _dbContext.Users.Find(userLogin);

            if(user is null)
            {
                Logger.Warn($"{DateTime.Now} - Could not update properties of user with login {userLogin}: User not found");
                return;
            }

            _internalToClientUserConverter.ParseAndSetUserProperties(ref user, properties);

            var passwordProp = properties.Where(p => p.Name == "Password").First();

            if(passwordProp is not null && !string.IsNullOrEmpty(passwordProp.Value) && !string.IsNullOrWhiteSpace(passwordProp.Value))
            {
                _dbContext.Passwords
                    .Where(p => p.UserId == userLogin)
                    .ExecuteUpdate(sp => sp.SetProperty(
                        pass => pass.Password, passwordProp.Value
                    ));
            } else if(passwordProp is not null && (string.IsNullOrEmpty(passwordProp.Value) || !string.IsNullOrWhiteSpace(passwordProp.Value)))
            {
                string errorMessage = $"{DateTime.Now} - Could not update password of user with login {userLogin}: User password property can not be null, empty or whitespace";
                Logger.Error(errorMessage);
                throw new ArgumentNullException(errorMessage);
            }

            _dbContext.SaveChanges();

        }
        public IEnumerable<Permission> GetAllPermissions()
        {
            return _allSystemPermissions;
        }
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var roles = from role in _dbContext.ITRoles
                        join userRole in _dbContext.UserITRoles
                        on role.Id equals userRole.RoleId
                        where userRole.UserId == userLogin
                        select role.Name;
        
            var permissions = from permission in _dbContext.RequestRights
                        join userPermission in _dbContext.UserRequestRights
                        on permission.Id equals userPermission.RightId
                        where userPermission.UserId == userLogin
                        select permission.Name;

            return roles.Concat(permissions).ToList();
        }
        public Integration.Data.Models.ILogger Logger { get; set; }
    }
}