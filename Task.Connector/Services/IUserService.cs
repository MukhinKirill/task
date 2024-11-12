using Task.Connector.Models;

namespace Task.Connector.Services;

public interface IUserService
{
    void AddUser(User user);
}