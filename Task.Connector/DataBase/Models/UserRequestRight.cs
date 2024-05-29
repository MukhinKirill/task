using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.DataBase.Models;

[Table("UserRequestRight", Schema = "TestTaskSchema")]
public class UserRequestRight
{
    public string UserId {  get; set; }
    
    public int RightId {  get; set; }
}