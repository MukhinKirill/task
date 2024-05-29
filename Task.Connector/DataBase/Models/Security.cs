namespace Task.Connector.DataBase.Models;

public class Security
{
    public Security(string userId, string password)
    {
        UserId = userId;
        Password = password;
    }

    public int? Id {get; set; }

    public string UserId {  get; set; }

    public string Password { get; set; }
}