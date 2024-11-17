using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models;

public class Password
{
    [Column("id")]
    public int Id { get; set; }
    [Column("userId")]
    public string UserId { get; set; }
    [Column("password")]
    public string UserPassword { get; set; }
}