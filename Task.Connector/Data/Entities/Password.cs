namespace Task.Connector.Data.Entities;

public class Password
{
    public int Id { get; init; }
    public string UserId { get; init; } = String.Empty;
    public string PasswordValue { get; init; } = String.Empty;
}