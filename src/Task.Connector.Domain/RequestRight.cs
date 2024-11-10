namespace Task.Connector.Domain;

public sealed class RequestRight : EntityBase
{
    public required int Id { get; set; }
    public required string Name { get; set; }
}
