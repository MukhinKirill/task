namespace Task.Connector.Domain;

public sealed class UserRequestRight : EntityBase
{
    public required string UserId { get; set; }
    public required int RightId { get; set; }
}
