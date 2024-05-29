using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Extensions
{
    public static class CreateUserExtensions
    {
        public static User SetOrDefaultProperties(this UserToCreate userToCreate)
        {
            return new User()
            {
                Login = userToCreate.Login,
                FirstName = userToCreate.Properties.GetValueOrEmpty("FirstName"),
                MiddleName = userToCreate.Properties.GetValueOrEmpty("MiddleName"),
                LastName = userToCreate.Properties.GetValueOrEmpty("FirstName"),
                TelephoneNumber = userToCreate.Properties.GetValueOrEmpty("LastName"),
                IsLead = userToCreate.Properties.GetValueOrEmpty("IsLead") == "true"
            };
        }

        private static string? GetValueOrEmpty(this IEnumerable<UserProperty> properties, string needProperty)
        {
            var p = properties.FirstOrDefault(p => p.Name == needProperty);
            return p == null ? "" : p.Value;
        }
    }
}

