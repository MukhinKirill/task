using TR.Connectors.Api.Entities;

namespace TR.Connectors.Api.Interfaces;
public interface IConnector
{
    public ILogger Logger { get; set; }
    void StartUp(string connectionString);
    void CreateUser(UserToCreate user);
    IEnumerable<Property> GetAllProperties();
    IEnumerable<UserProperty> GetUserProperties(string userLogin);
    bool IsUserExists(string userLogin);
    void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
    IEnumerable<Permission> GetAllPermissions();
    void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
    void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
    IEnumerable<string> GetUserPermissions(string userLogin);
}
