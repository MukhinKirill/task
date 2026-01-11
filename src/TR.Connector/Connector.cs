using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public partial class Connector : IConnector
    {
        public ILogger Logger { get; set; }

        private const string BearerScheme = "Bearer";
        private const string ApplicationJsonMediaType = "application/json";
        private const string LockedStatus = "Lock";
        private const string UnlockedStatus = "Unlock";
        private const string ItRolePrefix = "ItRole";
        private const string RequestRightPrefix = "RequestRight";

        private string url = string.Empty;
        private string login = string.Empty;
        private string password = string.Empty;


        private string token = string.Empty;

        //Пустой конструктор
        public Connector() { }

        public void StartUp(string connectionString)
        {
            Logger.Debug("Строка подключения: " + connectionString);

            ParseConnectionString(connectionString);

            var httpClient = InitializeHttpClient();

            Authenticate();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            using var httpClient = CreateAuthorizedHttpClient();

            //Получаем ИТРоли
            var response = httpClient.GetAsync("api/v1/roles/all").Result;
            var itRoleResponse = DeserializeResponse<RoleResponse>(response);
            var itRolePermissions = itRoleResponse!.data.Select(_ => new Permission($"{ItRolePrefix},{_.id}", _.name, _.corporatePhoneNumber));

            //Получаем права
            response = httpClient.GetAsync("api/v1/rights/all").Result;
            var RightResponse = DeserializeResponse<RoleResponse>(response);
            var RightPermissions = RightResponse!.data.Select(_ => new Permission($"{RequestRightPrefix},{_.id}", _.name, _.corporatePhoneNumber));

            return itRolePermissions.Concat(RightPermissions);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            using var httpClient = CreateAuthorizedHttpClient();

            //Получаем ИТРоли
            var response = httpClient.GetAsync($"api/v1/users/{userLogin}/roles").Result;
            var itRoleResponse = DeserializeResponse<UserRoleResponse>(response);
            var result1 = itRoleResponse!.data.Select(_ => $"{ItRolePrefix},{_.id}").ToList();

            //Получаем права
            response = httpClient.GetAsync($"api/v1/users/{userLogin}/rights").Result;
            var RightResponse = DeserializeResponse<UserRoleResponse>(response);
            var result2 = RightResponse!.data.Select(_ => $"{RequestRightPrefix},{_.id}").ToList();

            return result1.Concat(result2).ToList();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using var httpClient = CreateAuthorizedHttpClient();

            var user = GetUserByLogin(httpClient, userLogin);

            if (user != null && user.status == LockedStatus)
            {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
            }
            //Назначаем права.
            else if (user != null && user.status == UnlockedStatus)
            {
                foreach (var rightId in rightIds)
                {
                    var rightStr = rightId.Split(',');
                    switch (rightStr[0])
                    {
                        case ItRolePrefix:
                            httpClient.PutAsync($"api/v1/users/{userLogin}/add/role/{rightStr[1]}", null).Wait();
                            break;
                        case RequestRightPrefix:
                            httpClient.PutAsync($"api/v1/users/{userLogin}/add/right/{rightStr[1]}", null).Wait();
                            break;
                        default:
                            throw new Exception($"Тип доступа {rightStr[0]} не определен");
                    }
                }
            }
            else
            {
                Logger.Warn($"Пользователь с логином {userLogin} не найден, или его статус не соответсвует возможным");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using var httpClient = CreateAuthorizedHttpClient();

            var user = GetUserByLogin(httpClient, userLogin);

            if (user != null && user.status == LockedStatus)
            {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
            }
            //отзываем права.
            else if (user != null && user.status == UnlockedStatus)
            {
                foreach (var rightId in rightIds)
                {
                    var rightStr = rightId.Split(',');
                    switch (rightStr[0])
                    {
                        case ItRolePrefix:
                            httpClient.DeleteAsync($"api/v1/users/{userLogin}/drop/role/{rightStr[1]}").Wait();
                            break;
                        case RequestRightPrefix:
                            httpClient.DeleteAsync($"api/v1/users/{userLogin}/drop/right/{rightStr[1]}").Wait();
                            break;
                        default:
                            throw new Exception($"Тип доступа {rightStr[0]} не определен");
                    }
                }
            }
            else
            {
                Logger.Warn($"Пользователь с логином {userLogin} не найден, или его статус не соответсвует возможным");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var props = new List<Property>();
            foreach (var propertyInfo in new UserPropertyData().GetType().GetProperties())
            {
                if (propertyInfo.Name == "login") continue;

                props.Add(new Property(propertyInfo.Name, propertyInfo.Name));
            }
            return props;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            using var httpClient = CreateAuthorizedHttpClient();

            var response = httpClient.GetAsync($"api/v1/users/{userLogin}").Result;
            var userResponse = DeserializeResponse<UserPropertyResponse>(response);

            var user = userResponse!.data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");

            if (user.status == LockedStatus)
                throw new Exception($"Невозможно получить свойства, пользователь {userLogin} залочен");

            return user.GetType().GetProperties()
                .Select(_ => new UserProperty(_.Name, _.GetValue(user) as string));
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using var httpClient = CreateAuthorizedHttpClient();

            var response = httpClient.GetAsync($"api/v1/users/{userLogin}").Result;
            var userResponse = DeserializeResponse<UserPropertyResponse>(response);

            var user = userResponse!.data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");

            if (user.status == LockedStatus)
                throw new Exception($"Невозможно обновить свойства, пользователь {userLogin} залочен");

            foreach (var property in properties)
            {
                foreach (var userProp in user.GetType().GetProperties())
                {
                    if (property.Name == userProp.Name)
                    {
                        userProp.SetValue(user, property.Value);
                    }
                }
            }

            var content = CreateStringContent(user);
            httpClient.PutAsync("api/v1/users/edit", content).Wait();
        }

        public bool IsUserExists(string userLogin)
        {
            using var httpClient = CreateAuthorizedHttpClient();

            var response = httpClient.GetAsync($"api/v1/users/all").Result;
            var userResponse = DeserializeResponse<UserResponse>(response);
            var user = userResponse!.data.FirstOrDefault(_ => _.login == userLogin);

            if (user != null) return true;

            return false;
        }

        public void CreateUser(UserToCreate user)
        {
            using var httpClient = CreateAuthorizedHttpClient();

            if (IsUserExists(user.Login))
            {
                throw new Exception("Пользователь с таким логином существует");
            }

            var newUser = new CreateUSerDTO()
            {
                login = user.Login,
                password = user.HashPassword,

                lastName = user.Properties.FirstOrDefault(p => p.Name.Equals("lastName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                firstName = user.Properties.FirstOrDefault(p => p.Name.Equals("firstName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                middleName = user.Properties.FirstOrDefault(p => p.Name.Equals("middleName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,

                telephoneNumber = user.Properties.FirstOrDefault(p => p.Name.Equals("telephoneNumber", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                isLead = bool.TryParse(user.Properties.FirstOrDefault(p => p.Name.Equals("isLead", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty, out bool isLeadValue)
                    ? isLeadValue
                    : false,

                status = string.Empty
            };

            var content = CreateStringContent(newUser);
            httpClient.PostAsync("api/v1/users/create", content).Wait();
        }

        private StringContent CreateStringContent<T>(T value)
        {
            var content = new StringContent(JsonSerializer.Serialize(value), UnicodeEncoding.UTF8, ApplicationJsonMediaType);
            return content;
        }

        private void ParseConnectionString(string connectionString)
        {
            foreach (var item in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split('=', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "url": url = value; break;
                    case "login": login = value; break;
                    case "password": password = value; break;
                }
            }
        }

        private HttpClient InitializeHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            return httpClient;
        }

        private void Authenticate()
        {
            var httpClient = InitializeHttpClient();

            var body = new { login, password };

            var content = CreateStringContent(body);

            var response = httpClient.PostAsync("api/v1/login", content).Result;

            var tokenResponse = DeserializeResponse<TokenResponse>(response);

            token = tokenResponse?.data?.access_token ?? throw new InvalidOperationException("Аутентификация не удалась");
        }

        private HttpClient CreateAuthorizedHttpClient()
        {
            var client = InitializeHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerScheme, token);
            return client;
        }

        private UserResponseData? GetUserByLogin(HttpClient httpClient, string userLogin)
        {
            var response = httpClient.GetAsync($"api/v1/users/all").Result;
            var userResponse = DeserializeResponse<UserResponse>(response);
            var user = userResponse!.data.FirstOrDefault(_ => _.login == userLogin);
            return user;
        }

        private T DeserializeResponse<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<T>(content)
                ?? throw new InvalidOperationException("Не удалось десериализовать ответ");
        }
    }
}
