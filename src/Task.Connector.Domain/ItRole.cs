namespace Task.Connector.Domain;

public sealed class ItRole : EntityBase
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string CorporatePhoneNumber { get; set; }
}
