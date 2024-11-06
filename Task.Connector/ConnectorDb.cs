using Microsoft.EntityFrameworkCore;
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

        static string requestRightGroupName = "Request";
        static string itRoleRightGroupName = "Role";
        static string delimeter = ":";

        public ILogger Logger { get; set; }
        public ConnectorDb() { }

        /// <inheritdoc/>
        public void StartUp(string connectionString)
        {
            var startIdx = connectionString.IndexOf("ConnectionString='") + "ConnectionString='".Length;
            var endIdx = connectionString.IndexOf("';", startIdx);
            var extractedConnectionString = connectionString.Substring(startIdx, endIdx - startIdx);

            var providerStartIdx = connectionString.IndexOf("Provider='") + "Provider='".Length;
            var providerEndIdx = connectionString.IndexOf("';", providerStartIdx);
            var rawProvider = connectionString.Substring(providerStartIdx, providerEndIdx - providerStartIdx);

            var provider = rawProvider.StartsWith("PostgreSQL", StringComparison.OrdinalIgnoreCase) ? "POSTGRE" : rawProvider.ToUpper();

            var dbContextFactory = new DbContextFactory(extractedConnectionString);
            _dataManager = new DataManager(dbContextFactory, provider);
            _context = dbContextFactory.GetContext(provider);
        }

        /// <inheritdoc/>
        public void CreateUser(UserToCreate user)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var newUser = new User
                {
                    Login = user.Login,
                    FirstName = "Agent",
                    LastName = "Gabena",
                    MiddleName = "322",
                    IsLead = user.Properties.FirstOrDefault(v => v.Name.Equals("islead", StringComparison.OrdinalIgnoreCase))?.Value == "true",
                    TelephoneNumber = "89998887766",
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                var password = new Sequrity
                {
                    UserId = user.Login,
                    Password = user.HashPassword
                };

                _context.Passwords.Add(password);
                _context.SaveChanges();

                transaction.Commit();
                Logger.Debug($"User - {user.Login} created successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating user: {ex.Message}");
                transaction.Rollback();
                throw new ArgumentException($"Invalid format for login '{user}'");
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var userProperties = typeof(User)
                    .GetProperties()
                    .Where(prop => prop.Name != "Login")
                    .Select(prop => new Property(prop.Name, prop.PropertyType.ToString()));

                var passwordProperties = typeof(Sequrity)
                    .GetProperties()
                    .Where(prop => prop.Name != "Id" && prop.Name != "UserId")
                    .Select(prop => new Property(prop.Name, prop.PropertyType.ToString()));

                var result = userProperties.Concat(passwordProperties).ToList();

                Logger.Debug($"GetAllProperties completed successfully. Retrieved {result.Count} properties.");

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred in GetAllProperties: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = GetUserOrThrow(userLogin);

                var userProperties = new List<UserProperty>
                {
                    new UserProperty("FirstName", user.FirstName),
                    new UserProperty("LastName", user.LastName),
                    new UserProperty("MiddleName", user.MiddleName),
                    new UserProperty("TelephoneNumber", user.TelephoneNumber),
                    new UserProperty("IsLead", user.IsLead.ToString())
                };

                Logger.Debug($"GetUserProperties completed successfully for userLogin: {userLogin}. Retrieved {userProperties.Count()} properties.");

                return userProperties;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred in GetUserProperties for userLogin: {userLogin}. Error: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IsUserExists(string userLogin)
        {
            try
            {
                bool exists = _dataManager.GetUser(userLogin) != null ? true : false;

                Logger.Debug($"IsUserExists completed for userLogin: {userLogin}. Exists: {exists}");

                return exists;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred in IsUserExists for userLogin: {userLogin}. Error: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = GetUserOrThrow(userLogin);

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var property in properties)
                {
                    TryUpdateUserProperty(user, property);
                }

                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();
                transaction.Commit();
                Logger.Debug($"UpdateUserProperties completed successfully for userLogin: {userLogin}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.Error($"An error occurred in UpdateUserProperties for userLogin: {userLogin}. Error: {ex.Message}");
                throw new ArgumentException($"User with login '{userLogin}' not found.");
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var requestRightsPermissions = GetPermissionsFromRequestRights();

                Logger.Debug($"Retrieved {requestRightsPermissions.Count} RequestRight permissions");

                var itRolePermissions = GetPermissionsFromItRoles();

                Logger.Debug($"Retrieved {itRolePermissions.Count} ItRole permissions");

                var allPermissions = requestRightsPermissions.Concat(itRolePermissions).ToList();

                Logger.Debug($"GetAllPermissions completed successfully. Total permissions: {allPermissions.Count}");

                return allPermissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred in GetAllPermissions: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!rightIds.Any())
            {
                Logger.Error("No permissions provided to add.");
                return;
            }

            var user = GetUserOrThrow(userLogin);
            var rights = ParsePermissions(rightIds);

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var requestId in rights.RequestIds)
                {
                    if (!_context.UserRequestRights.Any(r => r.UserId == userLogin && r.RightId == requestId))
                    {
                        _context.UserRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = requestId });
                        Logger.Debug($"Added RequestRight with Id: {requestId} to userLogin: {userLogin}");
                    }
                }

                foreach (var roleId in rights.RoleIds)
                {
                    if (!_context.UserITRoles.Any(r => r.UserId == userLogin && r.RoleId == roleId))
                    {
                        _context.UserITRoles.Add(new UserITRole { UserId = userLogin, RoleId = roleId });
                        Logger.Debug($"Added ItRole with Id: {roleId} to userLogin: {userLogin}");
                    }
                }

                _context.SaveChanges();
                transaction.Commit();
                Logger.Debug($"AddUserPermissions completed for userLogin: {userLogin}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.Error($"Error adding permissions for userLogin: {userLogin}. Error: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!rightIds.Any())
            {
                Logger.Warn("No permissions provided to remove.");
                return;
            }

            var user = GetUserOrThrow(userLogin);
            var rights = ParsePermissions(rightIds);

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var requestId in rights.RequestIds)
                {
                    var userRequestRight = _context.UserRequestRights
                        .FirstOrDefault(r => r.UserId == userLogin && r.RightId == requestId);

                    if (userRequestRight != null)
                    {
                        _context.UserRequestRights.Remove(userRequestRight);
                        Logger.Debug($"Removed RequestRight with Id: {requestId} for userLogin: {userLogin}");
                    }
                    else
                    {
                        Logger.Warn($"Request right with Id: {requestId} not found for userLogin: {userLogin}");
                    }
                }

                foreach (var roleId in rights.RoleIds)
                {
                    var userItRole = _context.UserITRoles
                        .FirstOrDefault(r => r.UserId == userLogin && r.RoleId == roleId);

                    if (userItRole != null)
                    {
                        _context.UserITRoles.Remove(userItRole);
                        Logger.Debug($"Removed ItRole with Id: {roleId} for userLogin: {userLogin}");
                    }
                    else
                    {
                        Logger.Warn($"Role with Id: {roleId} not found for userLogin: {userLogin}");
                    }
                }

                _context.SaveChanges();
                transaction.Commit();
                Logger.Debug($"RemoveUserPermissions completed for userLogin: {userLogin}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.Error($"Error removing permissions for userLogin: {userLogin}. Error: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                var requestRights = GetRequestRights(userLogin);
                Logger.Debug($"Retrieved {requestRights.Count()} request rights for userLogin: {userLogin}");

                var itRoles = GetItRoles(userLogin);
                Logger.Debug($"Retrieved {itRoles.Count()} IT roles for userLogin: {userLogin}");

                var allPermissions = requestRights.Concat(itRoles).ToList();
                Logger.Debug($"GetUserPermissions completed successfully for userLogin: {userLogin}. Total permissions: {allPermissions.Count}");

                return allPermissions;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred in GetUserPermissions for userLogin: {userLogin}. Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Получает модель пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <returns>Модель пользователя</returns>
        /// <exception cref="ArgumentException">Пользователь с указанным логином не найден</exception>
        private User GetUserOrThrow(string userLogin)
        {
            var user = _dataManager.GetUser(userLogin);
            if (user == null)
            {
                Logger.Error($"User with login '{userLogin}' not found.");
                throw new ArgumentException($"User with login '{userLogin}' not found.");
            }

            return user;
        }

        /// <summary>
        /// Возвращает объект, содержащий два списка: идентификаторы запросов и ролей
        /// </summary>
        /// <param name="IDs">Список строковых идентификаторов прав в формате "Тип:Идентификатор"</param>
        private Right ParsePermissions(IEnumerable<string> IDs)
        {
            var result = new Right();

            foreach (var right in IDs)
            {
                var parts = right.Split(delimeter, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 || !int.TryParse(parts[1], out int id))
                {
                    Logger.Warn($"Invalid format for permission identifier: {right}");
                    continue;
                }

                if (parts[0].Contains(requestRightGroupName, StringComparison.OrdinalIgnoreCase))
                {
                    result.RequestIds.Add(id);
                }
                else if (parts[0].Contains(itRoleRightGroupName, StringComparison.OrdinalIgnoreCase))
                {
                    result.RoleIds.Add(id);
                }
                else
                {
                    Logger.Error($"Unknown permission type: {parts[0]}");
                }
            }

            return result;
        }

        /// <summary>
        /// Обновляет свойство пользователя заданным значением.
        /// </summary>
        /// <param name="user">Объект, в котором необходимо обновить свойство</param>
        /// <param name="property">Объект, содержащий имя и значение свойства для обновления</param>
        private bool TryUpdateUserProperty(User user, UserProperty property)
        {
            var userProperty = user.GetType().GetProperty(property.Name);
            if (userProperty != null && userProperty.CanWrite)
            {
                object value = property.Name == "IsLead"
                    ? bool.TryParse(property.Value, out var isLead) && isLead
                    : property.Value;

                userProperty.SetValue(user, value);

                Logger.Debug($"Updated property '{property.Name}' for userLogin '{user.Login}' to '{value}'");
                return true;
            }

            Logger.Error($"Property '{property.Name}' does not exist or is not writable on user '{user.Login}'");
            return false;
        }

        /// <summary>
        /// Список прав связанные с пользователем.
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        private IEnumerable<string> GetRequestRights(string userLogin)
        {
            return _context.UserRequestRights
                .Where(ur => ur.UserId == userLogin)
                .Select(ur => $"{requestRightGroupName}{delimeter}{ur.RightId}")
                .ToList();
        }

        /// <summary>
        /// Список ролей связанные с пользователем.
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        private IEnumerable<string> GetItRoles(string userLogin)
        {
            return _context.UserITRoles
                .Where(ur => ur.UserId == userLogin)
                .Select(ur => $"{itRoleRightGroupName}{delimeter}{ur.RoleId}")
                .ToList();
        }

        /// <summary>
        /// Получает права доступа на основе прав типа "Request".
        /// </summary>
        private List<Permission> GetPermissionsFromRequestRights()
        {
            return _context.RequestRights
                .Select(r => new Permission(r.Id.ToString(), r.Name, "RequestRight"))
                .ToList();
        }

        /// <summary>
        /// Получает роли на основе типа "Roles".
        /// </summary>
        private List<Permission> GetPermissionsFromItRoles()
        {
            return _context.ITRoles
                .Select(r => new Permission(r.Id.ToString(), r.Name, "ItRole"))
                .ToList();
        }

        public record Right
        {
            public List<int> RequestIds { get; set; } = new List<int>();
            public List<int> RoleIds { get; set; } = new List<int>();
        }
    }
}