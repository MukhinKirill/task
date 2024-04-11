using System.ComponentModel.DataAnnotations.Schema;

[Table("ItRole", Schema="TestTaskSchema")]
public class ItRoleDataModel
{
  [Column("id")]
  public int Id { get; set; }

  [Column("name")]
  public required string Name { get; set; }

  [Column("corporatePhoneNumber")]
  public required string CorporatePhoneNumber { get; set; }
}