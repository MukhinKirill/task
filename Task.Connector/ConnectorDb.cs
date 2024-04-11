using Microsoft.EntityFrameworkCore;
using Task.Connector.Helpers;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _context;
        public void StartUp(string connectionString)
        {
            var provider = ConnectionStringHelper.GetProvider(connectionString);
            _context = new DbContextFactory(ConnectionStringHelper.GetOriginal(connectionString))
                .GetContext(provider.ToString());
        } 

        public void CreateUser(UserToCreate user)
        {
            if (user is null || string.IsNullOrWhiteSpace(user.Login))
                return;

            var password = new Sequrity()
            {
                UserId = user.Login,
                Password = user.HashPassword,
            };

            _context.Users.Add(UserHelper.Map(user));
            _context.Passwords.Add(password);
            _context.SaveChanges();
            Logger.Debug($"Successfully created user with login {{{user.Login}}}");
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var propertyInfos = typeof(User).GetProperties().Where(p => p.Name != nameof(User.Login));
            var properties = propertyInfos.Select(p => new Property(p.Name, string.Empty)).ToList();
            properties.Add(new Property(nameof(Sequrity.Password), string.Empty));
            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Login == userLogin);
            if (user is null)
                return Enumerable.Empty<UserProperty>();

            var propertyInfos = user.GetType()
                .GetProperties()
                .Where(p => p.Name != nameof(User.Login));

            var userProperties = propertyInfos.Select(p =>
                new UserProperty(p.Name, p.GetValue(user)?.ToString() ?? string.Empty));
            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            if (string.IsNullOrWhiteSpace(userLogin))
                return false;

            return _context.Users.AsNoTracking().Any(u => u.Login == userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (string.IsNullOrWhiteSpace(userLogin) || !properties.Any())
                return;

            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user is null)
                return;
            
            UserHelper.LoadUserProperties(user, properties);

            _context.SaveChanges();
            Logger.Debug($"Successfully updated user {{{user.Login}}}'s properties");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var requestRights = _context.RequestRights.AsNoTracking().ToList();
            var iTRoles = _context.ITRoles.AsNoTracking().ToList();
            return PermissionMapper.Map(requestRights).Concat(PermissionMapper.Map(iTRoles));
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (string.IsNullOrWhiteSpace(userLogin))
                return;

            var user = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Login == userLogin);
            if (user is null)
                return;

            foreach (var rightId in rightIds)
            {
                var split = rightId.Split(':');
                var prefix = split[0];
                var id = split[1];
                
                if (prefix.Equals("Request"))
                {
                    var right = _context.RequestRights
                        .AsNoTracking()
                        .FirstOrDefault(r => r.Id == int.Parse(id));
                    _context.UserRequestRights.Add(new UserRequestRight()
                    {
                        RightId = (int)right.Id,
                        UserId = user.Login
                    });
                    Logger.Debug($"Added request right with Id {{{right.Id}}} for user {{{user.Login}}}");
                }

                if (prefix.Equals("Role"))
                {
                    var role = _context.ITRoles
                        .AsNoTracking()
                        .FirstOrDefault(r => r.Id == int.Parse(id));
                    _context.UserITRoles.Add(new UserITRole()
                    {
                        RoleId = (int)role.Id,
                        UserId = user.Login
                    });
                    Logger.Debug($"Added IT role with Id {{{role.Id}}} for user {{{user.Login}}}");
                }
            }

            _context.SaveChanges();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (string.IsNullOrWhiteSpace(userLogin))
                return;

            var user = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Login == userLogin);
            if (user is null)
                return;

            foreach (var rightId in rightIds)
            {
                var split = rightId.Split(':');
                var prefix = split[0];
                var id = split[1];

                if (prefix.Equals("Request"))
                {
                    var right = _context.RequestRights
                        .AsNoTracking()
                        .FirstOrDefault(r => r.Id == int.Parse(id));
                    _context.UserRequestRights.Remove(new UserRequestRight()
                    {
                        RightId = (int)right.Id,
                        UserId = user.Login
                    });
                    Logger.Debug($"Removed request right with Id {{{right.Id}}} of user {{{user.Login}}}");
                }

                if (prefix.Equals("Role"))
                {
                    var role = _context.ITRoles
                        .AsNoTracking()
                        .FirstOrDefault(r => r.Id == int.Parse(id));
                    _context.UserITRoles.Remove(new UserITRole()
                    {
                        RoleId = (int)role.Id,
                        UserId = user.Login
                    });
                    Logger.Debug($"Removed IT role with Id {{{role.Id}}} of user {{{user.Login}}}");
                }
            }

            _context.SaveChanges();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (string.IsNullOrWhiteSpace(userLogin))
                return Enumerable.Empty<string>();

            var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Login == userLogin);
            if (user is null)
                return Enumerable.Empty<string>();

            var requestRights = _context.UserRequestRights
                .AsNoTracking()
                .Where(urr => urr.UserId == userLogin)
                .ToList();

            return _context.RequestRights
                .AsNoTracking()
                .AsEnumerable()
                .Where(rr => requestRights.Exists(r => r.RightId == rr.Id))
                .Select(r => r.Name);
        }

        public ILogger Logger { get; set; }
    }
}