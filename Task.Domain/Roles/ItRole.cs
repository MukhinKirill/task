using System.ComponentModel.DataAnnotations.Schema;
using Task.Common.EntityFrameWork;
using Task.Domain.Users;

namespace Task.Domain.Roles;

[Table("ItRole")]
public class ItRole : Entity
{
    public string name { get; set; }
    public string corporatePhoneNumber { get; set; }
    public ICollection<User> users { get; set; }
}