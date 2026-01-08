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
        public ILogger Logger { get; set; }

        private readonly ConnectorSettings _settings;

        private string _url = "";
        private string _login = "";
        private string _password = "";
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
            Logger.Debug("Строка подключения: " + connectionString);
            foreach (var item in connectionString.Split(';'))
            {
                if (item.StartsWith("url")) _url = item.Split('=')[1];
                if (item.StartsWith("login")) _login = item.Split('=')[1];
                if (item.StartsWith("password")) _password = item.Split('=')[1];
            }

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            var body = new { login = _login, password = _password };
            var content = new StringContent(JsonSerializer.Serialize(body), UnicodeEncoding.UTF8, "application/json");
            var response = httpClient.PostAsync(_settings.Endpoints.Login, content).Result;
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content.ReadAsStringAsync().Result);
            _token = tokenResponse!.Data!.AccessToken;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = httpClient.GetAsync(_settings.Endpoints.RolesAll).Result;
            var itRoleResponse = JsonSerializer.Deserialize<RoleResponse>(response.Content.ReadAsStringAsync().Result);
            var itRolePermissions =
                itRoleResponse!.Data!.Select(_ => new Permission($"{_settings.PermissionTypes.ItRole},{_.Id}", _.Name, _.CorporatePhoneNumber));

            response = httpClient.GetAsync(_settings.Endpoints.RightsAll).Result;
            var rightResponse = JsonSerializer.Deserialize<RightResponse>(response.Content.ReadAsStringAsync().Result);
            var rightPermissions = rightResponse!.Data!.Select(_ =>
                new Permission($"{_settings.PermissionTypes.RequestRight},{_.Id}", _.Name, null));

            return itRolePermissions.Concat(rightPermissions);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = httpClient.GetAsync(string.Format(_settings.Endpoints.UserRoles, userLogin)).Result;
            var itRoleResponse = JsonSerializer.Deserialize<UserRoleResponse>(response.Content.ReadAsStringAsync().Result);
            var result1 = itRoleResponse!.Data!.Select(_ => $"{_settings.PermissionTypes.ItRole},{_.Id}").ToList();

            response = httpClient.GetAsync(string.Format(_settings.Endpoints.UserRights, userLogin)).Result;
            var rightResponse = JsonSerializer.Deserialize<UserRightResponse>(response.Content.ReadAsStringAsync().Result);
            var result2 = rightResponse!.Data!.Select(_ => $"{_settings.PermissionTypes.RequestRight},{_.Id}").ToList();

            return result1.Concat(result2).ToList();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = httpClient.GetAsync(_settings.Endpoints.UsersAll).Result;
            var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
            var user = userResponse!.Data!.FirstOrDefault(_ => _.Login == userLogin);

            if (user != null && user.Status == _settings.UserStatus.Locked)
            {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
            }

            if (user != null && user.Status == _settings.UserStatus.Unlocked)
            {
                foreach (var rightId in rightIds)
                {
                    var rightStr = rightId.Split(',');
                    if (rightStr[0] == _settings.PermissionTypes.ItRole)
                    {
                        httpClient.PutAsync(string.Format(_settings.Endpoints.AddRole, userLogin, rightStr[1]), null).Wait();
                    }
                    else if (rightStr[0] == _settings.PermissionTypes.RequestRight)
                    {
                        httpClient.PutAsync(string.Format(_settings.Endpoints.AddRight, userLogin, rightStr[1]), null).Wait();
                    }
                    else
                    {
                        throw new Exception($"Тип доступа {rightStr[0]} не определен");
                    }
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = httpClient.GetAsync(_settings.Endpoints.UsersAll).Result;
            var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
            var user = userResponse!.Data!.FirstOrDefault(_ => _.Login == userLogin);

            if (user != null && user.Status == _settings.UserStatus.Locked)
            {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
            }

            if (user != null && user.Status == _settings.UserStatus.Unlocked)
            {
                foreach (var rightId in rightIds)
                {
                    var rightStr = rightId.Split(',');
                    if (rightStr[0] == _settings.PermissionTypes.ItRole)
                    {
                        httpClient.DeleteAsync(string.Format(_settings.Endpoints.DropRole, userLogin, rightStr[1])).Wait();
                    }
                    else if (rightStr[0] == _settings.PermissionTypes.RequestRight)
                    {
                        httpClient.DeleteAsync(string.Format(_settings.Endpoints.DropRight, userLogin, rightStr[1])).Wait();
                    }
                    else
                    {
                        throw new Exception($"Тип доступа {rightStr[0]} не определен");
                    }
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
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = httpClient.GetAsync(string.Format(_settings.Endpoints.UserByLogin, userLogin)).Result;
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
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = httpClient.GetAsync(string.Format(_settings.Endpoints.UserByLogin, userLogin)).Result;
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(response.Content.ReadAsStringAsync().Result);

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

            var content = new StringContent(JsonSerializer.Serialize(user), UnicodeEncoding.UTF8, "application/json");
            httpClient.PutAsync(_settings.Endpoints.EditUser, content).Wait();
        }

        public bool IsUserExists(string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = httpClient.GetAsync(_settings.Endpoints.UsersAll).Result;
            var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
            var user = userResponse!.Data!.FirstOrDefault(u => u.Login == userLogin);

            return user != null;
        }

        public void CreateUser(UserToCreate user)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

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

            var content = new StringContent(JsonSerializer.Serialize(newUser), UnicodeEncoding.UTF8, "application/json");
            httpClient.PostAsync(_settings.Endpoints.CreateUser, content).Wait();
        }
    }
}
