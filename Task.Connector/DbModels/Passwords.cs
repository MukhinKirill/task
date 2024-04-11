namespace Task.Connector.DbModels;

public partial class Passwords
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Password { get; set; } = null!;
}
