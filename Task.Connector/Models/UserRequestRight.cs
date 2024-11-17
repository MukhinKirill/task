using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models;

public class UserRequestRight
{
    [Column("userId")]
    public string UserId { get; set; }
    [Column("rightId")]
    public int RequestRightId { get; set; }
}