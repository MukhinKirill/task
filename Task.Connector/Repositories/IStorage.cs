using Task.Connector.Models;

namespace Task.Connector.Repositories
{
    public interface IStorage
    {
        TestDbContext ConnectToDatabase();
        void AddUser(User user, Password password);
        User GetUserFromLogin(string userLogin);
        bool IsUserExists(string userLogin);
        void UpdateUser(User user);

        List<ItRole> GetAllItRoles();
        List<RequestRight> GetAllItRequestRights();

        List<ItRole> GetItRolesFromUser(string userLogin);
        List<RequestRight> GetItRequestRightsFromUser(string UserLogin);
    }
}
