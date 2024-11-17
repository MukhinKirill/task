using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models;

public class RequestRight
{
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
}