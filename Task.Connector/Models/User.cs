using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models;

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