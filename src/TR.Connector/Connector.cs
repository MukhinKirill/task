using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Entities.DTO.Requests;
using TR.Connectors.Api.Entities.Model;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class Connector : IConnector
    {
        private readonly IAuthService? _authService;
        private readonly IPermissionService? _permissionService;
        private readonly IUserService? _userService;
        private readonly ILogger<Connector>? _logger;

        public Connector() { }

        public Connector(
            IAuthService authService,
            IPermissionService permissionService,
            IUserService userService,
            ILogger<Connector> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger;
        }

        public Task StartUpAsync(string connectionString)
        {
            _logger?.LogDebug("Connector.StartUp called");
            if (_authService == null) throw new InvalidOperationException("AuthService is not available — configure DI");
            return _authService.StartUpAsync(connectionString, CancellationToken.None);
        }

        public Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            if (_permissionService == null) throw new InvalidOperationException("PermissionService is not available — configure DI");
            return _permissionService.GetAllPermissionsAsync(CancellationToken.None);
        }

        public Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin)
        {
            if (_permissionService == null) throw new InvalidOperationException("PermissionService is not available — configure DI");
            return _permissionService.GetUserPermissionsAsync(userLogin, CancellationToken.None);
        }

        public Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            if (_permissionService == null) throw new InvalidOperationException("PermissionService is not available — configure DI");
            return _permissionService.AddUserPermissionsAsync(userLogin, rightIds, CancellationToken.None);
        }

        public Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            if (_permissionService == null) throw new InvalidOperationException("PermissionService is not available — configure DI");
            return _permissionService.RemoveUserPermissionsAsync(userLogin, rightIds, CancellationToken.None);
        }

        public IEnumerable<Property> GetAllProperties()
        {
            if (_userService == null) throw new InvalidOperationException("UserService is not available — configure DI");
            return _userService.GetAllProperties();
        }
        
        public Task<IEnumerable<UserProperty>> GetUserPropertiesAsync(string userLogin, CancellationToken ct = default)
        {
            if (_userService == null) throw new InvalidOperationException("UserService is not available — configure DI");
            return _userService.GetUserPropertiesAsync(userLogin, ct);
        }

        public Task UpdateUserPropertiesAsync(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (_userService == null) throw new InvalidOperationException("UserService is not available — configure DI");
            return _userService.UpdateUserPropertiesAsync(properties, userLogin, CancellationToken.None);
        }

        public Task<bool> IsUserExistsAsync(string userLogin)
        {
            if (_userService == null) throw new InvalidOperationException("UserService is not available — configure DI");
            return _userService.IsUserExistsAsync(userLogin, CancellationToken.None);
        }

        public Task CreateUserAsync(UserToCreate user)
        {
            if (_userService == null) throw new InvalidOperationException("UserService is not available — configure DI");
            return _userService.CreateUserAsync(user, CancellationToken.None);
        }

        public Task LockUserAsync(string userLogin)
        {
            if (_userService == null) throw new InvalidOperationException("UserService is not available — configure DI");
            return _userService.LockUserAsync(userLogin, CancellationToken.None);
        }

        public Task UnlockUserAsync(string userLogin)
        {
            if (_userService == null) throw new InvalidOperationException("UserService is not available — configure DI");
            return _userService.UnlockUserAsync(userLogin, CancellationToken.None);
        }
    }
}