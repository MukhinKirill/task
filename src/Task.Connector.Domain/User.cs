namespace Task.Connector.Domain;

public sealed class User : EntityBase
{
    public required string Login { get; set; }
    public required string LastName { get; set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string TelephoneNumber { get; set; }
    public required bool IsLead { get; set; }
}
