
using System.ComponentModel.DataAnnotations.Schema;

[Table("UserRequestRight", Schema="TestTaskSchema")]
public class UserRequestRightDataModel
{
  [Column("userId")]
  public string UserId { get; set; }
  
  [Column("rightId")]
  public int RightId { get; set; }
}