using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Domain.Users
{
    public static class UserExtensions
    {
        private static readonly Dictionary<string, Action<User, string>> PropertySetters = new()
        {
            { UserPropertyNamesConst.LastName, (user, value) => user.LastName = value },
            { UserPropertyNamesConst.FirstName, (user, value) => user.FirstName = value },
            { UserPropertyNamesConst.MiddleName, (user, value) => user.MiddleName = value },
            { UserPropertyNamesConst.TelephoneNumber, (user, value) => user.TelephoneNumber = value },
            { UserPropertyNamesConst.IsLead, (user, value) => user.IsLead = bool.TryParse(value, out var isLead) && isLead }
        };

        private static readonly Dictionary<string, Func<User, string>> PropertyGetters = new()
        {
            { UserPropertyNamesConst.FirstName, user => user.FirstName },
            { UserPropertyNamesConst.LastName, user => user.LastName },
            { UserPropertyNamesConst.MiddleName, user => user.MiddleName },
            { UserPropertyNamesConst.TelephoneNumber, user => user.TelephoneNumber },
            { UserPropertyNamesConst.IsLead, user => user.IsLead.ToString() }
        };

        public static void MapProperties(this User user, IEnumerable<UserProperty> properties)
        {
            foreach (var property in properties)
            {
                if (PropertySetters.TryGetValue(property.Name, out var setter))
                {
                    setter(user, property.Value);
                }
                else
                {
                    throw new ArgumentException($"Invalid property name: {property.Name}", nameof(properties));
                }
            }
        }

        public static IEnumerable<UserProperty> ToUserProperties(this User user)
        {
            return PropertyGetters.Select(kvp => new UserProperty(kvp.Key, kvp.Value(user)));
        }
    }
}