using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models
{
    public class PermissionType
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public PermissionType(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public PermissionType(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public Permission ToPermission()
        {
            // Так как оригинальный Permission не имеет nullable в Description,
            // но он подразумевается, то пришлось подавить warning

#pragma warning disable CS8604 // Possible null reference argument.
            return new Permission(Id, Name, Description);
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
