using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TR.Connectors.Api.Entities.Model;

namespace TR.Connectors.Api.Interfaces
{
    public interface IPermissionService
    {
        Task<IEnumerable<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin, CancellationToken ct = default);
        Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds, CancellationToken ct = default);
        Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds, CancellationToken ct = default);
    }
}
