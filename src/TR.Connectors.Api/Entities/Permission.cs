namespace TR.Connectors.Api.Entities;

public sealed class Permission
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public Permission(string id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id))
            throw new Exception(string.IsNullOrWhiteSpace(name) ? "name" : "id");
        Id = id;
        Name = name;
        Description = description;
    }
}