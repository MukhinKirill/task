using Task.Connector.Models;

namespace Task.Connector.Repositories
{
    public interface IStorage
    {
        TestDbContext ConnectToDatabase();
        void AddUser(User user, Password password);
        User GetUserFromLogin(string userLogin);
        bool IsUserExists(string userLogin);
    }
}
