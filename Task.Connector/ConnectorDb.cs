using System.Reflection;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _context;
        private DataManager _dataManager;
        private string _providerName = "POSTGRE";
        static string requestRightGroupName = "Request";
        static string itRoleRightGroupName = "Role";
        static string delimeter = ":";
        public const string PostgreSqlConnectionString = "Host=localhost;Port=5432;Database=testDb;Username=root;Password=root";

        public ConnectorDb() { 

        }
        public void StartUp(string connectionString)
        {
            var dbContextFactory = new DbContextFactory(PostgreSqlConnectionString);
            _context = dbContextFactory.GetContext(_providerName);
            _dataManager = new DataManager(dbContextFactory, _providerName);
        }

        public void CreateUser(UserToCreate user)
        {
              if (_dataManager.GetUser(user.Login) is null)
              {
                User toCreate = new User();
                toCreate.Login = user.Login;

                //Filling default values. Let it be a little js like
                toCreate.TelephoneNumber = "undefined";
                toCreate.FirstName = "undefined";
                toCreate.LastName = "undefined";
                toCreate.MiddleName = "undefined";
                try
                 {
                toCreate.IsLead = bool.Parse(user.Properties.FirstOrDefault(x => x.Name == "isLead").Value);
                 }
                 catch { toCreate.IsLead = false; }
                _context.Users.Add(toCreate);
                _context.Passwords.Add(new Sequrity() { UserId = user.Login, Password = user.HashPassword });
                _context.SaveChanges();
                Logger.Debug($"Added user {user.Login} with default properties");
            }
            else
            {
                Logger.Error($"User with login {user.Login} already exists");
            } 
        }

        public IEnumerable<Property> GetAllProperties()
        {
            List<Property> properties = new List<Property>();

            PropertyInfo[] userProperties = typeof(User).GetProperties();
            properties.Add(new Property("Password", "Password"));
            foreach (PropertyInfo prop in userProperties)
            {
                if (prop.Name.ToLower() != "login")
                {
                    Property userProperty = new Property(prop.Name, prop.Name);
                    properties.Add(userProperty);
                }
            }
            return properties.AsEnumerable();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            User user = _dataManager.GetUser(userLogin);
            List<UserProperty> properties = new List<UserProperty>();
            if (user != null)
            {
                //string password = _dataManager.GetUserPassword(userLogin);
                PropertyInfo[] userProperties = typeof(User).GetProperties();

                //There are only 5 properties in test asset, sorry password
                //properties.Add(new UserProperty("Password", password));
                foreach (PropertyInfo prop in userProperties)
                {
                    if (prop.Name.ToLower() != "login")
                    {
                        UserProperty userProperty = new UserProperty(prop.Name, prop.GetValue(user)?.ToString());

                        properties.Add(userProperty);
                    }
                }
                Logger.Debug($"Returned properties of {userLogin}");
            }
            else
            {
                Logger.Warn($"Couldn't find user {userLogin}");
            }
            return properties;
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                var user = _dataManager.GetUser(userLogin);
                if (user is not null)
                {
                    Logger.Debug($"It's glad to hear that {userLogin} is not fired");
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                Logger.Error("Can't check if user exists");
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _dataManager.GetUser(userLogin);
            var password = _context.Passwords.FirstOrDefault(x => x.UserId == userLogin);
            properties = properties.Where(x => x.Name.ToLower() != "login");

            foreach (UserProperty property in properties)
            {
                if (property.Name.ToLower() == "password" && password != null)
                {
                    password.Password = property.Value;
                    password.UserId = userLogin;
                    
                    _context.Passwords.Update(password);
                    _context.SaveChanges();
                }
                else
                {
                    PropertyInfo propToUpdate = typeof(User).GetProperty(property.Name);

                    if (propToUpdate != null)
                    {
                        propToUpdate.SetValue(user, Convert.ChangeType(property.Value, propToUpdate.PropertyType));
                    }
                }
            }
            _context.Users.Update(user);
            _context.SaveChanges();
            Logger.Debug($"{userLogin} is updated");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            List<Permission> permissions = new List<Permission>();

            var itRights = _context.ITRoles.Select(x => new Permission(x.Id.ToString(), x.Name, itRoleRightGroupName));
            var rRights = _context.RequestRights.Select(x => new Permission(x.Id.ToString(), x.Name, requestRightGroupName));

            permissions.AddRange(itRights);
            permissions.AddRange(rRights);

            Logger.Debug("Loaded all permissions in system");
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = _dataManager.GetUser(userLogin);
            foreach(var rightId in rightIds)
            {
                try
                {
                    var splitted = rightId.Split(delimeter);
                    if (splitted[0] == itRoleRightGroupName)
                    {
                        var role = _context.ITRoles.FirstOrDefault(x => x.Id.ToString() == splitted[1]);
                        if(role != null)
                        {
                            _context.UserITRoles.Add(new UserITRole() { RoleId = role.Id ?? 0, UserId = user.Login });
                            _context.SaveChanges();
                        }
                        else
                        {
                            Logger.Error($"Can't find IT roles like {rightId}");
                        }
                    }
                    if (splitted[0] == requestRightGroupName)
                    {
                        var role = _context.RequestRights.FirstOrDefault(x => x.Id.ToString() == splitted[1]);
                        if (role != null)
                        {
                            _context.UserRequestRights.Add(new UserRequestRight() { RightId = role.Id ?? 0, UserId = user.Login});
                            _context.SaveChanges();
                        }
                        else{
                            Logger.Error($"Can't find request rights like {rightId}");
                        }
                    }
                }
                catch
                {
                    Logger.Error($"Error while adding new permissions for {userLogin}");
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = _dataManager.GetUser(userLogin);
            foreach (var rightId in rightIds)
            {
                try
                {
                    var splitted = rightId.Split(delimeter);
                    if (splitted[0] == itRoleRightGroupName)
                    {
                        var role = _context.ITRoles.FirstOrDefault(x => x.Id.ToString() == splitted[1]);
                        if (role != null)
                        {
                            _context.UserITRoles.Remove(new UserITRole() { RoleId = role.Id ?? 0, UserId = user.Login });
                            _context.SaveChanges();
                        }
                        else
                        {
                            Logger.Error($"Can't find IT roles like {rightId}");
                        }
                    }
                    if (splitted[0] == requestRightGroupName)
                    {
                        var role = _context.RequestRights.FirstOrDefault(x => x.Id.ToString() == splitted[1]);
                        if (role != null)
                        {
                            _context.UserRequestRights.Remove(new UserRequestRight() { RightId = role.Id ?? 0, UserId = user.Login });
                            _context.SaveChanges();
                        }
                        else
                        {
                            Logger.Error($"Can't find request rights like {rightId}");
                        }
                    }
                }
                catch
                {
                    Logger.Error($"Error while removing new permissions for {userLogin}");
                }
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var rRights = _dataManager.GetCRequestRightsByUser(userLogin)
                .Select(x => requestRightGroupName + delimeter + x.RightId.ToString());
            var itRoles = _dataManager.GetITRolesByUser(userLogin)
                .Select(x => itRoleRightGroupName + delimeter + x.RoleId.ToString());
            var result = new List<string>();
            result.AddRange(rRights);
            result.AddRange(itRoles);

            Logger.Debug($"Loaded {userLogin} permissions");
            return result;
        }

        public ILogger Logger { get; set; }
    }
}