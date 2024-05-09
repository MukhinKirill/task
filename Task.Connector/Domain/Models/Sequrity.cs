namespace Task.Connector.Domain.Models;

public class Sequrity
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Password { get; set; } = null!;
}
