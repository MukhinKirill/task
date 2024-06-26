using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories
{
    public interface IPermissionRepository
    {
        IEnumerable<Permission> GetAllPermissions();
        void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
        void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
        public IEnumerable<string> GetUserPermissions(string userLogin);
    }
}
