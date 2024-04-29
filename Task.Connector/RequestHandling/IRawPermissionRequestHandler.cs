using Task.Connector.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.RequestHandling
{
    public interface IRawPermissionRequestHandler
    {
        public IEnumerable<Permission> GetAllPermissions();

        public void AddUserPermissions(IEnumerable<UserPermission> permissions);

        public void RemoveUserPermissions(IEnumerable<UserPermission> permissions);

        public IEnumerable<UserPermission> GetUserPermissions(string userLogin);
    }
}
