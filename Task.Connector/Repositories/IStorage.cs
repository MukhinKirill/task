using Task.Connector.Models;

namespace Task.Connector.Repositories
{
    public interface IStorage
    {
        MSSqlDbContext ConnectToDatabase();
        void AddUser(User user, Password password);
        User GetUserFromLogin(string userLogin);
        bool IsUserExists(string userLogin);
        void UpdateUser(User user);

        List<ItRole> GetAllItRoles();
        List<RequestRight> GetAllItRequestRights();

        List<ItRole> GetItRolesFromUser(string userLogin);
        List<RequestRight> GetItRequestRightsFromUser(string UserLogin);
        void AddRolesToUser(string userLogin, List<UserItrole> userItRoles);
        void AddRequestRightsToUser(string userLogin, List<UserRequestRight> userRequestRights);
        void RemoveRolesToUser(string userLogin, List<UserItrole> userItRoles);
        void RemoveRequestRightsToUser(string userLogin, List<UserRequestRight> userRequestRights);
    }
}
