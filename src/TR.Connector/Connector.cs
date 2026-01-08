using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TR.Connector.Configuration;
using TR.Connector.Models;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class Connector : IConnector
    {
        private static readonly HttpClient HttpClient = new();

        public ILogger Logger { get; set; }

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

        public void StartUp(string connectionString)
        {
            Logger?.Debug("Строка подключения: " + connectionString);

            string login = "", password = "";
            foreach (var item in connectionString.Split(';'))
            {
                if (item.StartsWith("url")) _baseUrl = item.Split('=')[1].TrimEnd('/');
                if (item.StartsWith("login")) login = item.Split('=')[1];
                if (item.StartsWith("password")) password = item.Split('=')[1];
            }

            var body = new { login, password };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = HttpClient.PostAsync($"{_baseUrl}/{_settings.Endpoints.Login}", content).Result;

            if (!response.IsSuccessStatusCode)
            {
                Logger?.Error($"Ошибка аутентификации: {response.StatusCode}");
                throw new HttpRequestException($"Authentication failed: {response.StatusCode}");
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content.ReadAsStringAsync().Result);
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

        public IEnumerable<Permission> GetAllPermissions()
        {
            var rolesResponse = HttpClient.Send(CreateRequest(HttpMethod.Get, _settings.Endpoints.RolesAll));
            rolesResponse.EnsureSuccessStatusCode();
            var itRoleResponse = JsonSerializer.Deserialize<RoleResponse>(rolesResponse.Content.ReadAsStringAsync().Result);
            var itRolePermissions =
                itRoleResponse!.Data!.Select(r => new Permission($"{_settings.PermissionTypes.ItRole},{r.Id}", r.Name, r.CorporatePhoneNumber));

            var rightsResponse = HttpClient.Send(CreateRequest(HttpMethod.Get, _settings.Endpoints.RightsAll));
            rightsResponse.EnsureSuccessStatusCode();
            var rightResponse = JsonSerializer.Deserialize<RightResponse>(rightsResponse.Content.ReadAsStringAsync().Result);
            var rightPermissions = rightResponse!.Data!.Select(r =>
                new Permission($"{_settings.PermissionTypes.RequestRight},{r.Id}", r.Name, null));

            return itRolePermissions.Concat(rightPermissions);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var rolesResponse = HttpClient.Send(CreateRequest(HttpMethod.Get, string.Format(_settings.Endpoints.UserRoles, userLogin)));
            rolesResponse.EnsureSuccessStatusCode();
            var itRoleResponse = JsonSerializer.Deserialize<UserRoleResponse>(rolesResponse.Content.ReadAsStringAsync().Result);
            var result1 = itRoleResponse!.Data!.Select(r => $"{_settings.PermissionTypes.ItRole},{r.Id}").ToList();

            var rightsResponse = HttpClient.Send(CreateRequest(HttpMethod.Get, string.Format(_settings.Endpoints.UserRights, userLogin)));
            rightsResponse.EnsureSuccessStatusCode();
            var rightResponse = JsonSerializer.Deserialize<UserRightResponse>(rightsResponse.Content.ReadAsStringAsync().Result);
            var result2 = rightResponse!.Data!.Select(r => $"{_settings.PermissionTypes.RequestRight},{r.Id}").ToList();

            return result1.Concat(result2).ToList();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var usersResponse = HttpClient.Send(CreateRequest(HttpMethod.Get, _settings.Endpoints.UsersAll));
            usersResponse.EnsureSuccessStatusCode();
            var userResponse = JsonSerializer.Deserialize<UserResponse>(usersResponse.Content.ReadAsStringAsync().Result);
            var user = userResponse!.Data!.FirstOrDefault(u => u.Login == userLogin);

            if (user != null && user.Status == _settings.UserStatus.Locked)
            {
                Logger?.Error($"Пользователь {userLogin} залочен.");
                return;
            }

            if (user != null && user.Status == _settings.UserStatus.Unlocked)
            {
                foreach (var rightId in rightIds)
                {
                    var rightStr = rightId.Split(',');
                    string endpoint;
                    if (rightStr[0] == _settings.PermissionTypes.ItRole)
                    {
                        endpoint = string.Format(_settings.Endpoints.AddRole, userLogin, rightStr[1]);
                    }
                    else if (rightStr[0] == _settings.PermissionTypes.RequestRight)
                    {
                        endpoint = string.Format(_settings.Endpoints.AddRight, userLogin, rightStr[1]);
                    }
                    else
                    {
                        throw new Exception($"Тип доступа {rightStr[0]} не определен");
                    }

                    var response = HttpClient.Send(CreateRequest(HttpMethod.Put, endpoint, null));
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var usersResponse = HttpClient.Send(CreateRequest(HttpMethod.Get, _settings.Endpoints.UsersAll));
            usersResponse.EnsureSuccessStatusCode();
            var userResponse = JsonSerializer.Deserialize<UserResponse>(usersResponse.Content.ReadAsStringAsync().Result);
            var user = userResponse!.Data!.FirstOrDefault(u => u.Login == userLogin);

            if (user != null && user.Status == _settings.UserStatus.Locked)
            {
                Logger?.Error($"Пользователь {userLogin} залочен.");
                return;
            }

            if (user != null && user.Status == _settings.UserStatus.Unlocked)
            {
                foreach (var rightId in rightIds)
                {
                    var rightStr = rightId.Split(',');
                    string endpoint;
                    if (rightStr[0] == _settings.PermissionTypes.ItRole)
                    {
                        endpoint = string.Format(_settings.Endpoints.DropRole, userLogin, rightStr[1]);
                    }
                    else if (rightStr[0] == _settings.PermissionTypes.RequestRight)
                    {
                        endpoint = string.Format(_settings.Endpoints.DropRight, userLogin, rightStr[1]);
                    }
                    else
                    {
                        throw new Exception($"Тип доступа {rightStr[0]} не определен");
                    }

                    var response = HttpClient.Send(CreateRequest(HttpMethod.Delete, endpoint));
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var props = new List<Property>();
            foreach (var propertyInfo in typeof(UserPropertyData).GetProperties())
            {
                var jsonAttr = propertyInfo.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                    .FirstOrDefault() as System.Text.Json.Serialization.JsonPropertyNameAttribute;
                var jsonName = jsonAttr?.Name ?? propertyInfo.Name;

                if (jsonName == "login") continue;

                props.Add(new Property(jsonName, jsonName));
            }
            return props;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var response = HttpClient.Send(CreateRequest(HttpMethod.Get, string.Format(_settings.Endpoints.UserByLogin, userLogin)));
            response.EnsureSuccessStatusCode();
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(response.Content.ReadAsStringAsync().Result);

            var user = userResponse!.Data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");

            if (user.Status == _settings.UserStatus.Locked)
                throw new Exception($"Невозможно получить свойства, пользователь {userLogin} залочен");

            return user.GetType().GetProperties()
                .Select(prop =>
                {
                    var jsonAttr = prop.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                        .FirstOrDefault() as System.Text.Json.Serialization.JsonPropertyNameAttribute;
                    var jsonName = jsonAttr?.Name ?? prop.Name;
                    return new UserProperty(jsonName, prop.GetValue(user)?.ToString());
                });
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var getResponse = HttpClient.Send(CreateRequest(HttpMethod.Get, string.Format(_settings.Endpoints.UserByLogin, userLogin)));
            getResponse.EnsureSuccessStatusCode();
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(getResponse.Content.ReadAsStringAsync().Result);

            var user = userResponse!.Data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");
            if (user.Status == _settings.UserStatus.Locked)
                throw new Exception($"Невозможно обновить свойства, пользователь {userLogin} залочен");

            foreach (var property in properties)
            {
                foreach (var userProp in user.GetType().GetProperties())
                {
                    var jsonAttr = userProp.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                        .FirstOrDefault() as System.Text.Json.Serialization.JsonPropertyNameAttribute;
                    var jsonName = jsonAttr?.Name ?? userProp.Name;

                    if (property.Name == jsonName)
                    {
                        userProp.SetValue(user, property.Value);
                    }
                }
            }

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
            var putResponse = HttpClient.Send(CreateRequest(HttpMethod.Put, _settings.Endpoints.EditUser, content));
            putResponse.EnsureSuccessStatusCode();
        }

        public bool IsUserExists(string userLogin)
        {
            var response = HttpClient.Send(CreateRequest(HttpMethod.Get, _settings.Endpoints.UsersAll));
            response.EnsureSuccessStatusCode();
            var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
            var user = userResponse!.Data!.FirstOrDefault(u => u.Login == userLogin);

            return user != null;
        }

        public void CreateUser(UserToCreate user)
        {
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
            var response = HttpClient.Send(CreateRequest(HttpMethod.Post, _settings.Endpoints.CreateUser, content));
            response.EnsureSuccessStatusCode();
        }
    }
}
