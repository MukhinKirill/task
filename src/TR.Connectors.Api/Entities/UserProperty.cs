namespace TR.Connectors.Api.Entities;

public sealed class UserProperty
{
    public string Name { get; set; }
    public string Value { get; set; }

    public UserProperty(string name, string value)
    {
        if (string.IsNullOrEmpty(name))
            throw new Exception("name");
        Name = name;
        Value = value;
    }
}