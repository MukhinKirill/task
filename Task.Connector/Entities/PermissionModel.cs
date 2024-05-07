namespace Task.Connector.Entities;
public sealed class PermissionModel
{
    public const string RequestRightGroupName = "Request";
    public const string ItRoleRightGroupName = "Role";
    public const string Delimeter = ":";

    public string Name { get; set; }
    public int Id { get; set; }

    public PermissionModel()
    {
        Name = string.Empty;
        Id = 0;
    }

    public PermissionModel(int? id, string name)
    {
        Id = id ?? 0;
        Name = name ?? string.Empty;
    }

    public PermissionModel(string permission)
    {
        string[] permissionParts = permission.Split(Delimeter);
        if (permissionParts.Length != 2)
        {
            throw new ArgumentException("Invalid permission format");
        }

        if (int.TryParse(permissionParts[1], out int id))
        {
            Name = permissionParts[0];
            Id = id;
        }
        else
        {
            throw new ArgumentException("Invalid permission format");
        }
    }

    public override string ToString()
    {
        return $"{Name}{Delimeter}{Id}";
    }
}
