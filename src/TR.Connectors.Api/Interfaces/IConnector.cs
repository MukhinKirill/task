using TR.Connectors.Api.Entities.DTO.Requests;
using TR.Connectors.Api.Entities.Model;

namespace TR.Connectors.Api.Interfaces
{
    public interface IConnector
    {
        //public ILogger Logger { get; set; }
        Task StartUpAsync(string connectionString);

        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin);
        Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds);
        Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds);

        IEnumerable<Property> GetAllProperties();

        Task<IEnumerable<UserProperty>> GetUserPropertiesAsync(string userLogin, CancellationToken ct = default);
        Task UpdateUserPropertiesAsync(IEnumerable<UserProperty> properties, string userLogin);

        Task<bool> IsUserExistsAsync(string userLogin);
        Task CreateUserAsync(UserToCreate user);
        Task LockUserAsync(string userLogin);
        Task UnlockUserAsync(string userLogin);

    }
}
