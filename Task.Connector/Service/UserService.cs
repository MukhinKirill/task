using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Models;
using Task.Connector.Service.Interface;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Service
{
    public class UserService : IUserService
    {
        public PropertyModel ParseProperties(IEnumerable<Integration.Data.Models.Models.UserProperty> userProperties)
        {
            PropertyModel property = new PropertyModel();

            foreach(var userProperty in userProperties)
            {
                switch (userProperty.Name)
                {
                    case "lastName":
                        property.LastName = userProperty.Value;
                        break;
                    case "firstName":
                        property.FirstName = userProperty.Value;
                        break;
                    case "middleName":
                        property.MiddleName = userProperty.Value;
                        break;
                    case "telephoneNumber":
                        property.TelephoneNumber = userProperty.Value;
                        break;
                    case "isLead":
                        property.IsLead = bool.Parse(userProperty.Value);

                        break;

                }
            }
            return property;
        }

        public IEnumerable<UserProperty> SerializeUserPropertyFromUser(User entity)
        {
            List<UserProperty> properties = new List<UserProperty>();

            var userProperties = typeof(User).GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() == null);
            foreach (var item in userProperties)
            {
                properties.Add(new UserProperty( char.ToLower(item.Name[0]) + item.Name.Substring(1), item.GetValue(entity)?.ToString()));
            }

            return properties;
        }
    }
}
