
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.DB.Models;

[Table("UserITRole", Schema = "TestTaskSchema")]
public class UserITRole
{
    [Column("userId")]
    public string UserId { get; set; }

    [Column("roleId")]
    public int RoleId { get; set; }
}