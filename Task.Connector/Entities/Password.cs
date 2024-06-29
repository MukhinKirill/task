namespace Task.Connector.Entities;

public class Password
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string PasswordProperty { get; set; } = null!;
}
