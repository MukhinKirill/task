using Task.Integration.Data.Models.Models;

using UserObj = System.Collections.Generic.Dictionary<string, object>;

namespace Task.Connector.RequestHandling
{
    public class RawUserRequestHandler : IRawUserRequestHandler
    {
        public void CreateUser(UserObj user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
        }

        public UserObj GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }

        public bool IsUserExists(string userLogin)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserProperties(UserObj user)
        {
            throw new NotImplementedException();
        }
    }
}
