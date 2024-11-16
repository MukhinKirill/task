namespace Task.Connector.Infrastructure;

public static class PermissionHelper
{
    /// <summary>
    /// Get Permission type from the id.
    /// This does NOT validate the id, only gets the perm type
    /// </summary>
    /// <param name="permissionId"></param>
    /// <returns></returns>
    public static PermissionType GetPermissionTypeFromId(this string permissionId)
    {
        switch (permissionId)
        {
            case string r when r.StartsWith("role-"):
                return PermissionType.ItRole;
            case string r when r.StartsWith("right-"):
                return PermissionType.RequestRight;
            default:
                throw new ArgumentException($"Could not parse permission type: got permission id {permissionId}");
        }
    }

    public static int GetClientPermissionIdFromString(this string permissionId)
    {
        switch (GetPermissionTypeFromId(permissionId))
        {
            case PermissionType.ItRole:
                return int.Parse(permissionId.Substring(4));
            case PermissionType.RequestRight:
                return int.Parse(permissionId.Substring(5));
        }

        throw new FormatException($"Unable to parse permission: {permissionId}");
    }

    public static IQueryable MatchDBSetToPermissionType(this IQueryable query)
    {
        throw new NotImplementedException();
    }
}