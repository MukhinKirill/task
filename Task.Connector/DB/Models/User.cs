
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Task.Integration.Data.Models.Models;

[Table("User", Schema = "TestTaskSchema")]
public class User
{
    [Column("login")]
    public string Login { get; set; }

    [Column("lastName")]
    public string LastName { get; set; }

    [Column("firstName")]
    public string FirstName { get; set; }

    [Column("middleName")]
    public string MiddleName { get; set; }

    [Column("telephoneNumber")]
    public string TelephoneNumber { get; set; }

    [Column("isLead")]
    public bool IsLead { get; set; }
}