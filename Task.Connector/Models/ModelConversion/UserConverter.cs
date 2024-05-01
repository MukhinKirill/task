using Task.Integration.Data.Models.Models;

using UserObj = System.Collections.Generic.Dictionary<string, object>;

namespace Task.Connector.Models.ModelConversion
{
    public class UserConverter : IUserConverter
    {
        // В теории для быстрой аллокации памяти можно использовать пул объектов,
        // но я счёл это излишним
        public IEnumerable<UserProperty> ExtractProperties(UserObj properties)
        {
            var userProperties = from property in properties
                                 select new UserProperty(property.Key, (string) property.Value);

            return userProperties;
        }

        public UserObj ConstructUser(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = new UserObj { { "login", userLogin } };

            foreach (var property in properties)
            {
                user[property.Name] = property.Value;
            }

            return user;
        }

        public IEnumerable<Property> ConvertProperties(Dictionary<string, string> properties)
        {
            var propertyList = from property in properties
                               select new Property(property.Key, property.Value);

            return propertyList;
        }

        public UserObj ConvertUserToCreate(UserToCreate user)
        {
            var userObj = new UserObj { { "login", user.Login }, { "password", user.HashPassword } };

            foreach (var property in user.Properties)
            {
                userObj[property.Name] = property.Value;
            }

            return userObj;
        }
    }
}
