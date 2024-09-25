using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

public class ConnectorDb : IConnector
{
    public ILogger Logger { get; set; }

    private DataContext? _dataContext;

    public void StartUp(string connectionConfig)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionConfig, nameof(connectionConfig));

        var connectionString = Regex.Match(connectionConfig, @"(?i)ConnectionString='(?<value>[^']*)'[;]?").Groups["value"].Value;
        var provider = Regex.Match(connectionConfig, @"(?i)Provider='(?<value>[^']*)'[;]?").Groups["value"].Value;

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<DataContext>();
        switch (provider)
        {
            case var p when p.StartsWith("postgresql", StringComparison.OrdinalIgnoreCase):
                dbContextOptionsBuilder.UseNpgsql(connectionString);
                break;

            case var p when p.StartsWith("sqlserver", StringComparison.OrdinalIgnoreCase):
                dbContextOptionsBuilder.UseSqlServer(connectionString);
                break;

            default:
                throw new ConnectorException($"unexpected provider {provider}");
        }

        _dataContext = new DataContext(dbContextOptionsBuilder.Options);

        Logger?.Debug($"DataContext initialised");
    }

    public void CreateUser(UserToCreate userToCreate)
    {
        var dataContext = GetDataContext();

        var user = new User();
        user.Login = userToCreate.Login;
        user.FirstName = string.Empty;
        user.MiddleName = string.Empty;
        user.LastName = string.Empty;
        user.TelephoneNumber = string.Empty;
        user.IsLead = false;

        user.SetProperties(userToCreate.Properties);

        var security = new Sequrity();
        security.UserId = userToCreate.Login;
        security.Password = userToCreate.HashPassword;

        dataContext.Users.Add(user);
        dataContext.Passwords.Add(security);

        dataContext.SaveChanges();

        Logger?.Debug($"user {userToCreate.Login} created");
    }

    public IEnumerable<Property> GetAllProperties()
    {
        return Properties.GetAll();
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        var dataContext = GetDataContext();

        var user = dataContext.Users.AsNoTracking().FirstOrDefault(u => u.Login == userLogin);

        if (user == null)
            throw new InvalidOperationException($"user {userLogin} is not exist");

        var properties = user.GetProperties();

        return properties;
    }

    public bool IsUserExists(string userLogin)
    {
        var dataContext = GetDataContext();

        return dataContext.Users.AsNoTracking().Any(u => u.Login == userLogin);
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        var dataContext = GetDataContext();

        var user = dataContext.Users.FirstOrDefault(u => u.Login == userLogin);

        if (user == null)
            throw new InvalidOperationException($"user {userLogin} is not exist");

        user.SetProperties(properties);

        dataContext.SaveChanges();

        Logger?.Debug($"properties: {string.Join(", ", properties.Select(p => p.Name))} updated for user {userLogin}");
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        var dataContext = GetDataContext();

        var permissions = new List<Permission>();

        var requestRights = dataContext.RequestRights.AsNoTracking().ToList().Select(r => r.ToPermission());
        permissions.AddRange(requestRights);

        var itRoles = dataContext.ITRoles.AsNoTracking().ToList().Select(r => r.ToPermission());
        permissions.AddRange(itRoles);

        return permissions;
    }

    // база данных должна проверять существование пользователя
    // не возвращает ошибку если пользователь уже имеет данные разрешения
    public void AddUserPermissions(string userLogin, IEnumerable<string> permissionIds)
    {
        var dataContext = GetDataContext();

        var requestRightsIds = dataContext.UserRequestRights.Where(r => r.UserId == userLogin).Select(r => r.RightId).ToList();
        var itRolesIds = dataContext.UserITRoles.Where(r => r.UserId == userLogin).Select(r => r.RoleId).ToList();

        foreach (var permissionId in permissionIds)
        {
            if (PermissionExtension.TryParsePermissionId(permissionId, out var type, out var id))
            {
                switch (type)
                {
                    case PermissionType.Request:
                        if (!requestRightsIds.Contains(id))
                            dataContext.UserRequestRights.Add(new UserRequestRight() { UserId = userLogin, RightId = id });
                        else
                            Logger?.Debug($"user {userLogin} already has permission {permissionId}");
                        break;

                    case PermissionType.Role:
                        if (!itRolesIds.Contains(id))
                            dataContext.UserITRoles.Add(new UserITRole() { UserId = userLogin, RoleId = id });
                        else
                            Logger?.Debug($"user {userLogin} already has permission {permissionId}");
                        break;

                    default:
                        throw new ConnectorException($"unexpected permission type {type}");
                }
            }
            else
            {
                throw new ConnectorException($"invalid permission id {permissionId}");
            }
        }

        dataContext.SaveChanges();

        Logger?.Debug($"permissions: {string.Join(", ", permissionIds)} added for user {userLogin}");
    }

    // не возвращает ошибку если пользователь не существует
    // не возвращает ошибку если ползователь не имеет данных разрешений
    public void RemoveUserPermissions(string userLogin, IEnumerable<string> permissionIds)
    {
        var dataContext = GetDataContext();

        var requestRightsIds = new List<int>();
        var itRolesIds = new List<int>();

        foreach (var permissionId in permissionIds)
        {
            if (PermissionExtension.TryParsePermissionId(permissionId, out var type, out var id))
            {
                switch (type)
                {
                    case PermissionType.Request:
                        requestRightsIds.Add(id);
                        break;

                    case PermissionType.Role:
                        itRolesIds.Add(id);
                        break;

                    default:
                        throw new ConnectorException($"unexpected permission type {type}");
                }
            }
            else
            {
                throw new ConnectorException($"invalid permission id {permissionId}");
            }
        }

        if (requestRightsIds.Count > 0)
            dataContext.UserRequestRights.RemoveRange(dataContext.UserRequestRights.Where(x => x.UserId == userLogin && requestRightsIds.Contains(x.RightId)));

        if (itRolesIds.Count > 0)
            dataContext.UserITRoles.RemoveRange(dataContext.UserITRoles.Where(r => r.UserId == userLogin && itRolesIds.Contains(r.RoleId)));

        dataContext.SaveChanges();

        Logger?.Debug($"permissions: {string.Join(", ", permissionIds)} removed for user {userLogin}");
    }

    // не возвращает ошибку если пользователь не существует
    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        var dataContext = GetDataContext();

        var permissionsIds = new List<string>();

        var requestRightsIds = dataContext.UserRequestRights.Where(r => r.UserId == userLogin).AsNoTracking().ToList().Select(r => r.GetPermissionId());
        permissionsIds.AddRange(requestRightsIds);

        var itRolesIds = dataContext.UserITRoles.Where(r => r.UserId == userLogin).AsNoTracking().ToList().Select(r => r.GetPermissionId());
        permissionsIds.AddRange(itRolesIds);

        return permissionsIds;
    }

    private DataContext GetDataContext() => _dataContext ?? throw new ConnectorException("DataContext is not initialised");
}