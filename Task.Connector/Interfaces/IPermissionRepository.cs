using Task.Integration.Data.Models.Models;

namespace Task.Connector.Interfaces;

public interface IPermissionRepository : IRepository
{
    IEnumerable<Permission> GetAllPermissions();
    void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
    void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
    IEnumerable<string> GetUserPermissions(string userLogin);
}
