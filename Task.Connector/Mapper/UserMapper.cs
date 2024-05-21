using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mapper;

public sealed class UserMapper
{
    public static User CreateUserFromUserProps(UserToCreate userToCreate)
    {
        if (userToCreate.Login == string.Empty)
            throw new ArgumentException($"Property 'Login' is required for creating a User.");

        var user = new User
        {
            Login = userToCreate.Login
        };

        if (!MapProperty(userToCreate.Properties, "LastName", (value) => user.LastName = value))
            throw new ArgumentException($"Property 'LastName' is required for creating a User.");

        if (!MapProperty(userToCreate.Properties, "FirstName", (value) => user.FirstName = value))
            throw new ArgumentException($"Property 'FirstName' is required for creating a User.");

        MapProperty(userToCreate.Properties, "MiddleName", (value) => user.MiddleName = value);
        MapProperty(userToCreate.Properties, "TelephoneNumber", (value) => user.TelephoneNumber = value);

        if (!MapBooleanProperty(userToCreate.Properties, "IsLead", (value) => user.IsLead = value))
            throw new ArgumentException($"Property 'IsLead' is required for creating a User.");

        return user;
    }

    private static bool MapProperty(IEnumerable<UserProperty> properties, string propertyName, Action<string> setProperty)
    {
        var property = properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        if (property != null)
        {
            setProperty(property.Value);
            return true;
        }

        return false;
    }

    private static bool MapBooleanProperty(IEnumerable<UserProperty> properties, string propertyName, Action<bool> setProperty)
    {
        var propertyValue = GetPropertyValue(properties, propertyName);
        if (propertyValue != null && bool.TryParse(propertyValue, out var result))
        {
            setProperty(result);
            return true;
        }

        return false;
    }

    private static string GetPropertyValue(IEnumerable<UserProperty> properties, string propertyName)
    {
        var property = properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        return property?.Value;
    }
}
