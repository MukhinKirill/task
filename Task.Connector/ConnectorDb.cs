using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentResults;
using Task.Connector.Config.Parse;
using Task.Connector.Domain;
using Task.Connector.Storage;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorException : Exception
    {
        public ConnectorException() {}
        public ConnectorException(string s, params object[] args) : base(string.Format(s, args)) {}
    }
    
    public class ConnectorConnectException : ConnectorException
    {
        public ConnectorConnectException() {}
        public ConnectorConnectException(string s) : base(s) {}
        public ConnectorConnectException(string s, params object[] args) : base(s, args) {}
    }

    public class ConnectorNotInitializedException : ConnectorException
    {
        public ConnectorNotInitializedException() {}
        public ConnectorNotInitializedException(string s) : base(s) {}
        public ConnectorNotInitializedException(string s, params object[] args) : base(s, args) {}
    }
    
    public partial class ConnectorDb : IConnector
    {
        private IUserRepository? _userRepository;
        private IPermissionRepository? _permRepository;

        /// Copy pasted from unit test.
        /// Prefixes that allows to distinguish what client wants to add.
        
        /// UserITRole table  
        private const string ItRolePrefix = "Role";
        /// UserRequestRight table
        private const string RequestRightPrefix = "Request";
        
        /// <exception cref="ConnectorConnectException"></exception>
        public void StartUp(string connectionString)
        {
            ConnectionScheme scheme;
            try
            {
                scheme = ConnectionUrlParser.Default.Parse(connectionString);
            }
            catch (Exception)
            {
                throw new ConnectorConnectException("Connection url is malformed");
            }

            if (scheme.Domain.StartsWith("postgres"))
            {
                _userRepository = CreatePostgresUserRepository(scheme);
                _permRepository = CreatePostgresPermissionRepository(scheme);
            }
            else
            {
                // @todo we can register more providers here
                throw new ConnectorConnectException("Unknown data provider: \"{0}\"", scheme.Domain);
            }
        }

        private void ThrowIfNotInitialized()
        {
            if (_userRepository is null || _permRepository is null)
            {
                throw new ConnectorNotInitializedException();
            }
        }

        public void CreateUser(UserToCreate user)
        {
            ThrowIfNotInitialized();

            Result result;
            try
            {
                Logger.Debug($"Exec: CreateUser: [UserLogin = {user.Login}]");
                
                result = _userRepository
                    .CreateUserAsync(user, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                Logger.Error($"CreateUser: [UserLogin = {user.Login}] Exception: {e}");
                return;
            }

            if (result.IsFailed)
            {
                Logger.Error($"CreateUser: [UserLogin = {user.Login}] Error: {string.Join(',', result.Errors.Select(x => x.Message))}");
            }
            else
            {
                Logger.Debug($"CreateUser: OK [Login = {user.Login}]");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            ThrowIfNotInitialized();

            Result<IEnumerable<Property>> result;
            try
            {
                Logger.Debug("Exec: GetAllProperties");

                result = _userRepository
                    .GetAllPropertiesAsync(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                Logger.Error($"GetAllProperties: Exception: {e}");
                return Array.Empty<Property>();
            }
            
            if (result.IsFailed)
            {
                Logger.Error($"GetAllProperties: Error: {string.Join(',', result.Errors.Select(x => x.Message))}");
            }
            else
            {
                Logger.Debug($"GetAllProperties: OK");
            }

            return result.IsSuccess ? result.Value : Array.Empty<Property>();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            ThrowIfNotInitialized();

            Result<IEnumerable<UserProperty>> result;
            try
            {
                Logger.Debug($"Exec: GetUserProperties: [UserLogin = {userLogin}]");
                
                var req = new GetGetUserPropertiesDto(userLogin);
                result = _userRepository
                    .GetUserPropertiesAsync(req, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                Logger.Error($"GetUserProperties: [UserLogin = {userLogin}] Exception: {e}");
                return Array.Empty<UserProperty>();
            }

            if (result.IsFailed)
            {
                Logger.Error($"GetUserProperties: [UserLogin = {userLogin}] Error: {string.Join(',', result.Errors.Select(x => x.Message))}");
            }
            else
            {
                Logger.Debug($"GetUserProperties: OK [UserLogin = {userLogin}]");
            }

            return result.IsSuccess ? result.Value : Array.Empty<UserProperty>();
        }

        public bool IsUserExists(string userLogin)
        {
            ThrowIfNotInitialized();

            Result<bool> result;
            try
            {
                Logger.Debug($"Exec: IsUserExists: [UserLogin = {userLogin}]");
                
                var req = new GetUserExistDto(userLogin);
                result = _userRepository.DoesUserExist(req, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                Logger.Error($"IsUserExist: [UserLogin = {userLogin}] Exception: {e}");
                return false;
            }

            if (result.IsFailed)
            {
                Logger.Error($"IsUserExist: [UserLogin = {userLogin}] Error: {string.Join(',', result.Errors.Select(x => x.Message))}");
            }
            else
            {
                Logger.Debug($"IsUserExist: OK [UserLogin = {userLogin}]");
            }

            return result.IsSuccess && result.Value;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            ThrowIfNotInitialized();

            Result result;
            try
            {
                Logger.Debug($"Exec: UpdateUserProperties: [UserLogin = {userLogin}]");
                
                var req = new UpdateGetUserPropertiesDto(userLogin, properties);
                result = _userRepository.UpdateUserProperties(req, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                Logger.Error($"UpdateUserProperties: [UserLogin = {userLogin}] Exception: {e}");
                return;
            }
            
            if (result.IsFailed)
            {
                Logger.Error($"UpdateUserProperties: [UserLogin = {userLogin}] Error: {string.Join(',', result.Errors.Select(x => x.Message))}");
            }
            else
            {
                Logger.Error($"UpdateUserProperties: OK [UserLogin = {userLogin}]");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            ThrowIfNotInitialized();

            Result<IEnumerable<Permission>> result;
            try
            {
                Logger.Debug("Exec: GetAllPermissions");
                
                result = _permRepository.GetAllPermissionsAsync(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                Logger.Error($"GetAllPermissions: Exception: {e}");
                return Array.Empty<Permission>();
            }

            if (result.IsFailed)
            {
                Logger.Error($"GetAllPermissions: Error: {string.Join(',', result.Errors.Select(x => x.Message))}");
            }
            else
            {
                Logger.Debug("GetAllPermissions: OK");
            }

            return result.IsSuccess ? result.Value : Array.Empty<Permission>();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            ThrowIfNotInitialized();

            Logger.Debug($"Exec: AddUserPermissions [UserLogin = {userLogin}]");
            
            var roles = new List<int>();
            var perms = new List<int>();

            foreach (var rightId in rightIds)
            {
                if (rightId.StartsWith(ItRolePrefix))
                {
                    var offset = ItRolePrefix.Length;
                    if (int.TryParse(rightId.Substring(offset + 1), out var roleId))
                    {
                        roles.Add(roleId);
                    }
                }
                else if (rightId.StartsWith(RequestRightPrefix))
                {
                    var offset = RequestRightPrefix.Length;
                    if (int.TryParse(rightId.Substring(offset + 1), out var permId))
                    {
                        perms.Add(permId);
                    }
                }
            }

            if (!roles.Any() && !perms.Any())
            {
                Logger.Warn("AddUserPermissions: Empty list.");
                return;
            }
            
            if (roles.Any())
            {
                Result? result = null;
                try
                {
                    var req = new AddUserRolesDto(userLogin, roles);
                    
                    result = _permRepository.AddUserRoleAsync(req, CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception e)
                {
                    Logger.Error($"AddUserPermissions: Error on AddRoles: {string.Join(',', result!.Errors.Select(x => x.Message))}");
                }

                if (result.IsSuccess)
                {
                    Logger.Debug($"AddUserPermissions: OK [UserLogin = {userLogin}]");
                }
            }

            if (perms.Any())
            {
                Result? result = null;
                try
                {
                    var req = new AddUserPermissionsDto(userLogin, roles);
                    
                    result = _permRepository.AddUserPermissionAsync(req, CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception e)
                {
                    Logger.Error($"AddUserPermissions: Error on AddPermissions: {string.Join(',', result!.Errors.Select(x => x.Message))}");
                }
                
                if (result.IsSuccess)
                {
                    Logger.Debug($"AddUserPermissions: OK [UserLogin = {userLogin}]");
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            ThrowIfNotInitialized();

            Logger.Debug($"Exec: RemoveUserPermissions [UserLogin = {userLogin}]");
            
            var roles = new List<int>();
            var perms = new List<int>();

            foreach (var rightId in rightIds)
            {
                if (rightId.StartsWith(ItRolePrefix))
                {
                    var offset = ItRolePrefix.Length;
                    if (int.TryParse(rightId.Substring(offset + 1), out var roleId))
                    {
                        roles.Add(roleId);
                    }
                }
                else if (rightId.StartsWith(RequestRightPrefix))
                {
                    var offset = RequestRightPrefix.Length;
                    if (int.TryParse(rightId.Substring(offset + 1), out var permId))
                    {
                        perms.Add(permId);
                    }
                }
            }

            if (!roles.Any() && !perms.Any())
            {
                Logger.Warn("RemoveUserPermissions: Empty list.");
                return;
            }
            
            if (roles.Any())
            {
                Result? result = null;
                try
                {
                    var req = new AddUserRolesDto(userLogin, roles);
                    
                    result = _permRepository.RemoveUserRoleAsync(req, CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception e)
                {
                    Logger.Error($"RemoveUserPermissions: Error on RemoveRoles: {string.Join(',', result!.Errors.Select(x => x.Message))}");
                }

                if (result.IsSuccess)
                {
                    Logger.Debug($"RemoveUserPermissions: OK [UserLogin = {userLogin}]");
                }
            }

            if (perms.Any())
            {
                Result? result = null;
                try
                {
                    var req = new AddUserPermissionsDto(userLogin, perms);
                    
                    result = _permRepository.RemoveUserPermissionAsync(req, CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception e)
                {
                    Logger.Error($"RemoveUserPermissions: Error on RemovePermissions: {string.Join(',', result!.Errors.Select(x => x.Message))}");
                }
                
                if (result.IsSuccess)
                {
                    Logger.Debug($"RemoveUserPermissions: OK [UserLogin = {userLogin}]");
                }
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            ThrowIfNotInitialized();

            Result<IEnumerable<string>> result;
            try
            {
                Logger.Debug($"Exec: GetUserPermissions [UserLogin = {userLogin}");
                
                var req = new GetUserPermissionsDto(userLogin);
                result = _permRepository.GetUserPermissionsAsync(req, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                Logger.Error($"GetUserPermissions: [UserLogin = {userLogin}] Exception: {e}");
                return Array.Empty<string>();
            }
            
            if (result.IsFailed)
            {
                Logger.Error($"GetUserPermissions: [UserLogin = {userLogin}] Error: {string.Join(',', result.Errors.Select(x => x.Message))}");
            }
            else
            {
                Logger.Debug($"GetUserPermissions: OK [UserLogin = {userLogin}]");
            }

            return result.IsSuccess ? result.Value : Array.Empty<string>();
        }

        public ILogger Logger { get; set; }
    }
}