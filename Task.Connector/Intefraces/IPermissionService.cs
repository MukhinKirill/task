using Task.Integration.Data.Models.Models;

namespace Task.Connector.Intefraces;

internal interface IPermissionService
{
	IEnumerable<Permission> GetAllPermissions();
}
