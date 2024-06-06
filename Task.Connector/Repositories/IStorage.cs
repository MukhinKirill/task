using Task.Connector.Models;

namespace Task.Connector.Repositories
{
    internal interface IRepository
    {
        void AddUser(User user, Password password);
        void AddRolesToUser(string userLogin, List<UserItrole> userItRoles);
        void AddRequestRightsToUser(string userLogin, List<UserRequestRight> userRequestRights);


        void RemoveRolesToUser(string userLogin, List<UserItrole> userItRoles);
        void RemoveRequestRightsToUser(string userLogin, List<UserRequestRight> userRequestRights);


        List<ItRole> GetAllItRoles();
        List<RequestRight> GetAllItRequestRights();
        User GetUserFromLogin(string userLogin);
        List<ItRole> GetItRolesFromUser(string userLogin);
        List<RequestRight> GetItRequestRightsFromUser(string UserLogin);


        bool IsUserExists(string userLogin);


        void UpdateUser(User user);
    }
}
