using System.ComponentModel.DataAnnotations.Schema;
using Task.Common.EntityFrameWork;

namespace Task.Domain.Users;

[Table("Passwords")]
public class Password : Entity
{
    public string userId { get; set; }
    public string password { get; set; }
}