namespace Task.Connector.Interfaces
{
    public interface IUserRequestRightRepository : IRepository<Task.Integration.Data.DbCommon.DbModels.UserRequestRight>
    {
        void AddUserPermissions(string userId, string rightId);
        void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
    }
}
