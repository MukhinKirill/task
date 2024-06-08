using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services;

public interface IConnectorService
{
    public void AddUser(UserToCreate user);

    public IEnumerable<Property> GetProperties();

    public bool IsUserExists(string userLogin);

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);

    public IEnumerable<UserProperty> GetUserProperties(string userLogin);

    public IEnumerable<Permission> GetPermissions();

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);

    public IEnumerable<string> GetUserPermissions(string userLogin);
}