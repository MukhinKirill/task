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
    public class ConnectorDb : IConnector, IDisposable
    {
        private bool _disposed;
        private DataContext _context = null!;
        private UserRepository _userRepository = null!;
        private PermissionRepository _permissionRepository = null!;
        public ILogger Logger { get; set; } = null!;

        public void StartUp(string connectionString)
        {
            DataContextFactory factory = new(connectionString, Logger);
            _context = factory.GetContext();
            _userRepository = new(_context);
            _permissionRepository = new(_context);
        }

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

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Get all properties");
            return UserModel.GetPropertiesName();
        }

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