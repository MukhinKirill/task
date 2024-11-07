using System.Reflection;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure
{
    public static class PropertyHelper
    {
        public static IEnumerable<UserProperty> GetUserProperties(Task.Integration.Data.DbCommon.DbModels.User user)
        {
            var userProperties = user.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.CanRead && prop.Name != "Login")
                .Select(prop => new UserProperty(prop.Name, prop.GetValue(user)?.ToString()))
                .ToList();
            return userProperties;
        }
        public static IEnumerable<Property> GetAllProperties()
        {
            var properties = typeof(Task.Integration.Data.DbCommon.DbModels.User)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => prop.CanRead && prop.Name != "Login")
            .Select(prop => new Property(prop.Name, prop.PropertyType.ToString()));


            var passwordProperties = typeof(Sequrity)
                    .GetProperties()
                    .Where(prop => prop.Name != "Id" && prop.Name != "UserId")
                    .Select(prop => new Property(prop.Name, prop.PropertyType.ToString()));

            var result = properties.Concat(passwordProperties).ToList();
            return result;
        }
        public static Task.Integration.Data.DbCommon.DbModels.User UpdateUserProperties(IEnumerable<UserProperty> properties, Task.Integration.Data.DbCommon.DbModels.User user)
        {
            foreach (var property in properties)
            {
                var userProperty = user.GetType().GetProperty(property.Name);
                if (userProperty != null && userProperty.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(property.Value, userProperty.PropertyType);
                    userProperty.SetValue(user, convertedValue);
                }
            }
         return user;
        }
    }
}
