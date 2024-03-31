using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Extensions
{
    public static class UserExtensions
    {
        public static void ChangeProperties(this User user, IEnumerable<UserProperty> userProperties)
        {
            var properties = userProperties.ToDictionary(x => x.Name.ToLower(), x => x.Value);
            user.IsLead = properties.GetValueOrDefault(nameof(user.IsLead).ToLower()) == "true";
            user.TelephoneNumber = properties.GetValueOrDefault(nameof(user.TelephoneNumber).ToLower()) ?? user.TelephoneNumber ?? "";
            user.FirstName = properties.GetValueOrDefault(nameof(user.FirstName).ToLower()) ?? user.FirstName ?? "";
            user.LastName = properties.GetValueOrDefault(nameof(user.LastName).ToLower()) ?? user.LastName ?? "";
            user.MiddleName = properties.GetValueOrDefault(nameof(user.MiddleName).ToLower()) ?? user.MiddleName ?? "";
        }
    }
}
