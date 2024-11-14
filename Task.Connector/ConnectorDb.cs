using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

public class ConnectorDb : IConnector, IDisposable
{
    private AppDbContext _context;

    public ConnectorDb() { }

    public void StartUp(string connectionString)
    {
        try
        {
            var match = Regex.Match(connectionString, @"ConnectionString='([^']+)';");
            if (!match.Success)
            {
                Logger?.Error("Не удалось распарсить строку подключения.");
                throw new InvalidOperationException("Invalid connection string format.");
            }

            var dbConnectionString = match.Groups[1].Value;
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            if (connectionString.Contains("PostgreSQL"))
            {
                optionsBuilder.UseNpgsql(dbConnectionString);
            }
            else if (connectionString.Contains("SqlServer"))
            {
                optionsBuilder.UseSqlServer(dbConnectionString);
            }
            else
            {
                Logger?.Error("Неизвестный тип подключения.");
                throw new InvalidOperationException("Unsupported database type.");
            }

            _context = new AppDbContext(optionsBuilder.Options);


            if (_context.Database.CanConnect())
                Logger?.Debug("Подключение к базе данных успешно создано.");
            else
                Logger?.Error("Не удалось подключиться к базе данных.");
        }
        catch (Exception ex)
        {
            Logger?.Error($"Ошибка подключения к базе данных: {ex.Message}");
        }
    }


    public void CreateUser(UserToCreate user)
    {
        try
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Login == user.Login);
            if (existingUser != null) throw new Exception("Пользователь с таким логином уже существует.");

            var newUser = new User
            {
                Login = user.Login,
                FirstName = user.Properties?.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "DefaultFirstName",
                LastName = user.Properties?.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "DefaultLastName",
                MiddleName = user.Properties?.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "DefaultMiddleName",
                TelephoneNumber = user.Properties?.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ??
                                  "0000000000",
                IsLead = user.Properties?.FirstOrDefault(p => p.Name == "isLead")?.Value == "true"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            var password = new Sequrity
            {
                UserId = user.Login,
                Password = user.HashPassword
            };

            _context.Passwords.Add(password);
            _context.SaveChanges();
            Logger?.Debug($"Пользователь {user.Login} создан.");
        }
        catch (Exception ex)
        {
            Logger?.Warn($"Ошибка при создании пользователя: {ex.Message}");
        }
    }


    public IEnumerable<Property> GetAllProperties()
    {
        try
        {
            var properties = new List<Property>
            {
                new("FirstName", "Имя пользователя"),
                new("LastName", "Фамилия пользователя"),
                new("MiddleName", "Отчество пользователя"),
                new("TelephoneNumber", "Номер телефона пользователя"),
                new("IsLead", "Лидерство пользователя"),
                new("Password", "Пароль пользователя")
            };

            return properties;
        }
        catch (Exception ex)
        {
            Logger?.Error($"Ошибка при получении всех свойств: {ex.Message}");
            throw;
        }
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger?.Warn($"Пользователь с логином {userLogin} не найден.");
                return Enumerable.Empty<UserProperty>();
            }

            return new List<UserProperty>
            {
                new("firstName", user.FirstName),
                new("lastName", user.LastName),
                new("middleName", user.MiddleName),
                new("telephoneNumber", user.TelephoneNumber),
                new("isLead", user.IsLead.ToString().ToLower())
            };
        }
        catch (Exception ex)
        {
            Logger?.Error($"Ошибка при получении свойств пользователя {userLogin}: {ex.Message}");
            return Enumerable.Empty<UserProperty>();
        }
    }

    public bool IsUserExists(string userLogin)
    {
        try
        {
            return _context.Users.Any(u => u.Login == userLogin);
        }
        catch (Exception ex)
        {
            Logger?.Error($"Ошибка при проверке существования пользователя: {ex.Message}");
            return false;
        }
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            if (user == null)
            {
                Logger?.Warn($"Пользователь с логином {userLogin} не найден.");
                return;
            }

            foreach (var property in properties)
                switch (property.Name.ToLower())
                {
                    case "firstname":
                        user.FirstName = property.Value;
                        break;
                    case "lastname":
                        user.LastName = property.Value;
                        break;
                    case "middlename":
                        user.MiddleName = property.Value;
                        break;
                    case "telephonenumber":
                        user.TelephoneNumber = property.Value;
                        break;
                    case "islead":
                        if (bool.TryParse(property.Value, out var isLead))
                            user.IsLead = isLead;
                        else
                            Logger?.Warn(
                                $"Не удалось обновить свойство 'isLead' для пользователя {userLogin}. Неверный формат значения.");
                        break;
                    default:
                        Logger?.Warn($"Неизвестное свойство {property.Name} для пользователя {userLogin}.");
                        break;
                }

            _context.SaveChanges();
            Logger?.Debug($"Свойства пользователя {userLogin} успешно обновлены.");
        }
        catch (Exception ex)
        {
            Logger?.Error($"Ошибка при обновлении свойств пользователя {userLogin}: {ex.Message}");
        }
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        try
        {
            var permissions = new List<Permission>();

            var requestRights = _context.RequestRights.ToList();
            if (requestRights != null && requestRights.Any())
                foreach (var right in requestRights)
                    permissions.Add(new Permission(right.Id.ToString(), right.Name,
                        "Описание права из RequestRight"));
            else
                Logger?.Warn("Не удалось получить RequestRights.");

            var itRoles = _context.ItRoles.ToList();
            if (itRoles != null && itRoles.Any())
                foreach (var role in itRoles)
                    permissions.Add(new Permission(role.Id.ToString(), role.Name, "Описание роли из ItRole"));
            else
                Logger?.Warn("Не удалось получить ITRoles.");


            Logger?.Debug("Все права успешно получены.");
            return permissions;
        }
        catch (Exception ex)
        {
            Logger?.Error($"Ошибка при получении всех прав: {ex.Message}");
            throw;
        }
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        try
        {
            var roles = rightIds
                .Where(id => id.Contains("Role"))
                .Select(id => id.Split(':')[1])
                .ToList();

            var rights = rightIds
                .Where(id => id.Contains("Request"))
                .Select(id => id.Split(':')[1])
                .ToList();


            foreach (var roleId in roles)
            {
                var roleIdInt = int.Parse(roleId);
                var role = _context.ItRoles.FirstOrDefault(r => r.Id == roleIdInt);
                if (role != null)
                    _context.UserItRoles.Add(new UserITRole
                    {
                        UserId = userLogin,
                        RoleId = role.Id.Value
                    });
            }

            foreach (var rightId in rights)
            {
                var rightIdInt = int.Parse(rightId);
                var right = _context.RequestRights.FirstOrDefault(r => r.Id == rightIdInt);
                if (right != null)
                    _context.UserRequestRights.Add(new UserRequestRight
                    {
                        UserId = userLogin,
                        RightId = right.Id.Value
                    });
            }

            _context.SaveChanges();
            Logger?.Debug($"Пользователю {userLogin} добавлены права.");
        }
        catch (Exception ex)
        {
            Logger?.Error($"Ошибка при добавлении прав пользователю {userLogin}: {ex.Message}");
        }
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        try
        {
            var roles = rightIds
                .Where(id => id.Contains("Role"))
                .Select(id => id.Split(':')[1])
                .ToList();

            var rights = rightIds
                .Where(id => id.Contains("Request"))
                .Select(id => id.Split(':')[1])
                .ToList();

            foreach (var roleId in roles)
            {
                var roleIdInt = int.Parse(roleId);
                var role = _context.UserItRoles
                    .FirstOrDefault(uitr => uitr.UserId == userLogin && uitr.RoleId == roleIdInt);
                if (role != null) _context.UserItRoles.Remove(role);
            }

            foreach (var rightId in rights)
            {
                var rightIdInt = int.Parse(rightId);
                var right = _context.UserRequestRights
                    .FirstOrDefault(urr => urr.UserId == userLogin && urr.RightId == rightIdInt);
                if (right != null) _context.UserRequestRights.Remove(right);
            }

            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        try
        {
            var requestRights = _context.UserRequestRights
                .Where(urr => urr.UserId == userLogin)
                .Join(_context.RequestRights, urr => urr.RightId, rr => rr.Id, (urr, rr) => rr.Name)
                .ToList();

            if (!requestRights.Any()) requestRights = new List<string>();

            var roles = _context.UserItRoles
                .Where(uitr => uitr.UserId == userLogin)
                .Join(_context.ItRoles, utr => utr.RoleId, ir => ir.Id, (utr, ir) => ir.Name)
                .ToList();

            if (!roles.Any()) roles = new List<string>();

            var allPermissions = requestRights.Concat(roles).ToList();

            Logger?.Debug($"Права пользователя {userLogin} получены успешно.");
            return allPermissions;
        }
        catch (Exception ex)
        {
            Logger?.Error($"Ошибка при получении прав пользователя {userLogin}: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    public ILogger Logger { get; set; }

    public void Dispose()
    {
        _context?.Dispose();
    }
}