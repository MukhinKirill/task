
using System.ComponentModel.DataAnnotations.Schema;

[Table("UserRequestRight", Schema = "TestTaskSchema")]
public class UserRequestRight
{
    [Column("userId")]
    public string UserId { get; set; }

    [Column("rightId")]
    public int RightId { get; set; }
}