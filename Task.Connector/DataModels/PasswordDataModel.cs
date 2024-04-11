
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.DataModels;

[Table("Passwords", Schema = "TestTaskSchema")]
public class PasswordDataModel : IEntity
{
  [Column("id")]
  public int Id { get; set; }

  [Column("userId")]
  public required string UserId { get; set; }

  [Column("password")]
  [Description("Password description")]
  public required string Password { get; set; }
}