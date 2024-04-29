using Task.Integration.Data.Models.Models;

using UserObj = System.Collections.Generic.Dictionary<string, object>;

namespace Task.Connector.Models.ModelConversion
{
    public class UserConverter : IUserConverter
    {
        public IEnumerable<UserProperty> ExtractProperties(UserObj properties)
        {
            throw new NotImplementedException();
        }

        public UserObj ConstructUser(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Property> ConvertProperties(Dictionary<string, string> properties)
        {
            throw new NotImplementedException();
        }

        public UserObj ConvertUserToCreate(UserToCreate user)
        {
            throw new NotImplementedException();
        }
    }
}
