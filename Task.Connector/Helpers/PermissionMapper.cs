using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Helpers;
public class PermissionMapper
{
    public static IEnumerable<Permission> Map(IEnumerable<RequestRight> requestRights)
    {
        if (requestRights is null || !requestRights.Any())
            return null!;

        return requestRights.Select(rr => new Permission(rr.Id?.ToString(), rr.Name, string.Empty));
    }

    public static IEnumerable<Permission> Map(IEnumerable<ITRole> iTRoles)
    {
        if (iTRoles is null || !iTRoles.Any())
            return null!;

        return iTRoles.Select(ir => new Permission(ir.Id?.ToString(), ir.Name, string.Empty));
    }
}
