using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Task.Domain.Roles;

namespace Task.Domain.Users;

[Table("User")]
public class User
{
    public string login { get; set; }
    public string firstName { get; set; }
    public string middleName { get; set; }
    public string lastName { get; set; }
    public string telephoneNumber { get; set; }
    public bool isLead { get; set; }
    /*public ICollection<RequestRight> requestRights { get; set; }
    public ICollection<ItRole> itRoles { get; set; }*/
}