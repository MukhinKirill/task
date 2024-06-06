namespace Task.Connector.Attributes
{
    internal class PropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string? DefaultValue { get; set; }

        public PropertyAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
