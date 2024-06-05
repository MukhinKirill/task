using System.ComponentModel.DataAnnotations.Schema;
using Task.Common.EntityFrameWork;

namespace Task.Domain.Users;

[Table("RequestRight")]
public class RequestRight : Entity
{
    public string name { get; set; }
    public ICollection<User> users { get; set; }
}