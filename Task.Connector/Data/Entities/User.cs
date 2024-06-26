namespace Task.Connector.Data.Entities;

public class User
{
    public string Login { get; init; } = String.Empty;
    public string LastName { get; set; } = String.Empty;
    public string FirstName { get; set; } = String.Empty;
    public string MiddleName { get; set; } = String.Empty;
    public string TelephoneNumber { get; set; } = String.Empty;
    public bool IsLead { get; set; }
}