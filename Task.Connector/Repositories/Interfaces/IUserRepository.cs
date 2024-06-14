using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories.Interfaces;

public interface IUserRepository
{
    void CreateUser(User newUser);
    bool IsUserExists(string userLogin);
    User GetUserByLogin(string userLogin);
    void UpdateUserProperties(User user);
}