using Task.Connector.Models;

namespace Task.Connector.Data;

public interface IUserRepository
{
    void AddUser(User user);
}