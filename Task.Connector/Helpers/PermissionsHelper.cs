using System.Text.RegularExpressions;

namespace Task.Connector;

public static partial class PermissionsHelper
{
    public static (int Id, PermissionType Type) ExtractPermissionIdAndType(this string rightId)
    {
        var match = RightRegex().Match(rightId);

        if (!match.Success)
        {
            throw new FormatException("The permission right id must be in the format \"Role:{itRole id}\" or \"Request:{requestRight id}\".");
        }

        if (!int.TryParse(match.Groups[2].Value, out var id))
        {
            throw new FormatException("Invalid id format.");
        }

        return match.Groups[1].Value switch
        {
            "Role" => (id, PermissionType.ItRole),
            "Request" => (id, PermissionType.RequestRight),
            _ => throw new FormatException("Invalid permission type.")
        };
    }

    [GeneratedRegex(@"(^\S+):(\d+)$")]
    private static partial Regex RightRegex();
}

public enum PermissionType
{
    ItRole,
    RequestRight
}
