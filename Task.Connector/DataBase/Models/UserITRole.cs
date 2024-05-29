using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.DataBase.Models;

[Table("UserITRole", Schema = "TestTaskSchema")]
public class UserITRole
{
    public string UserId { get; set; }
    
    public int RoleId { get; set; }
}