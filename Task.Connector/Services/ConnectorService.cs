using Microsoft.Extensions.Logging;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;
using ILogger = Task.Integration.Data.Models.ILogger;

namespace Task.Connector.Services;

public class ConnectorService : IConnectorService
{
    private const string ProviderName = "POSTGRE";

    private static DbContextFactory _dbContextFactory = null!;
    private static DataContext _dataContext = null!;
    
    public ILogger Logger { get; set; }

    public ConnectorService(string connectionString, ILogger logger)
    {
        _dbContextFactory = new DbContextFactory(connectionString);
        _dataContext = _dbContextFactory.GetContext(ProviderName);
        Logger = logger;
    }

    /// <inheritdoc />
    public void AddUser(UserToCreate user)
    {
        try
        {
            Logger.Debug("Начало транзакции по добавлению пользователя");
            _dataContext.Database.BeginTransaction();
            var validUserProperties = user.Properties
                .Select(x =>
                {
                    bool.TryParse(x.Value, out var result);
                    return (result, x.Name, x.Value);
                })
                .Select(x => new
                {
                    x.Name,
                    ValueIsBool = x.result,
                    x.Value,
                })
                .ToList();

            var userToCreate = new User
            {
                Login = user.Login,
                LastName = "null",
                FirstName = "null",
                MiddleName = "null",
                TelephoneNumber = "null",
                IsLead = false,

            };
            
            validUserProperties.ForEach(x =>
            {
                ApplyChangePropertyValue(
                    currentUser: userToCreate,
                    value: x.Value,
                    name: x.Name,
                    isBoolValue: x.ValueIsBool);
            });

            _dataContext.Users.Add(userToCreate);
            _dataContext.SaveChanges();
            _dataContext.Database.CommitTransaction();
            Logger.Debug("Пользователь добавлен");
        }
        catch (Exception e)
        {
            Logger.Error($"Ошибка {e.Message}");
            _dataContext.Database.RollbackTransaction();
        }
    }

    /// <inheritdoc />
    public IEnumerable<Property> GetProperties()
    {
        Logger.Debug("Начало выдачи всех характеристик");
        return typeof(User).GetProperties()
            .Select(x => new Property(x.Name, x.PropertyType.Name));
    }

    /// <inheritdoc />
    public bool IsUserExists(string userLogin)
        => _dataContext.Users.Any(x => x.Login == userLogin);

    /// <inheritdoc />
    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        try
        {
            _dataContext.Database.BeginTransaction();
            var currentUser = _dataContext.Users
                .FirstOrDefault(x => x.Login == userLogin)
                ?? throw new ArgumentNullException(userLogin);

            var userProperties = currentUser
                .GetType()
                .GetProperties()
                .Where(x => properties.Any(y => y.Name.Equals(x.Name)))
                .ToList();
            
            userProperties.ForEach(x =>
            {
                var prop = properties
                    .FirstOrDefault(y => y.Name.Equals(x.Name))?.Value
                    ?? throw new ArgumentNullException(x.Name);
                    
                var isBoolValue = bool.TryParse(prop, out var result);
                    
                ApplyChangePropertyValue(
                    currentUser: currentUser,
                    value: prop!,
                    name: x.Name,
                    isBoolValue: isBoolValue);
            });

            _dataContext.SaveChanges();
            _dataContext.Database.CommitTransaction();
        }
        catch (Exception e)
        {
            Logger.Error($"Error {e.Message}");
            _dataContext.Database.RollbackTransaction();
        }
    }

    /// <inheritdoc />
    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        Logger.Debug("Поиск пользователя");
        var currentUser = _dataContext.Users
            .FirstOrDefault(x => x.Login == userLogin)
            ?? throw new ArgumentNullException(userLogin);

        Logger.Debug("Пользователь найден, выдача всех характеристик");
        
        return currentUser
            .GetType()
            .GetProperties()
            .Where(x => x.Name != "Login")
            .Select(x => new UserProperty(x.Name, x.GetValue(currentUser)!.ToString()!))
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<Permission> GetPermissions()
    {
        Logger.Debug("Поиск всех прав");
        var requestRights = _dataContext.RequestRights
            .Select(x => new Permission(x.Id.ToString()!, x.Name, default!))
            .ToList();

        var itRoles = _dataContext.ITRoles
            .Select(x => new Permission(x.Id.ToString()!, x.Name, x.CorporatePhoneNumber))
            .ToList();
        
        Logger.Debug("Конкатенация списков");

        return requestRights.Concat(itRoles);
    }

    /// <inheritdoc />
    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        try
        {
            _dataContext.Database.BeginTransaction();
            var currentUser = _dataContext.Users
                .FirstOrDefault(x => x.Login == userLogin)
                ?? throw new ArgumentNullException(userLogin);
            
            rightIds.ToList().ForEach(x =>
            {
                var rights = x.Split(':');
                var permission = rights[0];
                var permissionId = rights[1];
                
                if (permission.Equals("Role", StringComparison.OrdinalIgnoreCase))
                    _dataContext.UserITRoles.Add(new UserITRole
                    {
                        UserId = currentUser.Login,
                        RoleId = int.TryParse(permissionId, out var result)
                            ? result
                            : throw new ApplicationException("Вы передали не число")
                    });
      
                if (permission.Equals("Request", StringComparison.OrdinalIgnoreCase))
                    _dataContext.UserRequestRights.Add(new UserRequestRight
                    {
                        UserId = currentUser.Login,
                        RightId = int.TryParse(permissionId, out var result)
                            ? result
                            : throw new ApplicationException("Вы передали не число")
                    });
            });

            _dataContext.SaveChanges();
            _dataContext.Database.CommitTransaction();
        }
        catch (Exception e)
        {
            Logger.Error($"Error {e.Message}");
            _dataContext.Database.RollbackTransaction();
        }
    }

    /// <inheritdoc />
    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        try
        {
            Logger.Debug("Начало транзакции");
            _dataContext.Database.BeginTransaction();
            rightIds.ToList().ForEach(x =>
            {
                var rights = x.Split(':');
                var permission = rights[0];
                var permissionId = int.TryParse(rights[1], out var persedId)
                    ? persedId
                    : throw new ApplicationException("Вы передали не число");

                if (permission.Equals("Role", StringComparison.OrdinalIgnoreCase))
                {
                    var permissionToDelete = _dataContext.UserITRoles
                        .FirstOrDefault(y => y.RoleId == permissionId)
                        ?? throw new ArgumentNullException(permissionId.ToString());
                    
                    _dataContext.UserITRoles.Remove(permissionToDelete);
                }

                if (permission.Equals("Request", StringComparison.OrdinalIgnoreCase))
                {
                    var permissionToDelete = _dataContext.UserRequestRights
                        .FirstOrDefault(y => y.RightId == permissionId)
                        ?? throw new ArgumentNullException(permissionId.ToString());
                    
                    _dataContext.UserRequestRights.Remove(permissionToDelete);
                }
            });

            _dataContext.SaveChanges();
            _dataContext.Database.CommitTransaction();
            Logger.Debug("Конец транзакции");
        }
        catch (Exception e)
        {
            Logger.Error($"Error {e.Message}");
            _dataContext.Database.RollbackTransaction();
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        Logger.Debug("Соединяем таблицу пользователей и прав доступа");
        return _dataContext.UserRequestRights
            .Where(x => x.UserId == userLogin)
            .GroupJoin(
                _dataContext.Users,
                i => i.UserId,
                o => o.Login,
                (i, _) => new
                {
                    i.RightId,
                })
            .Join(
                _dataContext.RequestRights,
                o => o.RightId,
                i => i.Id, (_, i) => i.Name)
            .ToList();
    }

    private void ApplyChangePropertyValue(
        User currentUser,
        dynamic value,
        string name,
        bool isBoolValue)
    {
        if (name.Equals(UserConstants.FirstName, StringComparison.OrdinalIgnoreCase))
            currentUser.FirstName = value;
                
        if (name.Equals(UserConstants.LastName, StringComparison.OrdinalIgnoreCase))
            currentUser.LastName = value;
                
        if (name.Equals(UserConstants.MiddleName, StringComparison.OrdinalIgnoreCase))
            currentUser.MiddleName = value;
                
        if (name.Equals(UserConstants.TelephoneNumber, StringComparison.OrdinalIgnoreCase))
            currentUser.TelephoneNumber = value;
                
        if (name.Equals(UserConstants.IsLead, StringComparison.OrdinalIgnoreCase) && isBoolValue)
            currentUser.IsLead = value;
    }
}