
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.DB.Models;

[Table("Passwords", Schema = "TestTaskSchema")]
public class Password
{
    [Column("id")]
    public int Id { get; set; }

    [Column("userId")]
    public required string UserId { get; set; }

    [Column("password")]
    public required string Pass { get; set; }
}