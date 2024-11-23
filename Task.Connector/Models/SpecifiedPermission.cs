namespace Task.Connector.Models
{
    internal class SpecifiedPermission
    {
        public int Id { get; set; }
        public string Type { get; set; }

        public SpecifiedPermission(string type, string id)
        {
            Type = type;
            Id = int.Parse(id);
        }

        public SpecifiedPermission()
        {
            
        }
    }
}
