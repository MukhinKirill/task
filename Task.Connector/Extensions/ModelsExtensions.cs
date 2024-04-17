using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Extensions
{
    public static class ModelsExtensions
    {
        public static User SetPropertiesOrDefault (this UserToCreate userToCreate)
        {
            User user = new User
            {
                Login = userToCreate.Login,
                FirstName = userToCreate.Properties.GetProperty("firstName"),
                MiddleName = userToCreate.Properties.GetProperty("middleName"),
                LastName = userToCreate.Properties.GetProperty("lastName"),
                TelephoneNumber = userToCreate.Properties.GetProperty("telephoneNumber"),
                IsLead = userToCreate.Properties.GetProperty("islead") == "true"
            };

            return user;
        }

        public static string GetProperty(this IEnumerable<UserProperty> properties, string propertyKey)
        {
            foreach (var property in properties)
            {
                if (property.Name == propertyKey) return property.Value;
            }
            return "undefined";
        }
    }
}
