using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TR.Connector.Configuration;
using TR.Connector.Helpers;
using TR.Connector.Models;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class Connector : IConnector
    {
        private static readonly HttpClient HttpClient = new();

        public ILogger? Logger { get; set; }

        private readonly ConnectorSettings _settings;

        private string _baseUrl = "";
        private string _token = "";

        public Connector() : this(Options.Create(new ConnectorSettings()))
        {
        }

        public Connector(IOptions<ConnectorSettings> options)
        {
            _settings = options.Value;
        }

        public async Task StartUpAsync(string connectionString)
        {
            Logger?.Debug("Строка подключения: " + connectionString);

            var parsedConnection = ConnectionStringParser.Parse(connectionString);
            _baseUrl = parsedConnection.Url;

            var body = new { login = parsedConnection.Login, password = parsedConnection.Password };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync($"{_baseUrl}/{_settings.Endpoints.Login}", content);

            if (!response.IsSuccessStatusCode)
            {
                Logger?.Error($"Ошибка аутентификации: {response.StatusCode}");
                throw new HttpRequestException($"Authentication failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
            _token = tokenResponse?.Data?.AccessToken ?? throw new Exception("Failed to get access token");
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
        {
            var request = new HttpRequestMessage(method, $"{_baseUrl}/{endpoint}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return request;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, HttpContent? content)
        {
            var request = CreateRequest(method, endpoint);
            request.Content = content;
            return request;
        }

        private async Task<UserResponseData?> GetUserByLoginAsync(string userLogin)
        {
            var usersResponse = await HttpClient.SendAsync(CreateRequest(HttpMethod.Get, _settings.Endpoints.UsersAll));
            usersResponse.EnsureSuccessStatusCode();
            var usersContent = await usersResponse.Content.ReadAsStringAsync();
            var userResponse = JsonSerializer.Deserialize<UserResponse>(usersContent);
            return userResponse!.Data!.FirstOrDefault(u => u.Login == userLogin);
        }

        private bool IsUserLocked(UserResponseData? user)
        {
            return user != null && user.Status == _settings.UserStatus.Locked;
        }

        private string GetPermissionEndpoint(string permissionId, string userLogin, bool isAdd)
        {
            var parts = permissionId.Split(',');
            if (parts.Length != 2)
                throw new ArgumentException($"Invalid permission ID format: {permissionId}. Expected format: 'Type,Id'");

            var permissionType = parts[0];
            var id = parts[1];

            if (permissionType == _settings.PermissionTypes.ItRole)
            {
                return isAdd
                    ? string.Format(_settings.Endpoints.AddRole, userLogin, id)
                    : string.Format(_settings.Endpoints.DropRole, userLogin, id);
            }

            if (permissionType == _settings.PermissionTypes.RequestRight)
            {
                return isAdd
                    ? string.Format(_settings.Endpoints.AddRight, userLogin, id)
                    : string.Format(_settings.Endpoints.DropRight, userLogin, id);
            }

            throw new ArgumentException($"Unknown permission type: {permissionType}");
        }

        private static string GetJsonPropertyName(System.Reflection.PropertyInfo propertyInfo)
        {
            var jsonAttr = propertyInfo.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                .FirstOrDefault() as JsonPropertyNameAttribute;
            return jsonAttr?.Name ?? propertyInfo.Name;
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            var rolesResponse = await HttpClient.SendAsync(CreateRequest(HttpMethod.Get, _settings.Endpoints.RolesAll));
            rolesResponse.EnsureSuccessStatusCode();
            var rolesContent = await rolesResponse.Content.ReadAsStringAsync();
            var itRoleResponse = JsonSerializer.Deserialize<RoleResponse>(rolesContent);
            var itRolePermissions =
                itRoleResponse!.Data!.Select(r => new Permission($"{_settings.PermissionTypes.ItRole},{r.Id}", r.Name, r.CorporatePhoneNumber ?? string.Empty));

            var rightsResponse = await HttpClient.SendAsync(CreateRequest(HttpMethod.Get, _settings.Endpoints.RightsAll));
            rightsResponse.EnsureSuccessStatusCode();
            var rightsContent = await rightsResponse.Content.ReadAsStringAsync();
            var rightResponse = JsonSerializer.Deserialize<RightResponse>(rightsContent);
            var rightPermissions = rightResponse!.Data!.Select(r =>
                new Permission($"{_settings.PermissionTypes.RequestRight},{r.Id}", r.Name, string.Empty));

            return itRolePermissions.Concat(rightPermissions);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userLogin, nameof(userLogin));

            var rolesResponse = await HttpClient.SendAsync(CreateRequest(HttpMethod.Get, string.Format(_settings.Endpoints.UserRoles, userLogin)));
            rolesResponse.EnsureSuccessStatusCode();
            var rolesContent = await rolesResponse.Content.ReadAsStringAsync();
            var itRoleResponse = JsonSerializer.Deserialize<UserRoleResponse>(rolesContent);
            var result1 = itRoleResponse!.Data!.Select(r => $"{_settings.PermissionTypes.ItRole},{r.Id}").ToList();

            var rightsResponse = await HttpClient.SendAsync(CreateRequest(HttpMethod.Get, string.Format(_settings.Endpoints.UserRights, userLogin)));
            rightsResponse.EnsureSuccessStatusCode();
            var rightsContent = await rightsResponse.Content.ReadAsStringAsync();
            var rightResponse = JsonSerializer.Deserialize<UserRightResponse>(rightsContent);
            var result2 = rightResponse!.Data!.Select(r => $"{_settings.PermissionTypes.RequestRight},{r.Id}").ToList();

            return result1.Concat(result2).ToList();
        }

        public async Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userLogin, nameof(userLogin));
            ArgumentNullException.ThrowIfNull(rightIds, nameof(rightIds));

            var user = await GetUserByLoginAsync(userLogin);

            if (IsUserLocked(user))
            {
                Logger?.Error($"Пользователь {userLogin} залочен.");
                return;
            }

            if (user != null && user.Status == _settings.UserStatus.Unlocked)
            {
                foreach (var rightId in rightIds)
                {
                    var endpoint = GetPermissionEndpoint(rightId, userLogin, isAdd: true);
                    var response = await HttpClient.SendAsync(CreateRequest(HttpMethod.Put, endpoint, null));
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userLogin, nameof(userLogin));
            ArgumentNullException.ThrowIfNull(rightIds, nameof(rightIds));

            var user = await GetUserByLoginAsync(userLogin);

            if (IsUserLocked(user))
            {
                Logger?.Error($"Пользователь {userLogin} залочен.");
                return;
            }

            if (user != null && user.Status == _settings.UserStatus.Unlocked)
            {
                foreach (var rightId in rightIds)
                {
                    var endpoint = GetPermissionEndpoint(rightId, userLogin, isAdd: false);
                    var response = await HttpClient.SendAsync(CreateRequest(HttpMethod.Delete, endpoint));
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return typeof(UserPropertyData).GetProperties()
                .Select(prop => GetJsonPropertyName(prop))
                .Where(name => name != "login")
                .Select(name => new Property(name, name))
                .ToList();
        }

        public async Task<IEnumerable<UserProperty>> GetUserPropertiesAsync(string userLogin)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userLogin, nameof(userLogin));

            var response = await HttpClient.SendAsync(CreateRequest(HttpMethod.Get, string.Format(_settings.Endpoints.UserByLogin, userLogin)));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(content);

            var user = userResponse!.Data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");

            if (user.Status == _settings.UserStatus.Locked)
                throw new Exception($"Невозможно получить свойства, пользователь {userLogin} залочен");

            return user.GetType().GetProperties()
                .Select(prop => new UserProperty(GetJsonPropertyName(prop), prop.GetValue(user)?.ToString() ?? string.Empty));
        }

        public async Task UpdateUserPropertiesAsync(IEnumerable<UserProperty> properties, string userLogin)
        {
            ArgumentNullException.ThrowIfNull(properties, nameof(properties));
            ArgumentException.ThrowIfNullOrWhiteSpace(userLogin, nameof(userLogin));

            var getResponse = await HttpClient.SendAsync(CreateRequest(HttpMethod.Get, string.Format(_settings.Endpoints.UserByLogin, userLogin)));
            getResponse.EnsureSuccessStatusCode();
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(getContent);

            var user = userResponse!.Data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");
            if (user.Status == _settings.UserStatus.Locked)
                throw new Exception($"Невозможно обновить свойства, пользователь {userLogin} залочен");

            foreach (var property in properties)
            {
                var userProp = user.GetType().GetProperties()
                    .FirstOrDefault(p => GetJsonPropertyName(p) == property.Name);
                userProp?.SetValue(user, property.Value);
            }

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
            var putResponse = await HttpClient.SendAsync(CreateRequest(HttpMethod.Put, _settings.Endpoints.EditUser, content));
            putResponse.EnsureSuccessStatusCode();
        }

        public async Task<bool> IsUserExistsAsync(string userLogin)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userLogin, nameof(userLogin));

            var user = await GetUserByLoginAsync(userLogin);
            return user != null;
        }

        public async Task CreateUserAsync(UserToCreate user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentException.ThrowIfNullOrWhiteSpace(user.Login, nameof(user.Login));

            var newUser = new CreateUserDto
            {
                Login = user.Login,
                Password = user.HashPassword,

                LastName = user.Properties.FirstOrDefault(p => p.Name.Equals("lastName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                FirstName = user.Properties.FirstOrDefault(p => p.Name.Equals("firstName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                MiddleName = user.Properties.FirstOrDefault(p => p.Name.Equals("middleName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,

                TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name.Equals("telephoneNumber", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                IsLead = bool.TryParse(user.Properties.FirstOrDefault(p => p.Name.Equals("isLead", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty, out bool isLeadValue)
                    ? isLeadValue
                    : false,

                Status = string.Empty
            };

            var content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");
            var response = await HttpClient.SendAsync(CreateRequest(HttpMethod.Post, _settings.Endpoints.CreateUser, content));
            response.EnsureSuccessStatusCode();
        }
    }
}
