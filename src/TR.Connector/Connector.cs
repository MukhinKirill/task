using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector;

public partial class Connector : IConnector
{
    public ILogger Logger { get; set; }
    private string _url = string.Empty;
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _token = string.Empty;

    private HttpClient _httpClient;

    //Пустой конструктор
    public Connector() { }

    #region Startup

        public async Task StartUpAsync(string connectionString)
        {
            ParseConnectionString(connectionString);

            Logger.Debug($"Строка подключения: {connectionString}");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_url)
            };

            // Аутентификация
            var body = new { login = _login, password = _password };
            var tokenResponse = await PostAsync<TokenResponse>("api/v1/login", body);

            _token = tokenResponse?.data.access_token 
                     ?? throw new Exception("Не удалось получить токен");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
        }

        private void ParseConnectionString(string connectionString)
        {
            foreach (var item in connectionString.Split(';'))
            {
                var parts = item.Split('=');
                if (parts.Length != 2) continue;

                switch (parts[0])
                {
                    case "url": _url = parts[1]; break;
                    case "login": _login = parts[1]; break;
                    case "password": _password = parts[1]; break;
                }
            }
        }

    #endregion

    #region Permissions
        
        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            // ИТ-роли
            var roles = await GetAsync<RoleResponse>("api/v1/roles/all");
            var rolePermissions = roles?.data.Select(r =>
                                      new Permission($"ItRole,{r.id}", r.name, r.corporatePhoneNumber))
                                      ?? Enumerable.Empty<Permission>();
            // Права
            var rights = await GetAsync<RightResponse>("api/v1/rights/all");
            var rightPermissions = rights?.data.Select(r =>
                                       new Permission($"RequestRight,{r.id}", r.name, r.users?.ToString() ?? string.Empty))
                                       ?? Enumerable.Empty<Permission>();
            
            return rolePermissions.Concat(rightPermissions);
            
        }
        
        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin)
        {
            var roles = await GetAsync<UserRoleResponse>($"api/v1/users/{userLogin}/roles");
            var roleResult = roles?.data.Select(r =>
                                 $"ItRole,{r.id}, {r.name}, {r.corporatePhoneNumber}")
                                 ?? Enumerable.Empty<string>();
            
            var rights = await GetAsync<UserrightResponse>($"api/v1/users/{userLogin}/rights");
            var rightResult = rights?.data.Select(r =>
                                  $"RequestRight,{r.id}, {r.name}, {r.users?.ToString() ?? string.Empty}")
                                  ?? Enumerable.Empty<string>();
            
            return roleResult.Concat(rightResult);
            
        }
        
        public async Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            await EnsureUserUnlockedAsync(userLogin);
            
            foreach (var (type, id) in ParseRights(rightIds))
            {
                switch (type)
                {
                    case "ItRole":
                        await _httpClient.PutAsync($"api/v1/users/{userLogin}/add/role/{id}", null);
                        break;
                    
                    case "RequestRight":
                        await _httpClient.PutAsync($"api/v1/users/{userLogin}/add/right/{id}", null);
                        break;
                    
                    default:
                        throw new Exception($"Тип доступа {type} не определён");
                }
            }
        }
        
        public async Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            
            await EnsureUserUnlockedAsync(userLogin);
            
            foreach (var (type, id) in ParseRights(rightIds))
            {
                switch (type)
                {
                    case "ItRole":
                        await _httpClient.DeleteAsync($"api/v1/users/{userLogin}/drop/role/{id}");
                        break;
                    
                    case "RequestRight":
                        await _httpClient.DeleteAsync($"api/v1/users/{userLogin}/drop/right/{id}");
                        break;
                    
                    default:
                        throw new Exception($"Тип доступа {type} не определён");
                }
            }
        }
        
        private IEnumerable<(string type, string id)> ParseRights(IEnumerable<string> rightIds)
        {
            foreach (var right in rightIds)
            {
                var parts = right.Split(',');
                if (parts.Length < 2)
                    throw new FormatException($"Некорректный формат права: {right}");
                
                yield return (parts[0], parts[1]);
                
            }
        }
        
    #endregion

    #region Users
        
        public IEnumerable<Property> GetAllProperties()
        {
            return typeof(UserPropertyData)
                .GetProperties()
                .Where(p => p.Name != "login")
                .Select(p => new Property(p.Name, p.Name));
        }

        public async Task<IEnumerable<UserProperty>> GetUserPropertiesAsync(string userLogin)
        {
            var user = await GetUserAsync(userLogin);

            return user.GetType()
                .GetProperties()
                .Select(p => new UserProperty(p.Name, p.GetValue(user)?.ToString() ?? string.Empty));

        }

        public async Task UpdateUserPropertiesAsync(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = await GetUserAsync(userLogin);
            
            foreach (var prop in properties)
            {
                var userProp = user.GetType().GetProperty(prop.Name);
                userProp?.SetValue(user, prop.Value);
            }
            
            await PutAsync("api/v1/users/edit", user);
            
        }
        
        public async ValueTask<bool> IsUserExistsAsync(string userLogin)
        {
            var users = await GetAsync<UserResponse>("api/v1/users/all");
            return users?.data.Any(u => u.login == userLogin) == true;
        }
        
        public async Task CreateUserAsync(UserToCreate user)
        {
            var dto = new CreateUSerDTO
            {
                login = user.Login,
                password = user.HashPassword,
                lastName = GetProp(user, "lastName"),
                firstName = GetProp(user, "firstName"),
                middleName = GetProp(user, "middleName"),
                telephoneNumber = GetProp(user, "telephoneNumber"),
                isLead = bool.TryParse(GetProp(user, "isLead"), out var lead) && lead,
                status = string.Empty
            };
            
            await PostAsync("api/v1/users/create", dto);
        }
        
    #endregion
        
    #region Helpers
        
        private async Task<UserPropertyData> GetUserAsync(string userLogin)
        {
            var response = await GetAsync<UserPropertyResponse>($"api/v1/users/{userLogin}");
            var user = response?.data ?? throw new Exception($"Пользователь {userLogin} не найден");
            
            if (user.status == "Lock")
                throw new Exception($"Пользователь {userLogin} залочен");
            
            return user;
        }
        
        private async Task EnsureUserUnlockedAsync(string userLogin)
        {
            var users = await GetAsync<UserResponse>("api/v1/users/all");
            var user = users?.data.FirstOrDefault(u => u.login == userLogin);
            
            if (user == null)
                throw new Exception($"Пользователь {userLogin} не найден");
            
            if (user.status == "Lock")
            {
                Logger.Error($"Пользователь {userLogin} залочен.");
                throw new Exception("Операция невозможна");
            }
        }
        
        private static string GetProp(UserToCreate user, string name)
        {
            return user.Properties
                    .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ?.Value ?? string.Empty;
        }
        
    #endregion
        
    #region Http
        
        private async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        
        private async Task<T> PostAsync<T>(string url, object body)
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8,
                "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        
        private async Task PostAsync(string url, object body)
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8,
                "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        
        private async Task PutAsync(string url, object body)
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8,
                "application/json");
            
            var response = await _httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        
    #endregion
}
