using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.PermissionService;

internal class PermissionService : IPermissionService
{
    private DbContextFactory _contextFactory;
    private string _provider;

    internal PermissionService(DbContextFactory contextFactory, string provider)
    {
        _contextFactory = contextFactory;
        _provider = provider;
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        using var context = _contextFactory.GetContext(_provider);

        var requestRights = context.RequestRights
            .Select(rr => new Permission(rr.Id.ToString()!, rr.Name, ""))
            .ToArray();

        var itRoles = context.ITRoles
            .Select(itr => new Permission(itr.Id.ToString()!, itr.Name, ""))
            .ToArray();

        return requestRights.Union(itRoles);
    }
}
