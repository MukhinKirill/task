using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Interfaces
{
    public interface IUserITRoleRepository : IRepository<UserITRole>
    {
        void AddUserRoles(string userId, string roleId);
    }
}
