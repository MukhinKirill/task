using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Task.Connector.Services;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using ILogger = Task.Integration.Data.Models.ILogger;

namespace Task.Connector
{
    public class ConnectionStringParser
    {
        public static string GetPostgreConnectionString(string fullString)
        {
            var regexPattern = @"ConnectionString='([^']+)'";
            var match = Regex.Match(fullString, regexPattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new ArgumentException("Invalid connection string format");
            }
        }
    }

    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        private DataContext _context;
        private UserManagementService _userManagementService;

        public void StartUp(string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
                connectionString = config.GetConnectionString("PostgreSql");
            }

            DbContextOptionsBuilder<DataContext> optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            string postgreConnectionString = ConnectionStringParser.GetPostgreConnectionString(connectionString);
            optionsBuilder.UseNpgsql(postgreConnectionString);
            _context = new DataContext(optionsBuilder.Options);
            _userManagementService = new UserManagementService(_context, Logger);
        }

        public void CreateUser(UserToCreate user)
        {
            if (IsUserExists(user.Login))
            {
                Logger.Warn($"User with `{user.Login}` already exists");
                return;
            }

            User newUser = new User
            {
                Login = user.Login,
                LastName = "NoLastName",
                FirstName = "NoFirstName",
                MiddleName = "NoMiddleName",
                TelephoneNumber = "NoTelephoneNumber",
                IsLead = false
            };
            Sequrity newUserPassword = new Sequrity
            {
                UserId = newUser.Login,
                Password = user.HashPassword
            };
            _userManagementService.SetUserProperties(newUser, user.Properties);
            _context.Users.Add(newUser);
            _context.Passwords.Add(newUserPassword);
            _context.SaveChanges();
            Logger?.Debug($"User `{user.Login}` created successfully");
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Retrieving the list of all properties for users");
            List<Property> propertyList = new List<Property>
            {
                new(nameof(User.LastName), ""),
                new(nameof(User.FirstName), ""),
                new(nameof(User.MiddleName), ""),
                new(nameof(User.TelephoneNumber), ""),
                new(nameof(User.IsLead), ""),
                new(nameof(Sequrity.Password), "")
            };
            return propertyList;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (string.IsNullOrEmpty(userLogin))
            {
                Logger.Warn("User login cannot be null or empty");
                return Enumerable.Empty<UserProperty>();
            }

            User user = _context.Users.FirstOrDefault(x => x.Login == userLogin);
            if (user == null)
            {
                Logger.Warn($"No user found with login '{userLogin}'");
                return Enumerable.Empty<UserProperty>();
            }

            List<UserProperty> userProperties = new List<UserProperty>
            {
                new UserProperty(nameof(User.LastName), user.LastName ?? ""),
                new UserProperty(nameof(User.FirstName), user.FirstName ?? ""),
                new UserProperty(nameof(User.MiddleName), user.MiddleName ?? ""),
                new UserProperty(nameof(User.TelephoneNumber), user.TelephoneNumber ?? ""),
                new UserProperty(nameof(User.IsLead), user.IsLead.ToString())
            };

            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            return _context.Users.Any(user => user.Login == userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (string.IsNullOrEmpty(userLogin))
            {
                Logger.Warn("User login cannot be null or empty");
                return;
            }

            User user = _context.Users.FirstOrDefault(x => x.Login == userLogin);
            if (user == null)
            {
                Logger.Warn($"No user found with login '{userLogin}'");
                return;
            }

            _userManagementService.UpdateUserPropertiesFromList(user, properties);
            _context.SaveChanges();
            Logger.Debug($"User with login `{userLogin}` has been updated");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            List<Permission> itRoles = _context.ITRoles.Select(itRole =>
                new Permission(itRole.Id.ToString(), itRole.Name, "IT Role")
            ).ToList();

            List<Permission> requestRights = _context.RequestRights.Select(requestRight =>
                new Permission(requestRight.Id.ToString(), requestRight.Name, "Request Right")
            ).ToList();
            var allPermissions = itRoles.Concat(requestRights).ToList();
            return allPermissions;
        }


        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (string.IsNullOrEmpty(userLogin))
            {
                Logger.Warn("User login cannot be null or empty");
                return;
            }

            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger.Warn($"No user found with login '{userLogin}'");
                return;
            }

            foreach (var rightId in rightIds)
            {
                if (_userManagementService.IsRightIdInvalid(rightId)) continue;
                var (type, id) = _userManagementService.ParseRightId(rightId);

                switch (type)
                {
                    case "Role":
                        _userManagementService.AddUserRole(userLogin, id);
                        break;
                    case "Request":
                        _userManagementService.AddUserRequestRight(userLogin, id);
                        break;
                    default:
                        Logger.Warn($"Unknown permission type '{type}'");
                        break;
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (string.IsNullOrEmpty(userLogin))
            {
                Logger.Warn("User login cannot be null or empty");
                return;
            }

            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger.Warn($"No user found with login '{userLogin}'");
                return;
            }

            foreach (var rightId in rightIds)
            {
                if (_userManagementService.IsRightIdInvalid(rightId)) continue;
                var (type, id) = _userManagementService.ParseRightId(rightId);
                switch (type)
                {
                    case "Role":
                        _userManagementService.RemoveUserItRole(userLogin, id);
                        break;
                    case "Request":
                        _userManagementService.RemoveUserRequestRight(userLogin, id);
                        break;
                    default:
                        Logger.Warn($"Unknown permission type '{type}'");
                        break;
                }
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger.Warn($"No user found with login '{userLogin}'");
                return Enumerable.Empty<string>();
            }

            var permissions = _context.UserRequestRights
                .AsNoTracking()
                .Where(r => r.UserId == userLogin)
                .Select(r => "Request:" + r.RightId.ToString())
                .Union(
                    _context.UserITRoles
                        .AsNoTracking()
                        .Where(r => r.UserId == userLogin)
                        .Select(r => "Role:" + r.RoleId.ToString())
                );
            return permissions;
        }
    }
}