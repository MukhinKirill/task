using TR.Connector.Helpers;
using TR.Connector.Responses;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class Connector : IConnector
    {
        public ILogger Logger { get; set; } = null!;
        private HttpClientHelper _httpClientHelper = null!;

        public void StartUp(string connectionString)
        {
            //Парсим строку подключения.
            Logger.Debug("Строка подключения: " + connectionString);
            var parseString = ParseConnectionString(connectionString);

            if (string.IsNullOrWhiteSpace(parseString.Url))
            {
                throw new InvalidOperationException("В строке подключения не задан 'url'");
            }

            if (string.IsNullOrWhiteSpace(parseString.Login) || string.IsNullOrWhiteSpace(parseString.Password))
            {
                throw new InvalidOperationException("В строке подключения не заданы 'login' или 'password'");
            }

            _httpClientHelper = new HttpClientHelper(parseString.Url, parseString.Login, parseString.Password);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            //Получаем ИТРоли
            var rolesResponse = _httpClientHelper.Get("api/v1/roles/all");
            var itRoleResponse = JsonHelper.Deserialize<RoleResponse>(rolesResponse);
            var itRolePermissions = itRoleResponse?.Datas.Select(r=> 
                new Permission($"ItRole,{r.Id}", r.Name, r.CorporatePhoneNumber)) ?? [];

            //Получаем права
            var rightResponse = _httpClientHelper.Get("api/v1/rights/all");
            var requestRightResponse = JsonHelper.Deserialize<RoleResponse>(rightResponse);
            var rightPermissions = requestRightResponse?.Datas.Select(r => 
                new Permission($"RequestRight,{r.Id}", r.Name, r.CorporatePhoneNumber)) ?? [];

            return itRolePermissions.Concat(rightPermissions);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            //Получаем ИТРоли
            var roleResponse = _httpClientHelper.Get($"api/v1/users/{userLogin}/roles");
            var itRoleResponse = JsonHelper.Deserialize<UserRoleResponse>(roleResponse);
            var itRole = itRoleResponse?.Datas.Select(r => $"ItRole,{r.Id}") ?? [];

            //Получаем права
            var rightResponse = _httpClientHelper.Get($"api/v1/users/{userLogin}/rights");
            var userRoleResponse = JsonHelper.Deserialize<UserRoleResponse>(rightResponse);
            var requestRight = userRoleResponse?.Datas
                .Select(ur => $"RequestRight,{ur.Id}") ?? [];

            return itRole.Concat(requestRight).ToList();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        { 
            var user = GetUserByLogin(userLogin);

            if (user == null)
            {
                Logger.Error($"Пользователь {userLogin} не найден");
                return;
            }

            switch (user) 
            { 
                case { Status: "Lock" }: 
                    Logger.Error($"Пользователь {userLogin} залочен.");
                    break;

                //Назначаем права. 
                case { Status: "Unlock" }:
                { 
                    foreach (var rightId in rightIds)
                    {
                        var rightStr = rightId.Split(',');
                        switch (rightStr[0])
                        {
                            case "ItRole":
                                _httpClientHelper.Put($"api/v1/users/{userLogin}/add/role/{rightStr[1]}", null);
                                break;
                            case "RequestRight":
                                _httpClientHelper.Put($"api/v1/users/{userLogin}/add/right/{rightStr[1]}", null);
                                break;
                            default: 
                                throw new Exception($"Тип доступа {rightStr[0]} не определен");
                        }
                    }

                    break;
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = GetUserByLogin(userLogin);

            if (user == null)
            {
                Logger.Error($"Пользователь {userLogin} не найден");
                return;
            }

            switch (user.Status)
            {
                case "Lock":
                    Logger.Error($"Пользователь {userLogin} залочен.");
                    return;

                //отзываем права.
                case "Unlock":
                {
                    foreach (var rightId in rightIds)
                    {
                        var rightStr = rightId.Split(',');
                        switch (rightStr[0])
                        {
                            case "ItRole":
                                _httpClientHelper.Delete($"api/v1/users/{userLogin}/drop/role/{rightStr[1]}");
                                break;
                            case "RequestRight":
                                _httpClientHelper.Delete($"api/v1/users/{userLogin}/drop/right/{rightStr[1]}");
                                break;
                            default:
                                throw new Exception($"Тип доступа {rightStr[0]} не определен");
                        }
                    }

                    break;
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
            var response = _httpClientHelper.Get($"api/v1/users/{userLogin}");
            var userResponse = JsonHelper.Deserialize<UserPropertyResponse>(response);

            var user = userResponse?.Data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");

            if (user.Status == "Lock")
            {
                throw new Exception($"Невозможно получить свойства, пользователь {userLogin} залочен");
            }

            return user.GetType().GetProperties()
                .Select(u => new UserProperty(u.Name, u.GetValue(user)?.ToString() ?? string.Empty));
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var response = _httpClientHelper.Get($"api/v1/users/{userLogin}");
            var userResponse = JsonHelper.Deserialize<UserPropertyResponse>(response);

            var user = userResponse?.Data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");
            if (user.Status == "Lock")
            {
                throw new Exception($"Невозможно обновить свойства, пользователь {userLogin} залочен");
            }

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

            var content = JsonHelper.CreateJsonContent(user);
            _httpClientHelper.Put("api/v1/users/edit", content);
        }

        public bool IsUserExists(string userLogin)
        { 
            var response = _httpClientHelper.Get("api/v1/users/all");
            var userResponse = JsonHelper.Deserialize<UserResponse>(response);
            var user = userResponse?.Datas.FirstOrDefault(u => u.Login == userLogin);

            return user != null;
        }

        public void CreateUser(UserToCreate user)
        {
            var newUser = new CreateUserDTO
            {
                Login = user.Login,
                Password = user.HashPassword,

                LastName = user.Properties.FirstOrDefault(p => p.Name.Equals("lastName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                FirstName = user.Properties.FirstOrDefault(p => p.Name.Equals("firstName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                MiddleName = user.Properties.FirstOrDefault(p => p.Name.Equals("middleName", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,

                TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name.Equals("telephoneNumber", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                IsLead = bool.TryParse(user.Properties.FirstOrDefault(p => p.Name.Equals("isLead", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty, out bool isLeadValue) && isLeadValue,

                Status = string.Empty
            };

            var content = JsonHelper.CreateJsonContent(newUser);
            _httpClientHelper.Post("api/v1/users/create", content);
        }

        private (string Url, string Login, string Password) ParseConnectionString(string connectionString)
        {
            var parts = connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2, StringSplitOptions.TrimEntries))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0], p => p[1], StringComparer.OrdinalIgnoreCase);

            parts.TryGetValue("url", out var url);
            parts.TryGetValue("login", out var login);
            parts.TryGetValue("password", out var password);
            
            return (url ?? string.Empty, login ?? string.Empty, password ?? string.Empty);
        }

        private UserPropertyData? GetUserByLogin(string userLogin)
        {
            var response = _httpClientHelper.Get($"api/v1/users/{userLogin}");
            var userResponse = JsonHelper.Deserialize<UserPropertyResponse>(response);
            return userResponse?.Data;
        }
    }
}
