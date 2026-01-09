namespace TR.Connectors.Api.Entities.Model;

public sealed class Property
{
    public string Name { get; set; }
    public string Description { get; set; }

    public Property(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("name");
        Name = name;
        Description = description;
    }
}