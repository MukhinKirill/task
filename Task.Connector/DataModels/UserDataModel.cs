
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Task.Integration.Data.Models.Models;

[Table("User", Schema="TestTaskSchema")]
public class UserDataModel
{
  [Column("login")]
  [Description("Login description")]
  public string Login { get; set; }

  [Column("lastName")]
  [Description("LastName description")]
  public string LastName { get; set; }

  [Column("firstName")]
  [Description("FirstName description")]
  public string FirstName { get; set; }

  [Column("middleName")]
  [Description("MiddleName description")]
  public string MiddleName { get; set; }
  
  [Column("telephoneNumber")]
  [Description("TelephoneNumber description")]
  public string TelephoneNumber { get; set; }
  
  [Column("isLead")]
  [Description("IsLead description")]
  public bool IsLead { get; set; }
}