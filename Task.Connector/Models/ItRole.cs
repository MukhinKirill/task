using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models;

public class ItRole
{
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("corporatePhoneNumber")]
    public string CorporatePhoneNumber { get; set; }
}