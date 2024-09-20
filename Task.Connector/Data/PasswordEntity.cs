
namespace Task.Connector.Data;

public partial class PasswordEntity
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Password { get; set; } = null!;
}
