namespace Task.Connector.Domain;

public sealed class Security : EntityBase
{
    public required int Id { get; set; }
    public required string UserId { get; set; }
    public required string Password { get; set; }
}
