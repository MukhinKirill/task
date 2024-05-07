using Microsoft.EntityFrameworkCore;

using System.Data;
using System.Transactions;

using Task.Connector.Common;
using Task.Connector.Common.Exceptions;
using Task.Connector.Entities;
using Task.Connector.Persistence;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    /// <summary>
    /// Represents a database connector that implements the IConnector interface.
    /// </summary>
    /// <remarks>
    /// This class provides methods for user management and permission handling.
    /// </remarks>
    public class ConnectorDb : IConnector, IDisposable
    {
        private bool _disposed;
        private DataContext _context = null!;
        private UserRepository _userRepository = null!;
        private PermissionRepository _permissionRepository = null!;
        public ILogger Logger { get; set; } = null!;

        /// <summary>
        /// Starts up the database connection with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to the database.</param>
        public void StartUp(string connectionString)
        {
            DataContextFactory factory = new(connectionString, Logger);
            _context = factory.GetContext();
            _userRepository = new(_context);
            _permissionRepository = new(_context);
        }

        /// <summary>
        /// Creates a new user based on the provided UserToCreate object.
        /// </summary>
        /// <param name="user">The UserToCreate object containing user details.</param>
        public void CreateUser(UserToCreate user)
        {
            Logger.Debug("Try to create a new user");

            try
            {
                UserModel userModel = new(
                user,
                _userRepository.GetCountUsers());

                string isLeadString = userModel.IsLead ? "User is a lead" : "User is not a lead";
                Logger.Debug($"Attempt to create a user with login '{userModel.Login}' and other properties: firstName - '{userModel.FirstName}'; middleName - '{userModel.MiddleName}'; lastName - '{userModel.LastName}'; telephoneNumber - '{userModel.TelephoneNumber}'. {isLeadString}");

                _userRepository.Create(userModel);

                Logger.Debug($"User {userModel.Login} was created");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all properties available in the system.
        /// </summary>
        /// <returns>An IEnumerable of Property objects representing all properties.</returns>
        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Get all properties");
            return UserModel.GetPropertiesName();
            // NOTE : свойства формируются на основе модели, но была идея формировать их на основе базы данных
            // например с использованием Dapper используя системную таблицу
            // SELECT
            //     column_name AS Name,
            //     NULL AS Description
            // FROM
            //     information_schema.columns
            // WHERE
            //     table_name IN('User', 'Passwords') AND column_name NOT IN('id', 'userId', 'login')

        }

        /// <summary>
        /// Retrieves the properties of a specific user.
        /// </summary>
        /// <param name="userLogin">The login of the user to retrieve properties for.</param>
        /// <returns>An IEnumerable of UserProperty objects representing the user's properties.</returns>
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug($"Try get user properties for user '{userLogin}'");
            try
            {
                UserModel? user = _userRepository.GetUserByLogin(userLogin);
                if (user is null)
                {
                    Logger.Warn($"User '{userLogin}' not found");
                    throw new UserNotFoundException(userLogin);
                }

                return user.GetProperties();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Checks if a user with the specified login exists in the system.
        /// </summary>
        /// <param name="userLogin">The login of the user to check.</param>
        /// <returns>True if the user exists, otherwise false.</returns>
        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Try check exists user '{userLogin}'");

            try
            {
                bool userExists = _userRepository.CheckUserExists(userLogin);

                if (userExists)
                {
                    Logger.Debug($"User '{userLogin}' exists");
                }
                else
                {
                    Logger.Warn($"User '{userLogin}' not found");
                }

                return userExists;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Updates the properties of a specific user.
        /// </summary>
        /// <param name="properties">The properties to update for the user.</param>
        /// <param name="userLogin">The login of the user to update properties for.</param>
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug($"Try update user '{userLogin}' properties");

            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required);
                User? user = _userRepository.GetUser(userLogin);

                if (user is null)
                {
                    Logger.Error($"User '{userLogin}' not found");
                    throw new UserNotFoundException(userLogin);
                }

                SetProperties(properties, user);
                SetPassword(properties, userLogin);

                _context.SaveChanges();
                scope.Complete();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all permissions available in the system.
        /// </summary>
        /// <returns>An IEnumerable of Permission objects representing all permissions.</returns>
        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug("Try get all permissions in system");

            try
            {
                var roles = _permissionRepository.GetRolePermissions();
                var requests = _permissionRepository.GetRequestPermissions();

                Logger.Debug($"In system {roles.Count} IT role permissions and {requests.Count} request rights");

                return roles.Union(requests);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Adds the specified permissions to the user's roles and requests.
        /// </summary>
        /// <param name="userLogin">The login of the user to add permissions for.</param>
        /// <param name="rightIds">The IDs of the permissions to add.</param>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Try add permissions for user '{userLogin}'");

            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required);
                var user = _userRepository.GetUser(userLogin);
                if (user is null)
                {
                    Logger.Error($"User '{userLogin}' not found");
                    throw new UserNotFoundException(userLogin);
                }

                List<PermissionModel> permissions = rightIds.Select(right => new PermissionModel(right)).ToList();
                List<UserITRole> roles = GetRoles(userLogin, permissions);
                List<UserRequestRight> requests = GetRequests(userLogin, permissions);

                Logger.Debug($"Of the {permissions.Count} permissions for add, {roles.Count} relate to roles, and {requests.Count} relate to requests");

                _permissionRepository.AddRequestPermissions(requests);
                _permissionRepository.AddRolePermissions(roles);

                scope.Complete();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Removes the specified permissions from the user's roles and requests.
        /// </summary>
        /// <param name="userLogin">The login of the user to remove permissions for.</param>
        /// <param name="rightIds">The IDs of the permissions to remove.</param>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Try remove permissions for user '{userLogin}'");

            try
            {
                using var scope = new TransactionScope(TransactionScopeOption.Required);
                var user = _userRepository.GetUser(userLogin);
                if (user is null)
                {
                    Logger.Error($"User '{userLogin}' not found");
                    throw new UserNotFoundException(userLogin);
                }

                List<PermissionModel> permissions = rightIds.Select(right => new PermissionModel(right)).ToList();
                List<UserITRole> roles = GetRoles(userLogin, permissions);
                List<UserRequestRight> requests = GetRequests(userLogin, permissions);

                Logger.Debug($"Of the {permissions.Count} permissions for remove, {roles.Count} relate to roles, and {requests.Count} relate to requests");

                _permissionRepository.RemoveRequestPermissions(requests);
                _permissionRepository.RemoveRolePermissions(roles);

                scope.Complete();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the permissions associated with the specified user.
        /// </summary>
        /// <param name="userLogin">The login of the user to retrieve permissions for.</param>
        /// <returns>An IEnumerable of strings representing the user's permissions.</returns>
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Try get permissions for user '{userLogin}'");

            try
            {
                var userPermissions = _userRepository.GetPermissions(userLogin);
                int rolePermissionsCount = userPermissions.Count(permission => permission.Name == PermissionModel.ItRoleRightGroupName);
                int requestPermissionsCount = userPermissions.Count(permission => permission.Name == PermissionModel.RequestRightGroupName);
                Logger.Debug($"Of the {userPermissions.Count} user permissions, {rolePermissionsCount} relate to roles, and {requestPermissionsCount} relate to requests");

                return userPermissions.Select(permission => permission.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void SetProperties(IEnumerable<UserProperty> properties, User user)
        {
            foreach (var property in properties.Where(property => property.Name != PropertyName.Password))
            {
                if (UserModel.PropertyMap.TryGetValue(property.Name, out var propertyInfo) && propertyInfo is not null)
                {
                    if (propertyInfo.PropertyType == property.Value.GetType())
                    {
                        Logger.Debug($"Try set '{property.Name}' new value '{property.Value}' for user '{user.Login}'");
                        propertyInfo.SetValue(user, property.Value);
                    }
                    else
                    {
                        string errorMessage = $"Invalid property value type for '{property.Name}'. Expected: {propertyInfo.PropertyType}, Actual: {property.Value.GetType()}";
                        Logger.Error(errorMessage);
                        throw new InvalidOperationException(errorMessage);
                    }
                }
                else
                {
                    string errorMessage = $"Invalid property name: '{property.Name}'.";
                    Logger.Error(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }

        private void SetPassword(IEnumerable<UserProperty> properties, string userLogin)
        {
            string? password = properties.FirstOrDefault(property => property.Name == PropertyName.Password)?.Value;
            if (password is not null)
            {
                Logger.Debug($"Try set new password for user '{userLogin}'");
                Sequrity? sequrity = _userRepository.GetPassword(userLogin);
                if (sequrity is null)
                {
                    Logger.Error($"User '{userLogin}' has not password");
                    throw new UserNotFoundException(userLogin);
                }

                sequrity.Password = password;
            }
        }

        private static List<UserRequestRight> GetRequests(string userLogin, List<PermissionModel> permissions)
        {
            List<UserRequestRight> requests = new();
            foreach (var permission in permissions.Where(permission => permission.Name == PermissionModel.RequestRightGroupName))
            {
                requests.Add(new()
                {
                    RightId = permission.Id,
                    UserId = userLogin,
                });
            }

            return requests;
        }

        private static List<UserITRole> GetRoles(string userLogin, List<PermissionModel> permissions)
        {
            List<UserITRole> roles = new();
            foreach (var permission in permissions.Where(permission => permission.Name == PermissionModel.ItRoleRightGroupName))
            {
                roles.Add(new()
                {
                    RoleId = permission.Id,
                    UserId = userLogin,
                });
            }

            return roles;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }

        ~ConnectorDb()
        {
            Dispose(false);
        }
    }
}