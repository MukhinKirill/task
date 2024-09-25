using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

static class UserExtension
{
    public static IEnumerable<UserProperty> GetProperties(this User user)
    {
        var properties = new List<UserProperty>
        {
            new UserProperty(PropertyType.FirstName, user.FirstName),
            new UserProperty(PropertyType.MiddleName, user.MiddleName),
            new UserProperty(PropertyType.LastName, user.LastName),
            new UserProperty(PropertyType.TelephoneNumber, user.TelephoneNumber),
            new UserProperty(PropertyType.IsLead, user.IsLead.ToString()),
        };
        return properties;
    }
    public static void SetProperties(this User user, IEnumerable<UserProperty> properties)
    {
        foreach (var property in properties)
        {
            if (property.Name.Equals(PropertyType.FirstName, StringComparison.OrdinalIgnoreCase))
            {
                user.FirstName = property.Value;
                continue;
            }

            if (property.Name.Equals(PropertyType.MiddleName, StringComparison.OrdinalIgnoreCase))
            {
                user.MiddleName = property.Value;
                continue;
            }

            if (property.Name.Equals(PropertyType.LastName, StringComparison.OrdinalIgnoreCase))
            {
                user.LastName = property.Value;
                continue;
            }

            if (property.Name.Equals(PropertyType.TelephoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                user.TelephoneNumber = property.Value;
                continue;
            }

            if (property.Name.Equals(PropertyType.IsLead, StringComparison.OrdinalIgnoreCase))
            {
                if (bool.TryParse(property.Value, out var isLead))
                    user.IsLead = isLead;
                else
                    new ConnectorException($"invalid property {PropertyType.IsLead}");
                continue;
            }

            throw new ConnectorException($"unexpected property type {property.Name}");
        }
    }
}
