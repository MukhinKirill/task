using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories
{
    public interface IUserRepository
    {
        void CreateUser(UserToCreate user);
        IEnumerable<Property> GetAllProperties();
        IEnumerable<UserProperty> GetUserProperties(string userLogin);
        bool IsUserExists(string userLogin);
        void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
    }
}
