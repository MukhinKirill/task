using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;
using System.Reflection;

namespace Task.Connector.Mapping.Converters
{
    internal class UserToPropertiesConverter : ITypeConverter<User, IEnumerable<UserProperty>>
    {
        public IEnumerable<UserProperty> Convert(User sourceUser, IEnumerable<UserProperty> destinationProperties, ResolutionContext context)
        {
            var type = sourceUser.GetType();
            var userProperties = new List<UserProperty>();
            foreach (var property in type.GetProperties())
            {
                if (property.Name == "Login") continue;
                userProperties.Add(new UserProperty(property.Name, property.GetValue(sourceUser).ToString()));
            }

            return userProperties.AsEnumerable();
        }
    }
}
