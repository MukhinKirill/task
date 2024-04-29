using Task.Integration.Data.Models.Models;

using UserObj = System.Collections.Generic.Dictionary<string, object>;

namespace Task.Connector.RequestHandling
{
    public interface IRawUserRequestHandler
    {
        public void CreateUser(UserObj user);

        public IEnumerable<Property> GetAllProperties();

        public UserObj GetUserProperties(string userLogin);

        public bool IsUserExists(string userLogin);

        public void UpdateUserProperties(UserObj user);
    }
}
