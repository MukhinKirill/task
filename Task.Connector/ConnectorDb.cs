using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private const string requestRightGroupName = "Request";
        private const string itRoleRightGroupName = "Role";
        static char delemiter = ':';
        private DataContext _context;
        public ILogger Logger { get; set; }
        // Пустой конструктор, как требуется
        public ConnectorDb() { }
        public void StartUp(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                Logger?.Error("Строка подключения не может быть пустой.");
                throw new ArgumentException("Строка подключения не может быть пустой.", nameof(connectionString));
            }

            Logger?.Debug("Начало инициализации базы данных с строкой подключения: " + connectionString);

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
                optionsBuilder.UseNpgsql(connectionString);
                Logger?.Debug("Используется провайдер PostgreSQL.");
                _context = new DataContext(optionsBuilder.Options);
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при инициализации базы данных: {ex.Message}");
                throw;
            }
        }

        public void CreateUser(UserToCreate user)
        {
            if (user == null)
            {
                Logger?.Error("CreateUser вызывается с null.");
                throw new ArgumentNullException(nameof(user), "Пользователь не может быть null.");
            }

            Logger?.Debug($"Попытка создать пользователя: {user.Login}");

            // Проверка на существование пользователя с таким же именем
            if (IsUserExists(user.Login))
            {
                Logger?.Error($"Неудачное создание пользователя : Пользователь {user.Login} уже существует.");
                throw new InvalidOperationException($"Пользователь '{user.Login}' уже существует.");
            }
            try
            {
                // Создание нового пользователя
                var newUser = new User
                {
                    Login = user.Login,
                    LastName = "TestLastName",
                    FirstName = "TestFirstName",
                    MiddleName = "TestMiddleName",
                    TelephoneNumber = "TestTelephoneNumber",
                    IsLead = false // По умолчанию, пользователь не является руководителем
                };
                var newPassword = new Sequrity
                {
                    UserId = user.Login,
                    Password = user.HashPassword
                };

                _context.Passwords.Add(newPassword);
                _context.Users.Add(newUser);
                _context.SaveChanges();

                Logger?.Debug($"Пользователь {user.Login} успешно создан.");
            }
            catch (Exception ex)
            {
                Logger?.Error($"Ошибка при создании пользователя {user.Login}: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger?.Debug("Попытка получить все свойства");
            try
            {
                // В этом списке перечислены все стандартные свойства пользователя
                var properties = new List<Property>
                {
                    new Property ( "LastName", "Фамилия пользователя"),
                    new Property ("FirstName", "Имя пользователя" ),
                    new Property ( "MiddleName", "Отчество пользователя" ),
                    new Property ( "TelephoneNumber", "Телефонный номер пользователя" ),
                    new Property ( "IsLead", "Является ли пользователь руководителем" ),
                    new Property ( "Password", "Пароль пользователя" )
                };

                return properties;
            }
            catch (Exception ex)
            {
                Logger?.Error($"При получении всех свойств произошла ошибка: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger?.Debug("Попытка получить свойства пользователя");
            try
            {
                // Находим пользователя по логину
                var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    Logger?.Error($"Пользователь с логином {userLogin} не найден.");
                    throw new ArgumentException("Пользователь не найден", nameof(userLogin));
                }

                // Создаем список свойств пользователя
                var properties = new List<UserProperty>
                {
                    new UserProperty("LastName", user.LastName),
                    new UserProperty("FirstName", user.FirstName),
                    new UserProperty("MiddleName", user.MiddleName),
                    new UserProperty("TelephoneNumber", user.TelephoneNumber),
                    new UserProperty("IsLead", user.IsLead.ToString())
                };

                return properties;
            }
            catch (Exception ex)
            {
                Logger?.Error($"При получении свойств пользователя произошла ошибка: {ex.Message}");
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            Logger?.Debug("Проверка на существование пользователя");
            try
            {
                bool userExists = _context.Users.AsNoTracking().Any(u => u.Login == userLogin);
                Logger?.Debug($"Проверка существования пользователя: {userLogin} - Существует: {userExists}");
                return userExists;
            }
            catch (Exception ex)
            {
                Logger?.Error($"Проверка ошибок при наличии пользователя: {userLogin}. Ошибка: {ex.Message}");
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger?.Debug("Попытка обновить свойства пользователя");
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    Logger?.Error($"Пользователя с логином {userLogin} не существует");
                    throw new ArgumentException("Пользователь не найден", nameof(userLogin));
                }

                var propertyHandlers = new Dictionary<string, Action<string>>
                {
                    ["LastName"] = value => user.LastName = value,
                    ["FirstName"] = value => user.FirstName = value,
                    ["MiddleName"] = value => user.MiddleName = value,
                    ["TelephoneNumber"] = value => user.TelephoneNumber = value,
                    ["IsLead"] = value =>
                    {
                        if (bool.TryParse(value, out bool isLeadValue))
                        {
                            user.IsLead = isLeadValue;
                        }
                        else
                        {
                            Logger?.Warn($"Некорректное значение для IsLead: {value}");
                        }
                    }
                };

                // Обновляем свойства
                foreach (var property in properties)
                {
                    if (propertyHandlers.TryGetValue(property.Name, out var handler))
                    {
                        handler(property.Value);
                        Logger?.Debug($"Обновлено свойство {property.Name} пользователя {userLogin}.");
                    }
                    else
                    {
                        Logger?.Warn($"Неизвестное свойство: {property.Name}");
                    }
                }

                // Сохраняем изменения в базе данных
                _context.SaveChanges();
                Logger?.Debug("Изменения свойств пользователя успешно сохранены.");
            }
            catch (Exception ex)
            {
                Logger?.Error($"При обновлении свойств пользователя произошла ошибка: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger?.Debug("Попытка получить все права");
            var permissions = new List<Permission>();
            try
            {
                Logger?.Debug("Извлечение разрешений из БД.");

                // Извлечение RequestRights и преобразование их в Permission
                foreach (var rr in _context.RequestRights.AsNoTracking())
                {
                    // Преобразуем Id из int? в string, обрабатывая null
                    string idAsString = rr.Id.HasValue ? rr.Id.Value.ToString() : "Неизвестное ID";
                    permissions.Add(new Permission(idAsString, rr.Name, "RequestRight"));
                    Logger?.Debug($"Добавлен разрешение RequestRight : {rr.Name} с ID {idAsString}");
                }

                // Извлечение ITRoles и преобразование их в Permission
                foreach (var ir in _context.ITRoles.AsNoTracking())
                {
                    // То же преобразование для ITRoles
                    string idAsString = ir.Id.HasValue ? ir.Id.Value.ToString() : "Неизвестное ID";
                    permissions.Add(new Permission(idAsString, ir.Name, "ITRole"));
                    Logger?.Debug($"Добавлено разрешение ITRole : {ir.Name} с ID {idAsString}");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error($"При получении всех разрешений произошла ошибка: {ex.Message}");
                throw;
            }
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger?.Debug("Попытка добавить пользователю права");
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    Logger?.Error($"Пользователя с логином {userLogin} не существует");
                    throw new ArgumentException("Пользователь не найден", nameof(userLogin));
                }

                var handlers = new Dictionary<string, Action<int>>
                {
                    [requestRightGroupName] = permissionId =>
                    {
                        var right = _context.RequestRights.FirstOrDefault(rr => rr.Id == permissionId);
                        if (right != null)
                        {
                            _context.UserRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = permissionId });
                            Logger?.Debug($"RequestRight с ID {permissionId} добавлено пользователю {userLogin}.");
                        }
                        else
                        {
                            Logger?.Warn($"RequestRight с ID {permissionId} не найдено.");
                        }
                    },
                    [itRoleRightGroupName] = permissionId =>
                    {
                        var role = _context.ITRoles.FirstOrDefault(ir => ir.Id == permissionId);
                        if (role != null)
                        {
                            _context.UserITRoles.Add(new UserITRole { UserId = userLogin, RoleId = permissionId });
                            Logger?.Debug($"ITRole с ID {permissionId} добавлено пользователю {userLogin}.");
                        }
                        else
                        {
                            Logger?.Warn($"ITRole с ID {permissionId} не найдено.");
                        }
                    }
                };

                foreach (var rightId in rightIds)
                {
                    var splittedRightId = rightId.Split(delemiter);
                    if (splittedRightId.Length != 2 || !int.TryParse(splittedRightId[1], out int permissionId))
                    {
                        Logger?.Warn($"Некорректный формат права: {rightId}");
                        continue;
                    }

                    if (handlers.TryGetValue(splittedRightId[0], out var handleAction))
                    {
                        handleAction(permissionId);
                    }
                    else
                    {
                        Logger?.Warn($"Неизвестный тип права: {splittedRightId[0]}");
                    }
                }

                _context.SaveChanges();
                Logger?.Debug("Изменения успешно сохранены.");
            }
            catch (Exception ex)
            {
                Logger?.Error($"При добавлении разрешений пользователя произошла ошибка: {ex.Message}");
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger?.Debug("Попытка удалить права пользователя");
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    Logger?.Error($"Пользователь с логином {userLogin} не найден");
                    throw new ArgumentException("Пользователь не найден", nameof(userLogin));
                }

                var handlers = new Dictionary<string, Action<int>>
                {
                    [requestRightGroupName] = permissionId =>
                    {
                        var userRequestRight = _context.UserRequestRights
                            .FirstOrDefault(urr => urr.UserId == userLogin && urr.RightId == permissionId);
                        if (userRequestRight != null)
                        {
                            _context.UserRequestRights.Remove(userRequestRight);
                            Logger?.Debug($"RequestRight с ID {permissionId} удалено у пользователя {userLogin}.");
                        }
                        else
                        {
                            Logger?.Warn($"RequestRight с ID {permissionId} не найдено.");
                        }
                    },
                    [itRoleRightGroupName] = permissionId =>
                    {
                        var userItRole = _context.UserITRoles
                            .FirstOrDefault(uit => uit.UserId == userLogin && uit.RoleId == permissionId);
                        if (userItRole != null)
                        {
                            _context.UserITRoles.Remove(userItRole);
                            Logger?.Debug($"ITRole с ID {permissionId} удалено у пользователя {userLogin}.");
                        }
                        else
                        {
                            Logger?.Warn($"ITRole с ID {permissionId} не найдено.");
                        }
                    }
                };

                foreach (var rightId in rightIds)
                {
                    var splittedRightId = rightId.Split(delemiter);
                    if (splittedRightId.Length != 2 || !int.TryParse(splittedRightId[1], out int permissionId))
                    {
                        Logger?.Warn($"Некорректный формат идентификатора права: {rightId}");
                        continue;
                    }

                    if (handlers.TryGetValue(splittedRightId[0], out var handleAction))
                    {
                        handleAction(permissionId);
                    }
                    else
                    {
                        Logger?.Warn($"Неизвестный тип права: {splittedRightId[0]}");
                    }
                }

                _context.SaveChanges();
                Logger?.Debug("Успешно удалены разрешения пользователя.");
            }
            catch (Exception ex)
            {
                Logger?.Error($"При удалении разрешений пользователя произошла ошибка: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger?.Debug("Попытка получить права пользователя");
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    throw new ArgumentException("Пользователь не найден", nameof(userLogin));
                }
                // Извлекаем ID всех RequestRights, связанных с пользователем
                var requestRightIds = _context.UserRequestRights
                                          .AsNoTracking()
                                          .Where(urr => urr.UserId == userLogin)
                                          .Select(urr => "RequestRight:" + urr.RightId.ToString())
                                          .ToList();

                // Извлекаем ID всех ITRoles, связанных с пользователем
                var itRoleIds = _context.UserITRoles
                                     .AsNoTracking()
                                     .Where(uit => uit.UserId == userLogin)
                                     .Select(uit => "ITRole:" + uit.RoleId.ToString())
                                     .ToList();

                // Объединяем списки прав и ролей в один общий список
                var allPermissions = new List<string>();
                allPermissions.AddRange(requestRightIds);
                allPermissions.AddRange(itRoleIds);

                return allPermissions;
            }
            catch (Exception ex)
            {
                Logger?.Error($"При пролучении разрешений пользователя произошла ошибка: {ex.Message}");
                throw;
            }
        }
    }
}