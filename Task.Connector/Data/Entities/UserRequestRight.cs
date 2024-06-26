namespace Task.Connector.Data.Entities;

public class UserRequestRight
{
    public string UserId { get; set; }
    public int RightId { get; set; }

    public User User { get; set; }
    public RequestRight Right { get; set; }
}