using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.PermissionService;

internal interface IPermissionService
{
    IEnumerable<Permission> GetAllPermissions();
}
