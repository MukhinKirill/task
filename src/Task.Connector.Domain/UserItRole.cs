namespace Task.Connector.Domain;

public sealed class UserItRole : EntityBase
{
    public required string UserId { get; set; }
    public required int RoleId { get; set; }
}
