using Task.Integration.Data.Models.Models;

namespace Task.Connector.Interfaces;

public interface IPermissionService
{
    IEnumerable<Permission> GetAllPermissions();
    
    IEnumerable<string> GetPermissionByUserLogin(string userLogin);

    void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);

    void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
    
}