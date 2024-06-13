using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.Interfaces;

public interface IPermissionService
{
    IEnumerable<Permission> GetAllPermissions();
    void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
    IEnumerable<string> GetUserPermissions(string userLogin);
}