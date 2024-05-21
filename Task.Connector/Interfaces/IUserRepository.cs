using Task.Integration.Data.Models.Models;

namespace Task.Connector.Interfaces
{
    public interface IUserRepository : IRepository
    {
        void CreateUser(UserToCreate user);
        IEnumerable<UserProperty> GetUserProperties(string userLogin);
        void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
        IEnumerable<Property> GetAllProperties();
        bool IsUserExists(string userLogin);
    }
}
