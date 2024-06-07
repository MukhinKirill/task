using System.Reflection;
using Task.Domain.Users;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        private DbContextFactory _dbContextFactory;
        private DataContext _dbContext;

        public void StartUp(string connectionString)
        {
            _dbContextFactory = new DbContextFactory(connectionString);
            _dbContext = _dbContextFactory.GetContext("POSTGRE");
        }

        public void CreateUser(UserToCreate request)
        {
            var user = new User { Login = request.Login };
            user.MapProperties(request.Properties);

            var password = new Sequrity
            {
                UserId = user.Login,
                Password = request.HashPassword
            };

            _dbContext.Users.Add(user);
            _dbContext.Passwords.Add(password);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return typeof(UserPropertyNamesConst).GetMembers()
                .Where(m => m.MemberType.Equals(MemberTypes.Field))
                .Select(p => new Property(p.Name, p.MemberType.ToString()));
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _dbContext.Users
                .SingleOrDefault(u => u.Login.Equals(userLogin));

            if (user is not null)
                return user.ToUserProperties();

            Logger.Error($"User with login {userLogin} not found!");
            throw new ArgumentException($"User with login {userLogin} not found!");
        }

        public bool IsUserExists(string userLogin)
        {
            return _dbContext.Users.Any(u => u.Login.Equals(userLogin));
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Login.Equals(userLogin));

            if (user is null)
            {
                Logger.Error($"User with login {userLogin} not found!");
                throw new ArgumentException($"User with login {userLogin} not found!");
            }
            
            user.MapProperties(properties);
            _dbContext.Update(user);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var rolePermissions = _dbContext.ITRoles
                .Select(role => new Permission(role.Id.Value.ToString(), role.Name, "Role"))
                .ToList();

            var rightPermissions = _dbContext.RequestRights
                .Select(right => new Permission(right.Id.Value.ToString(), right.Name, "Request"))
                .ToList();

            var permissions = rolePermissions.Concat(rightPermissions);

            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Login.Equals(userLogin));

            if (user is null)
            {
                Logger.Error($"User with login {userLogin} not found!");
                throw new ArgumentException($"User with login {userLogin} not found!");
            }
                
            foreach (var rightId in rightIds)
            {
                AddPermission(userLogin, rightId);
            }

            _dbContext.SaveChanges();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Login.Equals(userLogin));

            if (user is null)
            {
                Logger.Error($"User with login {userLogin} not found!");
                throw new ArgumentException($"User with login {userLogin} not found!");
            }

            foreach (var rightId in rightIds)
            {
                RemovePermission(userLogin, rightId);
            }

            _dbContext.SaveChanges();
        }

        public IEnumerable<string> GetUserPermissions(string userId)
        {
            var userPermissions = _dbContext.RequestRights
                .Where(r => _dbContext.UserRequestRights
                    .Any(ur => ur.UserId.Equals(userId) && ur.RightId == r.Id))
                .Select(right => right.Name)
                .ToList();

            return userPermissions;
        }

        // SUPPORT
        private void AddPermission(string userLogin, string rightId)
        {
            if (rightId.Contains("Role"))
            {
                _dbContext.UserITRoles.Add(new UserITRole
                {
                    UserId = userLogin,
                    RoleId = int.Parse(rightId.Replace("Role:", ""))
                });
            }
            else if (rightId.Contains("Request"))
            {
                _dbContext.UserRequestRights.Add(new UserRequestRight
                {
                    UserId = userLogin,
                    RightId = int.Parse(rightId.Replace("Request:", ""))
                });
            }
        }

        private void RemovePermission(string userLogin, string rightId)
        {
            if (rightId.Contains("Role"))
            {
                var roleId = int.Parse(rightId.Replace("Role:", ""));
                var userRole = _dbContext.UserITRoles.SingleOrDefault(r => r.UserId == userLogin && r.RoleId == roleId);
                if (userRole != null)
                {
                    _dbContext.UserITRoles.Remove(userRole);
                }
            }
            else if (rightId.Contains("Request"))
            {
                var rightIdParsed = int.Parse(rightId.Replace("Request:", ""));
                var userRequestRight =
                    _dbContext.UserRequestRights.SingleOrDefault(r => r.UserId == userLogin && r.RightId == rightIdParsed);
                if (userRequestRight != null)
                {
                    _dbContext.UserRequestRights.Remove(userRequestRight);
                }
            }
        }
    }
}