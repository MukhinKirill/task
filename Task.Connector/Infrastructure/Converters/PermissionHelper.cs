namespace Task.Connector.Infrastructure;

public static class PermissionHelper
{
    public static readonly string delimiter = ":";
    public static readonly string rolePrefix = "Role";
    public static readonly string requestRightPrefix = "Request";
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
            case string r when r.StartsWith($"{rolePrefix}{delimiter}"):
                return PermissionType.ItRole;
            case string r when r.StartsWith($"{requestRightPrefix}{delimiter}"):
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
                return int.Parse(permissionId.Substring(rolePrefix.Length + delimiter.Length));
            case PermissionType.RequestRight:
                return int.Parse(permissionId.Substring(requestRightPrefix.Length + delimiter.Length));
        }

        throw new FormatException($"Unable to parse permission: {permissionId}");
    }
}