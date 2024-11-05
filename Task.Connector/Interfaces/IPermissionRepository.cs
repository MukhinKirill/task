namespace Task.Connector.Interfaces;

public interface IPermissionRepository
{
    IEnumerable<Task.Integration.Data.Models.Models.Permission> GetAllPermissions();
    void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
    void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
    IEnumerable<string> GetUserPermissions(string userLogin);
}
