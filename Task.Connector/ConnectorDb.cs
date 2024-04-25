using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Connector.Extensions;
using System.Collections;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _dbContext;
        static string requestRightGroupName = "Request";
        static string itRoleRightGroupName = "Role";
        static string _delimiter = ":";
        public ILogger Logger { get; set; }

        public ConnectorDb() { }

        public void StartUp(string connectionString)
        {
            var splittedConnection = connectionString.Split('\'');
            var dbConnectionString = splittedConnection[1];
            var provider = splittedConnection[3].Equals("PostgreSQL.9.5") ? "POSTGRE" : "MSSQL";

            var dbContextFactory = new DbContextFactory(dbConnectionString);
            _dbContext = dbContextFactory.GetContext(provider);
        }

        public void CreateUser(UserToCreate user)
        {
            if (IsUserExists(user.Login))
            {
                Logger.Warn($"Tried to create a user with login {user.Login}, but user with this login already exists");
                return;
            }

            var newUser = new User
            {
                Login = user.Login,
                FirstName = "",
                MiddleName = "",
                LastName = "",
                TelephoneNumber = ""
            };
            newUser.SetProperties(user.Properties);

            var newUserPassword = new Sequrity
            {
                UserId = user.Login,
                Password = user.HashPassword
            };

            _dbContext.Users.Add(newUser);
            _dbContext.Passwords.Add(newUserPassword);
            _dbContext.SaveChanges();

            Logger.Debug($"Created user with login {user.Login}");
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Returned properties");

            return new List<Property>
            {
                new Property(nameof(User.FirstName), string.Empty),
                new Property(nameof(User.MiddleName), string.Empty),
                new Property(nameof(User.LastName), string.Empty),
                new Property(nameof(User.TelephoneNumber), string.Empty),
                new Property(nameof(User.IsLead), string.Empty),
                new Property(nameof(Sequrity.Password), string.Empty)
            };
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _dbContext.Users.AsNoTracking().FirstOrDefault(x => x.Login.Equals(userLogin));
            if(user == null)
            {
                Logger.Error($"No user with login {userLogin} was found, so no properties was returned");
                return Enumerable.Empty<UserProperty>();
            }

            Logger.Debug($"Returned properties of user with login {userLogin}");
            return user.GetProperties();
        }

        public bool IsUserExists(string userLogin)
        {
            if (_dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Login.Equals(userLogin)) != null)
                return true;

            return false;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Login.Equals(userLogin));

            if(user == null)
            {
                Logger.Error($"No user with login {userLogin} was found");
                return;
            }

            user.SetProperties(properties);

            var passwordProperty = properties.FirstOrDefault(p => p.Name.Equals(nameof(Sequrity.Password)));
            if(passwordProperty != null && !String.IsNullOrEmpty(passwordProperty.Value))
            {
                var userPassword = _dbContext.Passwords.FirstOrDefault(p => p.UserId.Equals(userLogin));
                if(userPassword != null) userPassword.Password = passwordProperty.Value;
            }

            _dbContext.SaveChanges();

            Logger.Debug($"Updated properties of user with login {userLogin}");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var ItRolePermissions = _dbContext.ITRoles
                .AsNoTracking()
                .ToList()
                .Select(ir => new Permission(ir.Id.ToString()!, ir.Name, itRoleRightGroupName));
            var RequestRightsPermissions = _dbContext.RequestRights
                .AsNoTracking()
                .ToList()
                .Select(rr => new Permission(rr.Id.ToString()!, rr.Name, requestRightGroupName));

            Logger.Debug("Returned all permissions");

            return ItRolePermissions.Concat(RequestRightsPermissions);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if(!IsUserExists(userLogin))
            {
                Logger.Error($"No user with login {userLogin} was found to add permissions to");
                return;
            }

            foreach (var right in rightIds)
            {
                var splitted = right.Split(_delimiter);

                var name = splitted[0];
                if (!int.TryParse(splitted[1], out int id)) continue;

                if (name == requestRightGroupName)
                {
                    var requestRight = _dbContext.RequestRights.FirstOrDefault(r => r.Id == id);
                    if (requestRight == null)
                    {
                        Logger.Warn($"No requestRight with id {id} was found");
                        continue;
                    }

                    _dbContext.UserRequestRights.Add(new UserRequestRight
                    {
                        RightId = id,
                        UserId = userLogin
                    });
                }

                if (name == itRoleRightGroupName)
                {
                    var role = _dbContext.ITRoles.FirstOrDefault(r => r.Id == id);
                    if (role == null)
                    {
                        Logger.Warn($"No ITRole with id {id} was found");
                        continue;
                    }

                    _dbContext.UserITRoles.Add(new UserITRole
                    {
                        RoleId = id,
                        UserId = userLogin
                    });
                }
            }

            _dbContext.SaveChanges();

            Logger.Debug($"Added permissions to user with login {userLogin}");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Error($"No user with login {userLogin} was found to remove permissions from");
                return;
            }

            foreach (var right in rightIds)
            {
                var splitted = right.Split(_delimiter);

                var name = splitted[0];
                if (!int.TryParse(splitted[1], out int id)) continue;

                if (name == requestRightGroupName)
                {
                    var requestRight = _dbContext.UserRequestRights.FirstOrDefault(r => r.RightId == id && r.UserId == userLogin);

                    if (requestRight == null)
                    {
                        Logger.Warn($"No ITRole with id {id} for user with login {userLogin} wasn't found");
                        continue;
                    }

                    _dbContext.UserRequestRights.Remove(requestRight);
                }

                if (name == itRoleRightGroupName)
                {
                    var role = _dbContext.UserITRoles.FirstOrDefault(r => r.RoleId == id && r.UserId == userLogin);

                    if (role == null)
                    {
                        Logger.Warn($"No ITRole with id {id} for user with login {userLogin} wasn't found");
                        continue;
                    }

                    _dbContext.UserITRoles.Remove(role);
                }
            }

            _dbContext.SaveChanges();

            Logger.Debug($"Removed permissions from user with login {userLogin}");
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Error($"No user with login {userLogin} was found to get permissions from");
                return Enumerable.Empty<string>();
            }

            var requestRight = _dbContext.UserRequestRights
                .AsNoTracking()
                .Where(r => r.UserId == userLogin)
                .ToList()
                .Select(r => requestRightGroupName + _delimiter + r.RightId);

            var itRoles = _dbContext.UserITRoles
                .AsNoTracking()
                .Where(r => r.UserId == userLogin)
                .ToList()
                .Select(r => requestRightGroupName + _delimiter + r.RoleId);

            Logger.Debug($"Returned permissions for user with login {userLogin}");

            return requestRight.Concat(itRoles);
        }

    }
}