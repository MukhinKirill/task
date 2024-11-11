using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private TestTaskDbContext _context;
        public ILogger Logger { get; set; }

        static string requestRightGroupName = "Request";
        static string itRoleRightGroupName = "Role";
        static string delimeter = ":";

        static string lastNameProperty = "lastName";
        static string firstNameProperty = "firstName";
        static string middleNameProperty = "middleName";
        static string telephoneNumberProperty = "telephoneNumber";
        static string isLeadProperty = "isLead";

        // Initializes the database context with the given connection string
        public void StartUp(string connectionString)
        {
            var options = new DbContextOptionsBuilder<TestTaskDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            _context = new TestTaskDbContext(options);
            Logger?.Debug("Database context initialized.");
        }

        // Adds a new user to the database
        public void CreateUser(UserToCreate userToCreate)
        {
            if (userToCreate == null)
            {
                Logger?.Error("CreateUser was called with a null argument.");
                throw new ArgumentNullException(nameof(userToCreate));
            }

            // Checks if user with the same login already exists
            if (_context != null && _context.Users.Any(u => u.Login == userToCreate.Login))
            {
                Logger?.Error("User with the specified login already exists.");
                throw new InvalidOperationException("User with this login already exists.");
            }

            // Retrieve user properties or set default values
            string lastName = userToCreate.Properties.FirstOrDefault(p => p.Name == lastNameProperty)?.Value ?? string.Empty;
            string firstName = userToCreate.Properties.FirstOrDefault(p => p.Name == firstNameProperty)?.Value ?? string.Empty;
            string middleName = userToCreate.Properties.FirstOrDefault(p => p.Name == middleNameProperty)?.Value ?? string.Empty;
            string telephoneNumber = userToCreate.Properties.FirstOrDefault(p => p.Name == telephoneNumberProperty)?.Value ?? string.Empty;
            bool isLead = bool.TryParse(userToCreate.Properties.FirstOrDefault(p => p.Name == isLeadProperty)?.Value, out bool parsedIsLead) && parsedIsLead;

            // Create a new user object
            var newUser = new User
            {
                Login = userToCreate.Login,
                LastName = lastName,
                FirstName = firstName,
                MiddleName = middleName,
                TelephoneNumber = telephoneNumber,
                IsLead = isLead
            };

            // Add new user to context
            _context.Users.Add(newUser);
            Logger?.Debug($"User {userToCreate.Login} added to Users table.");

            // Create an entry for user password
            var passwordEntry = new Password
            {
                UserId = userToCreate.Login,
                Password1 = userToCreate.HashPassword
            };

            _context.Passwords.Add(passwordEntry);
            Logger?.Debug("Password entry added.");

            // Save changes to the database
            _context.SaveChanges();
            Logger?.Debug("Changes saved to the database.");
        }

        // Retrieves all user properties excluding the login
        public IEnumerable<Property> GetAllProperties()
        {
            Logger?.Debug("Retrieving all properties for User entity.");
            var properties = new List<Property>();

            foreach (var prop in typeof(User).GetProperties())
            {
                if (prop.Name != "Login")
                {
                    properties.Add(new Property(prop.Name, string.Empty));
                }
            }

            // Add password property from Password class, because it counts as property
            properties.Add(new Property(nameof(Password.Password1), string.Empty));
            Logger?.Debug($"Total properties retrieved: {properties.Count}");

            return properties;
        }

        // Gets properties for a specific user
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger?.Debug($"Retrieving properties for user with login: {userLogin}");
            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger?.Warn($"User with login {userLogin} not found.");
                return Enumerable.Empty<UserProperty>();
            }

            // Create a list of user properties
            var userProperties = new List<UserProperty>
            {
                new UserProperty(lastNameProperty, user.LastName),
                new UserProperty(firstNameProperty, user.FirstName),
                new UserProperty(middleNameProperty, user.MiddleName),
                new UserProperty(telephoneNumberProperty, user.TelephoneNumber),
                new UserProperty(isLeadProperty, user.IsLead.ToString())
            };

            Logger?.Debug($"Properties retrieved for user {userLogin}: {userProperties.Count}");
            return userProperties;
        }

        // Checks if a user exists by login
        public bool IsUserExists(string userLogin)
        {
            var exists = _context.Users.Any(u => u.Login == userLogin);
            Logger?.Debug($"User existence check for login: {userLogin} - Result: {exists}");
            return exists;
        }

        // Updates properties for an existing user
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger?.Warn($"User with login {userLogin} not found for property update.");
                return;
            }

            // Update each property based on its name
            foreach (var property in properties)
            {
                switch (property.Name)
                {
                    case var _ when property.Name == lastNameProperty:
                        user.LastName = property.Value;
                        break;
                    case var _ when property.Name == firstNameProperty:
                        user.FirstName = property.Value;
                        break;
                    case var _ when property.Name == middleNameProperty:
                        user.MiddleName = property.Value;
                        break;
                    case var _ when property.Name == telephoneNumberProperty:
                        user.TelephoneNumber = property.Value;
                        break;
                    case var _ when property.Name == isLeadProperty:
                        user.IsLead = bool.Parse(property.Value);
                        break;
                    default:
                        Logger?.Warn($"Unknown property: {property.Name}");
                        break;
                }
            }
            _context.SaveChanges();
            Logger?.Debug($"User properties updated for login: {userLogin}");
        }

        // Retrieves all permissions available in the system
        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger?.Debug("Retrieving all permissions.");
            var requestRights = _context.RequestRights
                .Select(r => new Permission(r.Id.ToString(), r.Name, requestRightGroupName))
                .ToList();

            var itRoles = _context.ItRoles
                .Select(i => new Permission(i.Id.ToString(), i.Name, itRoleRightGroupName))
                .ToList();

            var allPermissions = requestRights.Concat(itRoles).ToList();
            Logger?.Debug($"Total permissions retrieved: {allPermissions.Count}");

            return allPermissions;
        }

        // Adds permissions to a user based on provided right IDs
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger?.Warn($"User with login {userLogin} not found for adding permissions.");
                return;
            }

            foreach (var rightId in rightIds)
            {
                var split = rightId.Split(delimeter);
                var rightOrRole = split[0];
                var parsedId = split.Length > 1 ? split[1] : string.Empty;

                if (string.IsNullOrEmpty(parsedId))
                {
                    continue;
                }

                // Check permission type and add accordingly
                switch (rightOrRole)
                {
                    case var _ when rightOrRole == requestRightGroupName:
                        AddRequestRight(userLogin, parsedId);
                        break;

                    case var _ when rightOrRole == itRoleRightGroupName:
                        AddItRole(userLogin, parsedId);
                        break;

                    default:
                        Logger?.Warn($"Unknown permission type: {rightOrRole}");
                        break;
                }
            }

            _context.SaveChanges();
            Logger?.Debug($"Permissions added for user with login: {userLogin}");
        }

        // Adds a request right to the user
        private void AddRequestRight(string userLogin, string parsedId)
        {
            if (int.TryParse(parsedId, out int rightId) &&
                _context.RequestRights.Any(r => r.Id == rightId) &&
                !_context.UserRequestRights.Any(ur => ur.UserId == userLogin && ur.RightId == rightId))
            {
                _context.UserRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = rightId });
                Logger?.Debug($"Request right added for user: {userLogin}, Right ID: {rightId}");
            }
        }

        // Adds an IT role to the user
        private void AddItRole(string userLogin, string parsedId)
        {
            if (int.TryParse(parsedId, out int roleId) &&
                _context.ItRoles.Any(r => r.Id == roleId) &&
                !_context.UserItroles.Any(ur => ur.UserId == userLogin && ur.RoleId == roleId))
            {
                _context.UserItroles.Add(new UserItrole { UserId = userLogin, RoleId = roleId });
                Logger?.Debug($"IT role added for user: {userLogin}, Role ID: {roleId}");
            }
        }

        // Removes permissions from a user based on provided right IDs
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            foreach (var rightId in rightIds)
            {
                var split = rightId.Split(delimeter);
                var rightOrRole = split[0];
                var parsedId = split.Length > 1 ? split[1] : string.Empty;

                if (parsedId.IsNullOrEmpty())
                {
                    continue;
                }

                // Check permission type and remove accordingly
                switch (rightOrRole)
                {
                    case var _ when rightOrRole == requestRightGroupName:
                        RemoveRequestRight(userLogin, parsedId);
                        break;

                    case var _ when rightOrRole == itRoleRightGroupName:
                        RemoveItRole(userLogin, parsedId);
                        break;

                    default:
                        Logger?.Warn($"Unknown permission type: {rightOrRole}");
                        break;
                }
            }

            _context.SaveChanges();
            Logger?.Debug($"Permissions removed for user with login: {userLogin}");
        }

        // Removes a request right from the user
        private void RemoveRequestRight(string userLogin, string parsedId)
        {
            var requestRight = _context.UserRequestRights
                .FirstOrDefault(ur => ur.UserId == userLogin && ur.RightId.ToString() == parsedId);

            if (requestRight != null)
            {
                _context.UserRequestRights.Remove(requestRight);
                Logger?.Debug($"Request right removed for user: {userLogin}, Right ID: {parsedId}");
            }
        }

        // Removes an IT role from the user
        private void RemoveItRole(string userLogin, string parsedId)
        {
            var itRole = _context.UserItroles
                .FirstOrDefault(ur => ur.UserId == userLogin && ur.RoleId.ToString() == parsedId);

            if (itRole != null)
            {
                _context.UserItroles.Remove(itRole);
                Logger?.Debug($"IT role removed for user: {userLogin}, Role ID: {parsedId}");
            }
        }

        // Retrieves permissions for a specific user
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger?.Debug($"Retrieving permissions for user with login: {userLogin}");
            var requestRights = _context.UserRequestRights
                .Where(ur => ur.UserId == userLogin)
                .Select(ur => ur.RightId.ToString());

            var itRoles = _context.UserItroles
                .Where(ur => ur.UserId == userLogin)
                .Select(ur => ur.RoleId.ToString());

            var permissions = requestRights.Concat(itRoles).ToList();
            Logger?.Debug($"Permissions retrieved for user {userLogin}: {permissions.Count}");

            return permissions;
        }
    }
}
