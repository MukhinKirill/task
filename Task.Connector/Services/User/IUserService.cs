using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.User
{
    public interface IUserService
    {
        public void CreateUser(UserToCreate user);
        bool IsUserExists(string userLogin);
        IEnumerable<Property> GetAllProperties();
        IEnumerable<UserProperty> GetUserProperties(string userLogin);
        void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
    }
}