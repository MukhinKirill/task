using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.UserPropertyService;

internal class UserPropertyService : IUserPropertyService
{
    private DbContextFactory _contextFactory;
    private string _provider;

    internal UserPropertyService(DbContextFactory contextFactory, string provider)
    {
        _contextFactory = contextFactory;
        _provider = provider;
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        using var context = _contextFactory.GetContext(_provider);

        var user = context.Users.FirstOrDefault(u => u.Login == userLogin);

        if (user is null)
        {
            throw new Exception($"Пользователь с логином '{userLogin}' не найден.");
        }

        return MapUserToProperties(user);
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        using var context = _contextFactory.GetContext(_provider);

        var user = context.Users.FirstOrDefault(u => u.Login == userLogin);

        if (user is null)
        {
            throw new Exception($"Пользователь с логином '{userLogin}' не найден.");
        }

        foreach (var property in properties)
        {
            UpdateUserProperty(user, property);
        }

        context.SaveChanges();
    }

    private static IEnumerable<UserProperty> MapUserToProperties(User user)
    {
        return new[]
        {
            new UserProperty("lastName", user.LastName),
            new UserProperty("firstName", user.FirstName),
            new UserProperty("middleName", user.MiddleName),
            new UserProperty("telephoneNumber", user.TelephoneNumber),
            new UserProperty("isLead", user.IsLead.ToString())
        };
    }

    private static void UpdateUserProperty(User user, UserProperty property)
    {
        switch (property.Name)
        {
            case "lastName":
                user.LastName = property.Value;
                break;
            case "firstName":
                user.FirstName = property.Value;
                break;
            case "middleName":
                user.MiddleName = property.Value;
                break;
            case "telephoneNumber":
                user.TelephoneNumber = property.Value;
                break;
            case "isLead":
                if (bool.TryParse(property.Value, out bool isLead))
                {
                    user.IsLead = isLead;
                }
                else
                {
                    throw new ArgumentException($"Недопустимое значение для свойства isLead: {property.Value}");
                }
                break;
            default:
                throw new ArgumentException($"Неизвестное свойство: {property.Name}");
        }
    }
}
