using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.UserService;

internal interface IUserService
{
    bool IsUserExists(string userLogin);
    void CreateUser(UserToCreate user);
}
