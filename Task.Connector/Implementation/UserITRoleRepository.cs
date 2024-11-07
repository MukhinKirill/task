using Task.Connector.Interfaces;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Implementation
{
    public class UserITRoleRepository : Repository<UserITRole>, IUserITRoleRepository
    {
        public UserITRoleRepository(Integration.Data.DbCommon.DataContext context) : base(context)
        {

        }

        public void AddUserRoles(string userId, string roleId)
        {
            if (int.TryParse(roleId, out int i))
                ObjectSet.Add(new UserITRole {UserId = userId, RoleId = i});
        }
    }
}
