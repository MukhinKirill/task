namespace Task.Connector.Models;

public partial class Password
{
    public int Id { get; set; }

    public User UserId { get; set; } = null!;

    public string Password1 { get; set; } = null!;
}
