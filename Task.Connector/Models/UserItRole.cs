using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models;

public class UserItRole
{
    [Column("userId")]
    public string UserId { get; set; }
    [Column("roleId")]
    public int ItRoleId { get; set; }
}