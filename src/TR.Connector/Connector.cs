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
            token = tokenResponse.data.access_token;
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
                itRoleResponse.data.Select(_ => new Permission($"ItRole,{_.id}", _.name, _.corporatePhoneNumber));

            //Получаем права
            response = httpClient.GetAsync("api/v1/rights/all").Result;
            var RightResponse = JsonSerializer.Deserialize<RoleResponse>(response.Content.ReadAsStringAsync().Result);
            var RightPermissions = RightResponse.data.Select(_ =>
                new Permission($"RequestRight,{_.id}", _.name, _.corporatePhoneNumber));

            return itRolePermissions.Concat(RightPermissions);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Получаем ИТРоли
            var response = httpClient.GetAsync($"api/v1/users/{userLogin}/roles").Result;
            var itRoleResponse = JsonSerializer.Deserialize<UserRoleResponse>(response.Content.ReadAsStringAsync().Result);
            var result1 = itRoleResponse.data.Select(_ => $"ItRole,{_.id}").ToList();

            //Получаем права
            response = httpClient.GetAsync($"api/v1/users/{userLogin}/rights").Result;
            var RightResponse = JsonSerializer.Deserialize<UserRoleResponse>(response.Content.ReadAsStringAsync().Result);
            var result2 = RightResponse.data.Select(_ => $"RequestRight,{_.id}").ToList();

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
             var user = userResponse.data.FirstOrDefault(_ => _.login == userLogin);

             if (user != null && user.status == "Lock")
             {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
             }
             //Назначаем права.
             else if (user != null && user.status == "Unlock")
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
            var user = userResponse.data.FirstOrDefault(_ => _.login == userLogin);

            if (user != null && user.status == "Lock")
            {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
            }
             //отзываем права.
            else if (user != null && user.status == "Unlock")
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
            foreach (var propertyInfo in new UserPropertyData().GetType().GetProperties())
            {
                if(propertyInfo.Name == "login") continue;

                props.Add(new Property(propertyInfo.Name, propertyInfo.Name));
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

            var user = userResponse.data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");

            if (user.status == "Lock")
                throw new Exception($"Невозможно получить свойства, пользователь {userLogin} залочен");

            return user.GetType().GetProperties()
                .Select(_ => new UserProperty(_.Name, _.GetValue(user) as string));
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = httpClient.GetAsync($"api/v1/users/{userLogin}").Result;
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(response.Content.ReadAsStringAsync().Result);

            var user = userResponse.data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");
            if (user.status == "Lock")
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
            var user = userResponse.data.FirstOrDefault(_ => _.login == userLogin);

            if(user != null) return true;

            return false;
        }

        public void CreateUser(UserToCreate user)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

            var content = new StringContent(JsonSerializer.Serialize(newUser), UnicodeEncoding.UTF8, "application/json");
            httpClient.PostAsync("api/v1/users/create", content).Wait();
        }
    }
}
