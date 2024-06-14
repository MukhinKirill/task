using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories.Interfaces;

public interface IUserRequestRightRepository
{
    IEnumerable<UserRequestRight> GetAllUserRequestRight();
    IQueryable<UserRequestRight> GetUserRequestsRightsByLogin(string login);
        
    void AddUserRequestRight(List<UserRequestRight> userRequestRights);
    void RemoveUserRequestRights(List<UserRequestRight> userRequestRights);
}