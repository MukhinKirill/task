
using System.ComponentModel.DataAnnotations.Schema;

[Table("RequestRight", Schema="TestTaskSchema")]
public class RequestRightDataModel : IEntity
{
  [Column("id")]
  public int Id { get; set; }

  [Column("name")]
  public required string Name { get; set; }
}