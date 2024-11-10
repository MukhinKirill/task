using Task.Connector.Helpers;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services;

public class UserService : IUserService
{
    private readonly DataContext _dataContext;

    public UserService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    #region Methods

    public void CreateUser(UserToCreate userToCreate)
    {
        if (string.IsNullOrEmpty(userToCreate.Login))
            throw new ArgumentException("Login is invalid.");

        if (string.IsNullOrEmpty(userToCreate.HashPassword))
            throw new ArgumentException("Password is invalid.");

        if (IsUserExists(userToCreate.Login))
            throw new InvalidOperationException($"User with login {userToCreate.Login} already exists.");
        
        var newUser = CreateUserAndSetProperties(userToCreate);
        
        using var transaction = _dataContext.Database.BeginTransaction();
        
        _dataContext.Users.Add(newUser);
        _dataContext.Passwords.Add(new Sequrity { UserId = userToCreate.Login, Password = userToCreate.HashPassword });
        _dataContext.SaveChanges();
        
        transaction.Commit();
    }
    
    public IEnumerable<Permission> GetAllPermissions()
    {
        var requestRights = _dataContext.RequestRights
            .Select(right => new Permission(right.Id.ToString(), right.Name, "RequestRight"))
            .ToList();
        var itRoles = _dataContext.ITRoles
            .Select(role => new Permission(role.Id.ToString(), role.Name, "ItRole"))
            .ToList();

        return requestRights.Concat(itRoles);
    }

    public bool IsUserExists(string login) => _dataContext.Users.Any(user => user.Login == login);
    
    public IEnumerable<UserProperty> GetUserProperties(string login)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == login);

        if (user is null)
            throw new InvalidOperationException($"User with login: {login} was not found.");

        var userProperties = typeof(User)
            .GetProperties()
            .Where(info => info.CanRead && info.Name != "Login")
            .Select(info => new UserProperty(info.Name, info.GetValue(user).ToString()));
        
        return userProperties;
    }
    
    public IEnumerable<Property> GetAllProperties()
    {
        var userProperties = typeof(User)
            .GetProperties()
            .Where(info => info.Name != "Login")
            .Select(info => new Property(info.Name, info.PropertyType.ToString()));

        var passwordProperties = typeof(Sequrity)
            .GetProperties()
            .Where(info => !info.Name.Contains("Id"))
            .Select(info => new Property(info.Name, info.PropertyType.Name));

        return userProperties.Concat(passwordProperties);
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string login)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == login);

        if (user is null)
            throw new InvalidOperationException($"User with login: {login} was not found.");
        
        using var transaction = _dataContext.Database.BeginTransaction();

        foreach (var userProperty in typeof(User).GetProperties())
        {
            var property = properties.FirstOrDefault(prop => prop.Name == CaseConverter.CamelToPascal(userProperty.Name));

            if (property is not null)
                userProperty.SetValue(user, property.Value);
        }

        _dataContext.Users.Update(user);
        _dataContext.SaveChanges();
        
        transaction.Commit();
    }

    public IEnumerable<string> GetUserPermissions(string login)
    {
        var userRequestRights = _dataContext.UserRequestRights
            .Where(right => right.UserId == login)
            .Select(right => right.RightId.ToString())
            .ToList();
        var userItRoles = _dataContext.UserITRoles
            .Where(role => role.UserId == login)
            .Select(role => role.RoleId.ToString())
            .ToList();

        return userRequestRights.Concat(userItRoles);
    }

    public void AddUserPermissions(string login, IEnumerable<string> rightIds)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == login);

        if (user is null)
            throw new InvalidOperationException($"User with login: {login} was not found.");

        var rights = ParsePermissions("Request", rightIds);
        var roles = ParsePermissions("Role", rightIds);

        using var transaction = _dataContext.Database.BeginTransaction();

        foreach (var right in rights)
            if (!_dataContext.UserRequestRights.Any(r => r.UserId == login && r.RightId == right))
                _dataContext.UserRequestRights.Add(new UserRequestRight { UserId = login, RightId = right });
        
        foreach (var role in roles)
            if (!_dataContext.UserITRoles.Any(r => r.UserId == login && r.RoleId == role))
                _dataContext.UserITRoles.Add(new UserITRole { UserId = login, RoleId = role });

        _dataContext.SaveChanges();
        
        transaction.Commit();
    }

    public void RemoveUserPermissions(string login, IEnumerable<string> rightIds)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == login);

        if (user is null)
            throw new InvalidOperationException($"User with login: {login} was not found.");

        var rights = ParsePermissions("Request", rightIds);
        var roles = ParsePermissions("Role", rightIds);

        using var transaction = _dataContext.Database.BeginTransaction();

        foreach (var right in rights)
        {
            var userRight = _dataContext.UserRequestRights.FirstOrDefault(r => r.UserId == login && r.RightId == right);
            
            if (userRight is not null)
                _dataContext.UserRequestRights.Remove(userRight);
        }

        foreach (var role in roles)
        {
            var userRole = _dataContext.UserITRoles.FirstOrDefault(r => r.UserId == login && r.RoleId == role);
            
            if (userRole is not null)
                _dataContext.UserITRoles.Remove(userRole);
        }

        _dataContext.SaveChanges();
        
        transaction.Commit();
    }

    #endregion

    #region Helpers

    private static User CreateUserAndSetProperties(UserToCreate user)
    {
        var newUser = new User();
        
        foreach (var userProperty in newUser.GetType().GetProperties())
        {
            var property = user.Properties.FirstOrDefault(u => u.Name == CaseConverter.PascalToCamel(userProperty.Name));

            if (property is not null)
            {
                var propertyInfo = typeof(User).GetProperty(CaseConverter.CamelToPascal(userProperty.Name));

                if (userProperty.PropertyType == typeof(bool))
                    propertyInfo.SetValue(newUser, bool.Parse(property.Value));
                else
                    propertyInfo.SetValue(newUser, property.Value);
            }
            else if (userProperty.PropertyType == typeof(string))
            {
                var propertyInfo = typeof(User).GetProperty(CaseConverter.CamelToPascal(userProperty.Name));
                propertyInfo.SetValue(newUser, string.Empty);
            }
        }

        newUser.Login = user.Login;

        return newUser;
    }

    private static IEnumerable<int> ParsePermissions(string type, IEnumerable<string> ids)
    {
        var permissions = new List<int>();

        foreach (var id in ids)
        {
            var parts = id.Split(':');

            if (parts.Length != 2 || !int.TryParse(parts[1], out var number))
                continue;

            if (parts[0].Contains(type))
                permissions.Add(number);
        }

        return permissions;
    }

    #endregion
}