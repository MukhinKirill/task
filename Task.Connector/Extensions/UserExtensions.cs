using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Extensions
{
    internal static class UserExtensions
    {
        public static void SetProperties(this User user, IEnumerable<UserProperty> properties)
        {
            foreach (var property in properties)
            {
                switch (property.Name)
                {
                    case nameof(user.IsLead):
                        Boolean.TryParse(property.Value, out bool res);
                        user.IsLead = res;
                        break;
                    case nameof(user.TelephoneNumber):
                        user.TelephoneNumber = property.Value;
                        break;
                    case nameof(user.FirstName):
                        user.TelephoneNumber = property.Value;
                        break;
                    case nameof(user.MiddleName):
                        user.TelephoneNumber = property.Value;
                        break;
                    case nameof(user.LastName):
                        user.TelephoneNumber = property.Value;
                        break;
                }
            }
        }

        public static IEnumerable<UserProperty> GetProperties(this User user)
        {
            return new List<UserProperty>
            {
                new UserProperty(nameof(User.FirstName), user.FirstName),
                new UserProperty(nameof(User.MiddleName), user.MiddleName),
                new UserProperty(nameof(User.LastName), user.LastName),
                new UserProperty(nameof(User.TelephoneNumber), user.TelephoneNumber),
                new UserProperty(nameof(User.IsLead), user.IsLead.ToString())
            };
        }
    }
}
