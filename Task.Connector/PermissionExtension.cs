using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

static class PermissionExtension
{
    public static string GetPermissionId(this RequestRight requestRight) => $"{PermissionType.Request}:{requestRight.Id}";
    public static string GetPermissionId(this ITRole itRole) => $"{PermissionType.Role}:{itRole.Id}";

    public static string GetPermissionId(this UserRequestRight requestRight) => $"{PermissionType.Request}:{requestRight.RightId}";
    public static string GetPermissionId(this UserITRole itRole) => $"{PermissionType.Role}:{itRole.RoleId}";

    public static Permission ToPermission(this RequestRight requestRight) => new Permission(requestRight.GetPermissionId(), requestRight.Name, string.Empty);
    public static Permission ToPermission(this ITRole itRole) => new Permission(itRole.GetPermissionId(), itRole.Name, string.Empty);

    public static bool TryParsePermissionId(string permissionId, out string type, out int id)
    {
        var parts = permissionId?.Split(':');

        if (parts != null && parts.Length == 2 && !string.IsNullOrEmpty(parts[0]) && int.TryParse(parts[1], out var i))
        {
            type = parts[0];
            id = i;
            return true;
        }
        else
        {
            type = string.Empty;
            id = -1;
            return false;
        }
    }
}