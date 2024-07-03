using Microsoft.EntityFrameworkCore;
using Task.Connector.Context;
using Task.Connector.Entities;
using Task.Connector.Errors;
using Task.Connector.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services;

public class UserService : IUserInterface
{
    private readonly DatabaseContext _context;

    private readonly ILogger _logger;
    
    public UserService(DatabaseContext context, ILogger logger)
    {
        _logger = logger;
        _context = context;
    }

    public void CreateUser(UserToCreate userToCreate)
    {
        var user = new User();
        foreach (var prop in user.GetType().GetProperties())
        {
            var property = userToCreate.Properties.FirstOrDefault(p => p.Name == prop.Name); 
            if ( property is null)
            {
                if (prop.PropertyType == typeof(bool)) prop.SetValue(user, false);
                else prop.SetValue(user, string.Empty);
            }
            else
            {
                if (prop.PropertyType == typeof(bool))
                    prop.SetValue(user, property.Value.ToLower().Contains("true"));
                else prop.SetValue(user, property.Value);
            }
        }
        user.login = userToCreate.Login;
        var nPassword = new Password() { userId = user.login, password1 = userToCreate.HashPassword };
        
        _context.Users.Add(user);
        _context.Passwords.Add(nPassword);
        _context.SaveChanges();
    }

    public User GetUserByLogin(string userLogin)
    {
        var user = _context.Users.FirstOrDefault(user => user.login == userLogin);
        if (user is null) Error.Throw(_logger, new ArgumentException($"GetUserProperties(userLogin) : Пользователя с логином {userLogin} не существует"));
        return user!;
    }

    public IEnumerable<Property> GetAllProperties()
    {
        var properties = new User().GetType().GetProperties();
        return properties
            .Select(property => new Property(property.Name, $"Тип свойства - {property.PropertyType}"))
            .Where(property => property.Name != "login")
            .Append(new Property("Пароль", "Пароль пользователя"));
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        var user = _context.Users.FirstOrDefault(user => user.login == userLogin);
        if (user is null) Error.Throw(_logger, new ArgumentException($"GetUserProperties(userLogin) : Пользователя с логином {userLogin} не существует"));
            
        foreach (var prop in properties)
        {
            var property = user!.GetType().GetProperties().FirstOrDefault(p => p.Name == prop.Name);
            if (property is null) throw new ArgumentException("Такого свойства нет");
            if(property.PropertyType == typeof(bool)) 
                property.SetValue(user, prop.Value.Contains("true", StringComparison.OrdinalIgnoreCase));
            else
                property.SetValue(user, prop.Value);
        }
        _context.SaveChanges();
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        var user = _context.Users.FirstOrDefault(user => user.login == userLogin);
        if (user is null) Error.Throw(_logger, new ArgumentException($"GetUserProperties(userLogin) : Пользователя с логином {userLogin} не существует"));
            
        var properties = user!.GetType().GetProperties();
        return properties
            .Select(property => new UserProperty(property.Name, property.GetValue(user)?.ToString() ?? string.Empty))
            .Where(property => property.Name != "login");
    }

    public bool IsUserExists(string userLogin)
    {
        return _context.Users.FirstOrDefault(user => user.login == userLogin) is not null;
    }
}