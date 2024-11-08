using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _context;
        public ILogger Logger { get; set; }

        private IMemoryCache _cache;
        private TimeSpan? _cacheExpiration;

        /// <summary>
        /// Инициализация с использованием переданной конфигурации, поддерживается PostgreSQL и SqlServer, а также кэширование.
        /// </summary>
        /// <param name="configuration">Конфигурационная строка типа "Key:'Value';" с поддержкой вложенности, обязательно должна содержать ConnectionString и Provider, опционально CacheExpiration.</param>
        /// <remarks>
        /// Конфигурационная строка поддерживает следующие ключи:
        /// - ConnectionString: строка подключения к базе данных.
        /// - Provider: провайдер базы данных, поддерживаются "SqlServer" и "PostgreSQL".
        /// - CacheExpiration (необязательно): время истечения кэша в формате TimeSpan. Например, "00:30:00" для 30 минут.
        /// Если параметр CacheExpiration не указан, кэширование не будет использоваться.
        /// </remarks>
        public void StartUp(string configuration)
        {
            var configParams = ParseConfiguration(configuration);

            if (!configParams.TryGetValue("ConnectionString", out var connectionString))
            {
                Logger?.Error("Отсутствует параметр ConnectionString в конфигурации.");
                throw new ArgumentException("Отсутствует параметр ConnectionString в конфигурации.");
            }
            if (!configParams.TryGetValue("Provider", out var provider))
            {
                Logger?.Error("Отсутствует параметр Provider в конфигурации.");
                throw new ArgumentException("Отсутствует параметр Provider в конфигурации.");
            }
            if (configParams.TryGetValue("CacheExpiration", out var cacheExpirationStr) && TimeSpan.TryParse(cacheExpirationStr, out var cacheExpiration))
            {
                _cacheExpiration = cacheExpiration;
                Logger?.Debug("Используется кэширование, срок истечения кэша: " + cacheExpiration.ToString());
            }
            else
            {
                _cacheExpiration = null;
            }
            _cache = new MemoryCache(new MemoryCacheOptions());

            _context = GetContext(connectionString, provider);
            Logger?.Debug("Контекст базы данных создан для провайдера: " + provider);
        }

        private Dictionary<string, string> ParseConfiguration(string configuration)
        {
            var configParams = new Dictionary<string, string>();

            var regex = new Regex(@"(\w+)\s*=\s*'([^']*)'|(\w+)\s*=\s*([^;]*)");

            foreach (Match match in regex.Matches(configuration))
            {
                var key = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[3].Value;
                var value = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[4].Value;

                configParams[key.Trim()] = value.Trim();
            }

            return configParams;
        }

        private DataContext GetContext(string connectionString, string provider)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            if (provider.StartsWith("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
            else if (provider.StartsWith("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
            else
            {
                Logger.Error($"Неподдерживаемый провайдер базы данных: {provider}");
                throw new NotSupportedException($"Неподдерживаемый провайдер базы данных: {provider}");
            }

            return new DataContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Создание нового пользователя в базе данных.
        /// </summary>
        /// <param name="user">Информация о пользователе для создания.</param>
        public void CreateUser(UserToCreate user)
        {
            if (IsUserExists(user.Login))
            {
                Logger.Error("Пользователь с таким логином уже существует: " + user.Login);
                throw new SqlAlreadyFilledException("Пользователь с таким логином уже существует.");
            }

            var newUser = new User
            {
                Login = user.Login,
                FirstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? string.Empty,
                LastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? string.Empty,
                MiddleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? string.Empty,
                TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ?? string.Empty,
                IsLead = user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value == "true"
            };

            _context.Users.Add(newUser);
            _context.Passwords.Add(new Sequrity
            {
                UserId = user.Login,
                Password = user.HashPassword
            });

            _context.SaveChanges();
            Logger.Debug($"Пользователь с логином {user.Login} создан.");
        }

        /// <summary>
        /// Проверка, существует ли пользователь с заданным логином.
        /// </summary>
        /// <param name="userLogin">Логин пользователя для проверки.</param>
        /// <returns>True, если пользователь существует; иначе - fakse.</returns>
        public bool IsUserExists(string userLogin)
        {
            if (string.IsNullOrWhiteSpace(userLogin))
            {
                Logger.Error("Логин пользователя не может быть пустым.");
                throw new ArgumentException("Логин пользователя не может быть пустым.", nameof(userLogin));
            }

            return _context.Users.Any(u => u.Login == userLogin);
        }

        /// <summary>
        /// Получение всех свойств пользователя (включая хэш пароля), использует кэширование.
        /// </summary>
        /// <returns>Список свойств пользователя.</returns>
        public IEnumerable<Property> GetAllProperties()
        {
            if (_cache.TryGetValue("AllProperties", out IEnumerable<Property> cachedProperties))
            {
                return cachedProperties;
            }

            var properties = GetModelProperties<User>(includePrimaryKeys: false)
                .Select(p => new Property(p.Name, $"Свойство пользователя (из EF): {p.Name}"))
                .ToList();

            properties.Add(new Property("Password", "Хэш пароля пользователя, захардкожено"));
            Logger.Debug($"Получено {properties.Count} свойств пользователя.");

            if (_cacheExpiration.HasValue)
            {
                _cache.Set("AllProperties", properties, _cacheExpiration.Value);
            }

            return properties;
        }

        /// <summary>
        /// Получение значений свойств конкретного пользователя, использует кэширование.
        /// </summary>
        /// <param name="userLogin">Логин пользователя, чьи свойства необходимо получить.</param>
        /// <returns>Список значений свойств пользователя.</returns>
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (_cache.TryGetValue($"UserProperties_{userLogin}", out IEnumerable<UserProperty> cachedProperties))
            {
                return cachedProperties;
            }

            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger.Error("Пользователь с логином не найден: " + userLogin);
                throw new InvalidOperationException("Пользователь не найден.");
            }

            var properties = GetModelProperties<User>(includePrimaryKeys: false)
                .Select(p => new UserProperty(p.Name, p.GetValue(user)?.ToString() ?? string.Empty))
                .ToList();

            Logger.Debug($"Получено {properties.Count} свойств для пользователя с логином {userLogin}.");

            if (_cacheExpiration.HasValue)
            {
                _cache.Set($"UserProperties_{userLogin}", properties, _cacheExpiration.Value);
            }

            return properties;
        }

        /// <summary>
        /// Обновление свойств пользователя.
        /// </summary>
        /// <param name="properties">Список свойств для обновления.</param>
        /// <param name="userLogin">Логин пользователя, чьи свойства необходимо обновить.</param>
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger.Error("Пользователь с логином не найден: " + userLogin);
                throw new InvalidOperationException("Пользователь не найден.");
            }

            foreach (var prop in properties)
            {
                var propertyInfo = typeof(User).GetProperty(prop.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(prop.Value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(user, convertedValue);
                }
            }

            _context.SaveChanges();
            Logger.Debug($"{properties.Count()} cвойств пользователя с логином {userLogin} обновлены.");
        }

        /// <summary>
        /// Получение всех доступных прав, использует кэширование.
        /// </summary>
        /// <returns>Список всех прав.</returns>
        public IEnumerable<Permission> GetAllPermissions()
        {
            if (_cache.TryGetValue("AllPermissions", out IEnumerable<Permission> cachedPermissions))
            {
                return cachedPermissions;
            }

            var permissions = _context.RequestRights
                .Select(right => new Permission(right.Id.ToString(), right.Name, "Право изменения заявок"))
                .ToList();

            permissions.AddRange(_context.ITRoles
                .Select(role => new Permission(role.Id.ToString(), role.Name, "Право роли исполнителя")));

            Logger.Debug($"Получено {permissions.Count} прав.");

            if (_cacheExpiration.HasValue)
            {
                _cache.Set("AllPermissions", permissions, _cacheExpiration.Value);
            }

            return permissions;
        }

        /// <summary>
        /// Добавление прав пользователю, при одновременном добавлении более 20 прав проверки производятся локально.
        /// </summary>
        /// <param name="userLogin">Логин пользователя, которому добавляются права.</param>
        /// <param name="rightIds">Список идентификаторов прав для добавления.</param>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var userRequestRights = new List<UserRequestRight>();
            var userITRoles = new List<UserITRole>();

            bool useCache = rightIds.Count() > 20;

            HashSet<int> existingRequestIds = null;
            HashSet<int> existingRoleIds = null;

            if (useCache)
            {
                existingRequestIds = _context.RequestRights.Select(r => (int)r.Id).ToHashSet();
                existingRoleIds = _context.ITRoles.Select(r => (int)r.Id).ToHashSet();
            }

            foreach (var rightId in rightIds)
            {
                var parsedRightId = ParseRightId(rightId);
                if (parsedRightId == null) continue;

                var (prefix, id) = parsedRightId.Value;

                if (prefix.Equals("Request", StringComparison.OrdinalIgnoreCase))
                {
                    if ((useCache && existingRequestIds.Contains(id)) || (!useCache && _context.RequestRights.Any(r => r.Id == id)))
                    {
                        userRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = id });
                    }
                }
                else if (prefix.Equals("Role", StringComparison.OrdinalIgnoreCase))
                {
                    if ((useCache && existingRoleIds.Contains(id)) || (!useCache && _context.ITRoles.Any(r => r.Id == id)))
                    {
                        userITRoles.Add(new UserITRole { UserId = userLogin, RoleId = id });
                    }
                }
            }

            if (userRequestRights.Any()) _context.UserRequestRights.AddRange(userRequestRights);
            if (userITRoles.Any()) _context.UserITRoles.AddRange(userITRoles);

            _context.SaveChanges();
            Logger.Debug($"Пользователю с логином {userLogin} добавлено {rightIds.Count()} прав.");
        }

        /// <summary>
        /// Удаление прав пользователя.
        /// </summary>
        /// <param name="userLogin">Логин пользователя, у которого удаляются права.</param>
        /// <param name="rightIds">Список идентификаторов прав для удаления.</param>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var requestRightsToRemove = new List<UserRequestRight>();
            var itRolesToRemove = new List<UserITRole>();

            foreach (var rightId in rightIds)
            {
                var parsedRightId = ParseRightId(rightId);
                if (parsedRightId == null) continue;

                var (prefix, id) = parsedRightId.Value;

                if (prefix.Equals("Request", StringComparison.OrdinalIgnoreCase))
                {
                    var requestRight = _context.UserRequestRights.FirstOrDefault(r => r.UserId == userLogin && r.RightId == id);
                    if (requestRight != null) requestRightsToRemove.Add(requestRight);
                }
                else if (prefix.Equals("Role", StringComparison.OrdinalIgnoreCase))
                {
                    var itRole = _context.UserITRoles.FirstOrDefault(r => r.UserId == userLogin && r.RoleId == id);
                    if (itRole != null) itRolesToRemove.Add(itRole);
                }
            }

            if (requestRightsToRemove.Any()) _context.UserRequestRights.RemoveRange(requestRightsToRemove);
            if (itRolesToRemove.Any()) _context.UserITRoles.RemoveRange(itRolesToRemove);

            _context.SaveChanges();
            Logger.Debug($"Удалено {rightIds.Count()} прав пользователя с логином {userLogin}.");
        }

        /// <summary>
        /// Получение прав пользователя.
        /// </summary>
        /// <param name="userLogin">Логин пользователя, чьи права необходимо получить.</param>
        /// <returns>Список прав пользователя.</returns>
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var permissions = _context.UserRequestRights
                .Where(r => r.UserId == userLogin)
                .Select(r => $"Request:{r.RightId}")
                .ToList();

            permissions.AddRange(_context.UserITRoles
                .Where(r => r.UserId == userLogin)
                .Select(r => $"Role:{r.RoleId}"));

            Logger.Debug($"Получено {permissions.Count} прав для пользователя с логином {userLogin}.");
            return permissions;
        }

        /// <summary>
        /// Разбор идентификатора права доступа.
        /// </summary>
        /// <param name="rightId">Идентификатор права доступа в формате "Префикс:Id".</param>
        /// <returns>Кортеж с префиксом и идентификатором права, либо null, если формат некорректен.</returns>
        private (string Prefix, int Id)? ParseRightId(string rightId)
        {
            var parts = rightId.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int id))
            {
                return (parts[0].Trim(), id);
            }
            Logger.Error($"Некорректный формат идентификатора права: {rightId}");
            throw new ArgumentException($"Некорректный формат идентификатора права: {rightId}");
        }

        /// <summary>
        /// Получение всех свойств модели из метаданных EF контекста.
        /// </summary>
        /// <typeparam name="T">Тип сущности для получения свойств.</typeparam>
        /// <param name="includePrimaryKeys">Включать ли свойства, являющиеся ключами.</param>
        /// <returns>Перечисление свойств модели.</returns>
        private IEnumerable<PropertyInfo> GetModelProperties<T>(bool includePrimaryKeys = true) where T : class
        {
            var entityType = _context.Model.FindEntityType(typeof(T));

            if (entityType == null)
            {
                Logger.Error($"Тип сущности {typeof(T).Name} не найден в модели.");
                throw new InvalidOperationException($"Тип сущности {typeof(T).Name} не найден в модели.");
            }

            var properties = entityType
                .GetProperties()
                .Where(p => includePrimaryKeys || !p.IsPrimaryKey())
                .Select(p => typeof(T).GetProperty(p.Name))
                .Where(p => p != null)
                .Cast<PropertyInfo>();

            Logger.Debug($"Получено {properties.Count()} свойств для типа {typeof(T).Name}.");
            return properties;
        }
    }
}
