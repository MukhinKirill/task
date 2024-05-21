using System.Reflection;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mapper;

public sealed class UserMapper
{
    public static User CreateUserFromUserProps(IEnumerable<UserProperty> props)
    {
        var user = new User();

        foreach (var property in user.GetType().GetProperties().Where(p => p.Name != "Login"))
        {
            var propertyName = property.Name;
            var propertyValue = props.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.Value;

            if (propertyValue != null)
            {
                if (property.PropertyType == typeof(bool))
                {
                    bool parsedValue = Convert.ToBoolean(propertyValue);
                    property.SetValue(user, parsedValue, null);
                }
                else
                {
                    object parsedValue = Convert.ChangeType(propertyValue, property.PropertyType);
                    property.SetValue(user, parsedValue, null);
                }
            }
            else
            {
                throw new NullReferenceException($"The Properties has not value for { propertyName } field");
            }
        }

        return user;
    }

    public static bool SetUserProperty(User user, string propName, object value)
    {
        var prop = user.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (prop == null)
            return false;

        if (prop.PropertyType == typeof(bool))
            value = bool.Parse(value.ToString());

        prop.SetValue(user, value);
        return true;
    }
}
