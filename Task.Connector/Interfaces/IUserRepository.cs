
using Task.Integration.Data.Models.Models;

namespace AvanpostGelik.Connector.Interfaces;

public interface IUserRepository
{
    void CreateUser(UserToCreate user);
    bool CheckUserExists(string login);
    IEnumerable<UserProperty> GetUserProperties(string userLogin);
    void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
}
