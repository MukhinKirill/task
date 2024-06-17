namespace Task.Connector.Services.UserPermission
{
    public class UserPermissionService : IUserPermission
    {
        public void AddUserPermissions(string userLogin, IEnumerable<string> permissionIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> permissionIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }
    }
}