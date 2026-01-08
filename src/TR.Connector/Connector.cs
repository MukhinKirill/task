using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TR.Connector.Models;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class Connector : IConnector
    {
        public ILogger Logger { get; set; }

        private string url = "";
        private string login = "";
        private string password = "";

        private string token = "";

        //Пустой конструктор
        public Connector() {}

        public void StartUp(string connectionString)
        {
            //Парсим строку подключения.
            Logger.Debug("Строка подключения: " + connectionString);
            foreach (var item in connectionString.Split(';'))
            {
                if (item.StartsWith("url")) url = item.Split('=')[1];
                if (item.StartsWith("login")) login = item.Split('=')[1];
                if (item.StartsWith("password")) password = item.Split('=')[1];
            }

            //Проходим аунтификацию на сервере.
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            var body = new { login, password };
            var content = new StringContent(JsonSerializer.Serialize(body), UnicodeEncoding.UTF8, "application/json");
            var response = httpClient.PostAsync("api/v1/login", content).Result;
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content.ReadAsStringAsync().Result);
            token = tokenResponse!.Data!.AccessToken;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Получаем ИТРоли
            var response = httpClient.GetAsync("api/v1/roles/all").Result;
            var itRoleResponse = JsonSerializer.Deserialize<RoleResponse>(response.Content.ReadAsStringAsync().Result);
            var itRolePermissions =
                itRoleResponse!.Data!.Select(_ => new Permission($"ItRole,{_.Id}", _.Name, _.CorporatePhoneNumber));

            //Получаем права
            response = httpClient.GetAsync("api/v1/rights/all").Result;
            var rightResponse = JsonSerializer.Deserialize<RightResponse>(response.Content.ReadAsStringAsync().Result);
            var rightPermissions = rightResponse!.Data!.Select(_ =>
                new Permission($"RequestRight,{_.Id}", _.Name, null));

            return itRolePermissions.Concat(rightPermissions);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Получаем ИТРоли
            var response = httpClient.GetAsync($"api/v1/users/{userLogin}/roles").Result;
            var itRoleResponse = JsonSerializer.Deserialize<UserRoleResponse>(response.Content.ReadAsStringAsync().Result);
            var result1 = itRoleResponse!.Data!.Select(_ => $"ItRole,{_.Id}").ToList();

            //Получаем права
            response = httpClient.GetAsync($"api/v1/users/{userLogin}/rights").Result;
            var rightResponse = JsonSerializer.Deserialize<UserRightResponse>(response.Content.ReadAsStringAsync().Result);
            var result2 = rightResponse!.Data!.Select(_ => $"RequestRight,{_.Id}").ToList();

            return result1.Concat(result2).ToList();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

             //проверяем что пользователь не залочен.
             var response = httpClient.GetAsync($"api/v1/users/all").Result;
             var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
             var user = userResponse!.Data!.FirstOrDefault(_ => _.Login == userLogin);

             if (user != null && user.Status == "Lock")
             {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
             }
             //Назначаем права.
             else if (user != null && user.Status == "Unlock")
             {
                 foreach (var rightId in rightIds)
                 {
                     var rightStr = rightId.Split(',');
                     switch (rightStr[0])
                     {
                        case "ItRole":
                            httpClient.PutAsync($"api/v1/users/{userLogin}/add/role/{rightStr[1]}", null).Wait();
                            break;
                        case "RequestRight":
                            httpClient.PutAsync($"api/v1/users/{userLogin}/add/right/{rightStr[1]}", null).Wait();
                            break;
                        default: 
                            throw new Exception($"Тип доступа {rightStr[0]} не определен");
                     }
                 }
             }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //проверяем что пользователь не залочен.
            var response = httpClient.GetAsync($"api/v1/users/all").Result;
            var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
            var user = userResponse!.Data!.FirstOrDefault(_ => _.Login == userLogin);

            if (user != null && user.Status == "Lock")
            {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
            }
             //отзываем права.
            else if (user != null && user.Status == "Unlock")
            {
                foreach (var rightId in rightIds)
                {
                    var rightStr = rightId.Split(',');
                    switch (rightStr[0])
                    {
                        case "ItRole":
                            httpClient.DeleteAsync($"api/v1/users/{userLogin}/drop/role/{rightStr[1]}").Wait();
                            break;
                        case "RequestRight":
                            httpClient.DeleteAsync($"api/v1/users/{userLogin}/drop/right/{rightStr[1]}").Wait();
                            break;
                        default:
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
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = httpClient.GetAsync($"api/v1/users/{userLogin}").Result;
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(response.Content.ReadAsStringAsync().Result);

            var user = userResponse!.Data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");

            if (user.Status == "Lock")
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
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = httpClient.GetAsync($"api/v1/users/{userLogin}").Result;
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(response.Content.ReadAsStringAsync().Result);

            var user = userResponse!.Data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");
            if (user.Status == "Lock")
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
            httpClient.PutAsync("api/v1/users/edit", content).Wait();
        }

        public bool IsUserExists(string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = httpClient.GetAsync($"api/v1/users/all").Result;
            var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
            var user = userResponse!.Data!.FirstOrDefault(_ => _.Login == userLogin);

            if(user != null) return true;

            return false;
        }

        public void CreateUser(UserToCreate user)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            httpClient.PostAsync("api/v1/users/create", content).Wait();
        }
    }
}
