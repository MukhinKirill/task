using TR.Connectors.Api.Entities;

namespace TR.Connectors.Api.Interfaces;
public interface IConnector
{
    public ILogger Logger { get; set; }
    Task StartUpAsync(string connectionString);
    Task CreateUserAsync(UserToCreate user);
    IEnumerable<Property> GetAllProperties();
    Task<IEnumerable<UserProperty>> GetUserPropertiesAsync(string userLogin);
    ValueTask<bool> IsUserExistsAsync(string userLogin);
    Task UpdateUserPropertiesAsync(IEnumerable<UserProperty> properties, string userLogin);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds);
    Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin);
}
