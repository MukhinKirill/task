using Task.Connector.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.RequestHandling
{
    public class RawPermissionRequestHandler : IRawPermissionRequestHandler
    {
        public void AddUserPermissions(IEnumerable<UserPermission> permissions)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserPermission> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(IEnumerable<UserPermission> permissions)
        {
            throw new NotImplementedException();
        }
    }
}
