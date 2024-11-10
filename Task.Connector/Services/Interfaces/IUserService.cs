using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.Interfaces;

public interface IUserService
{
    void CreateUser(UserToCreate userToCreate);

    IEnumerable<Permission> GetAllPermissions();
    
    bool IsUserExists(string login);

    IEnumerable<Property> GetAllProperties();
    
    IEnumerable<UserProperty> GetUserProperties(string login);
    
    void UpdateUserProperties(IEnumerable<UserProperty> properties, string login);
    
    IEnumerable<string> GetUserPermissions(string login);
    
    void AddUserPermissions(string login, IEnumerable<string> rightIds);
    
    void RemoveUserPermissions(string login, IEnumerable<string> rightIds);
}