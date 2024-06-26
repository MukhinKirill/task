namespace Task.Connector.Data.Entities;

public class UserItRole
{ 
    public string UserId { get; set; }
    public int RoleId { get; set; }
    
    public User User { get; set; }
    public ItRole Role { get; set; }
}