using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TR.Connectors.Api.Entities.DTO.Requests;
using TR.Connectors.Api.Entities.Model;

namespace TR.Connectors.Api.Interfaces
{
    public interface IUserService
    {
        IEnumerable<Property> GetAllProperties();
        Task<IEnumerable<UserProperty>> GetUserPropertiesAsync(string userLogin, CancellationToken ct = default);
        Task UpdateUserPropertiesAsync(IEnumerable<UserProperty> properties, string userLogin, CancellationToken ct = default);
        Task<bool> IsUserExistsAsync(string userLogin, CancellationToken ct = default);
        Task CreateUserAsync(UserToCreate user, CancellationToken ct = default);
        Task LockUserAsync(string userLogin, CancellationToken ct = default);
        Task UnlockUserAsync(string userLogin, CancellationToken ct = default);

    }
}
