
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.DataModels;

[Table("UserITRole", Schema="TestTaskSchema")]
public class UserITRoleDataModel
{
  [Column("userId")]
  public string UserId { get; set; }
  
  [Column("roleId")]
  public int RoleId { get; set; }
}