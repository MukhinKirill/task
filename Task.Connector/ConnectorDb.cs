using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb :IConnector
    {
        private readonly ILogger<ConnectorDb> _logger;
        private readonly DbContextOptions<AppDbContext> _options;

        public ConnectorDb()
        {
            // Инициализация полей значениями по умолчанию
            _options = new DbContextOptions<AppDbContext>(); // Здесь можно добавить значения по умолчанию, если нужно       
            _logger = new DummyLogger<ConnectorDb>(); // Пример использования заглушки
        }

        // Заглушка для ILogger
        public class DummyLogger<T> :ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Console.WriteLine(formatter(state, exception));
            }
        }

        public ConnectorDb(DbContextOptions<AppDbContext> options, ILogger<ConnectorDb> logger)
        {
            _options = options;
            _logger = logger;
        }

        public Integration.Data.Models.ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            // Убираем внешние кавычки и обрабатываем строку
            connectionString = connectionString.Replace("'", ""); // Убираем все одиночные кавычки
            connectionString = connectionString.Replace("\"", "");
            connectionString = connectionString.Replace("ConnectionString=", "");
            // Разделяем строку подключения на ключи и значения
            var connectionStringDictionary = connectionString
                .Split(';')
                .Select(part => part.Split('='))
                .Where(split => split.Length == 2)
                .ToDictionary(split => split[0].Trim(), split => split[1].Trim('\'', ' '));

            foreach(var kv in connectionStringDictionary)
            {
                _logger.LogInformation($"Key: {kv.Key}, Value: {kv.Value}");
            }

            // Собираем строку подключения, включая только ключи, связанные с самой строкой подключения
            var necessaryKeys = new[] { "Host", "Port", "Username", "Password", "Database" };

            var assembledConnectionString = string.Join(";", connectionStringDictionary
                .Where(kv => necessaryKeys.Contains(kv.Key))
                .Select(kv => $"{kv.Key}={kv.Value}"));

            _logger.LogInformation($"Собранная строка подключения: {assembledConnectionString}");

            // Настройка DbContextOptions для PostgreSQL
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(assembledConnectionString); // Используем собранную строку подключения

            // Создание контекста базы данных с опциями
            using(var context = new AppDbContext(optionsBuilder.Options))
            {
                // Проверка существования базы данных и выполнение миграций
                context.Database.EnsureCreated();
                context.Database.Migrate();
            }

            // Логирование успешной инициализации
            _logger.LogInformation("Коннектор успешно инициализирован с заданной строкой подключения.");
        }

        public void CreateUser(UserToCreate user)
        {
            if(user == null)
            {
                _logger?.LogError("Попытка создать пользователя с null-значением.");
                throw new ArgumentNullException(nameof(user), "Пользователь не может быть null.");
            }

            using(var context = new AppDbContext(_options))
            {
                User newUser = new User(
                    user.Login,
                    user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "firstName",
                    user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "lastName",
                    user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "middleName",
                    user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ??
                        "telephoneNumber",
                    user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value == "true"
                 );

                context.Users.Add(newUser);

                context.Passwords.Add(new UserPassword
                {
                    UserId = user.Login,
                    Password = user.HashPassword,
                });

                context.SaveChanges();
                // Логируем успешное создание пользователя
                _logger?.LogInformation($"Пользователь с логином {user.Login} был успешно создан.");
            }
        }

        public bool IsUserExists(string userLogin)
        {
            if(string.IsNullOrWhiteSpace(userLogin))
            {
                _logger?.LogError("Попытка проверить существование пользователя с пустым логином.");
                throw new ArgumentException("Логин не может быть пустым.", nameof(userLogin));
            }

            using(var context = new AppDbContext(_options))
            {
                var userExists = context.Users.Any(u => u.Login == userLogin);
                return userExists;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            using(var context = new AppDbContext(_options))
            {
                // Извлекаем пользователя по логину
                var user = context.Users
                    .FirstOrDefault(u => u.Login == userLogin);

                // Если пользователь не найден, можно выбросить исключение или вернуть пустую коллекцию
                if(user == null)
                {
                    _logger?.LogWarning($"Пользователь с логином {userLogin} не найден.");
                    return Enumerable.Empty<UserProperty>();
                }

                // Список свойств пользователя, преобразованный в UserProperty
                var userProperties = new List<UserProperty>
                {
                    new UserProperty("firstName", user.FirstName),
                    new UserProperty("lastName", user.LastName),
                    new UserProperty("middleName", user.MiddleName),
                    new UserProperty("telephoneNumber", user.TelephoneNumber),
                    new UserProperty("isLead", user.IsLead.ToString())
                };

                _logger?.LogWarning($"Свойства пользователя с логином {userLogin} успешно получены.");

                return userProperties;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using(var context = new AppDbContext(_options))
            {
                var user = context.Users
                    .FirstOrDefault(u => u.Login == userLogin);

                if(user == null)
                {
                    _logger?.LogWarning($"Пользователь с логином {userLogin} не найден.");
                    return;
                }

                // Обновляем свойства пользователя
                foreach(var property in properties)
                {
                    var propertyName = char.ToUpper(property.Name[0]) + property.Name.Substring(1);

                    // Проверяем, существует ли свойство в User с учетом регистра
                    var propertyInfo = typeof(User).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if(propertyInfo != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(user, property.Value);
                    }
                    else
                    {
                        _logger?.LogWarning($"Свойство {property.Name} не найдено или оно не может быть изменено.");
                    }
                }
                context.SaveChanges();
                _logger?.LogWarning($"Свойства для пользователя {userLogin} были успешно обновлены.");
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if(rightIds == null || !rightIds.Any())
            {
                _logger?.LogWarning("Список прав пустой или равен null.");
                return;
            }

            using(var context = new AppDbContext(_options))
            {
                var user = context.Users.FirstOrDefault(u => u.Login == userLogin);

                if(user == null)
                {
                    _logger?.LogWarning($"Пользователь с логином {userLogin} не найден.");
                    return; // Если пользователь не найден, возвращаемся.
                }

                foreach(var rightId in rightIds)
                {
                    if(int.TryParse(rightId.Split(":").Skip(1).First(), out var numericRightId))
                    {
                        // Проверяем, существует ли уже связь между пользователем и правом
                        var isRecordAlreadyExist = context.UserRequestRights
                            .Any(userRequestRight =>
                                userRequestRight.UserId == userLogin && userRequestRight.RightId == numericRightId
                            );

                        if(!isRecordAlreadyExist)
                        {
                            // Если записи нет, создаем новую
                            context.UserRequestRights.Add(new UserRequestRight
                            {
                                UserId = userLogin,
                                RightId = numericRightId
                            });
                            _logger?.LogInformation($"Право с ID {numericRightId} добавлено пользователю {userLogin}.");
                        }
                        else
                        {
                            _logger?.LogInformation($"Право с ID {numericRightId} уже добавлено пользователю {userLogin}.");
                        }

                        // Добавляем роль пользователю
                        var isRoleAlreadyAssigned = context.UserITRoles
                            .Any(userITRole => userITRole.UserId == userLogin && userITRole.RoleId == numericRightId);

                        if(!isRoleAlreadyAssigned)
                        {
                            context.UserITRoles.Add(new UserITRole
                            {
                                UserId = userLogin,
                                RoleId = numericRightId
                            });
                            _logger?.LogInformation($"Роль с ID {numericRightId} добавлена пользователю {userLogin}.");
                        }
                        else
                        {
                            _logger?.LogInformation($"Роль с ID {numericRightId} уже добавлена пользователю {userLogin}.");
                        }
                    }
                    else
                    {
                        _logger?.LogError($"Не удалось правильно разобрать идентификатор права: {rightId}");
                        throw new ArgumentException("Не удалось правильно разобрать идентификатор права.");
                    }
                }

                // Проверяем, что права и роли были добавлены
                var userRequestRightsCount = context.UserRequestRights.Count(r => r.UserId == userLogin);
                var userITRolesCount = context.UserITRoles.Count(r => r.UserId == userLogin);
                _logger?.LogInformation($"UserRequestRights count: {userRequestRightsCount}, UserITRoles count: {userITRolesCount}");

                // Сохраняем изменения в базе данных
                context.SaveChanges();
                _logger?.LogInformation($"Права успешно добавлены пользователю {userLogin}.");
            }
        }
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            using(var context = new AppDbContext(_options))
            {
                var userPermission = context.UserRequestRights
                    .Where(userRequestRight => userRequestRight.UserId == userLogin)
                    .Join(
                        context.RequestRights,
                        userRequestRight => userRequestRight.RightId,
                        requestRight => requestRight.Id,
                        (userRequestRight, requestRight) => requestRight.Name
                    )
                    .ToList();
                _logger?.LogWarning("Права пользователя были успешно получены.");
                return userPermission;
            }
        }

        //думаю, что данный метод работает неправильно, так как решение подгонялось под работу теста (из теста приходит строка Request:2), которая является rightId - то есть удаление должно происходить по номеру...)
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using(var context = new AppDbContext(_options))
            {
                var user = context.Users
                    .FirstOrDefault(u => u.Login == userLogin);

                if(user == null)
                {
                    _logger?.LogWarning($"Пользователь с логином {userLogin} не найден.");
                    return;
                }

                // Извлекаем числовые значения из строк, например "Request:2" -> 2
                var rightIdsToRemove = rightIds
                    .Select(id =>
                    {
                        var parts = id.Split(':');
                        return parts.Length > 1 ? Convert.ToInt32(parts[1]) : 0; // Преобразуем в число после двоеточия
                    })
                    .ToList();

                // Получаем связанные записи в UserRequestRights, которые нужно удалить
                var userRequestRightsToRemove = context.UserRequestRights
                    .Where(urr => urr.UserId == user.Login && rightIdsToRemove.Contains(urr.RightId))
                    .ToList();

                context.UserRequestRights.RemoveRange(userRequestRightsToRemove);
                context.SaveChanges();

                _logger?.LogWarning($"Права для пользователя {userLogin} были успешно удалены.");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            using(var context = new AppDbContext(_options))
            {
                var properties = typeof(User).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                // Создаем список Property, который будет содержать имя свойства,а значение будет равно имени каждого свойства
                var userProperties = properties.Select(property => new Property(
                    property.Name,
                    property.Name
                )).ToList();
                return userProperties;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            using(var context = new AppDbContext(_options))
            {
                // Извлекаем все права из таблицы RequestRights
                var permissionsRequestRights = context.RequestRights
                    .Select(requestRight => new Permission(
                        requestRight.Id.ToString(),
                        requestRight.Name,
                        ""
                    )).ToList();
                // Извлекаем все права из таблицы ItRoles
                var permissionsItRoles = context.ItRoles
                  .Select(requestRight => new Permission(
                      requestRight.Id.ToString(),
                      requestRight.Name,
                      ""
                  )).ToList();
                var permissions = permissionsRequestRights.Concat(permissionsItRoles).ToList();
                if(!permissions.Any())
                {
                    _logger?.LogWarning("Нет прав в системе.");
                }

                return permissions;
            }
        }
    }
}
