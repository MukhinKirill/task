using Task.Connector.Resources;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.DomainModels;

public static class ColumnsNames
{
    //Таблица User
    public const String FirstName = "firstName";
    public const String LastName = "lastName";
    public const String MiddleName = "middleName";
    public const String TelephoneNumber = "telephoneNumber";
    public const String IsLead = "isLead";
    public const String Login = "login";
    
    //Таблица Passwords
    public const String Password = "password";
    
    //Таблицы ItRole и RequestRights
    public const String Id = "id";
    public const String Name = "name";

    //Названия групп для изменения прав
    public const string RequestRightGroupName = "Request";
    public const string ItRoleRightGroupName = "Role";

    public static readonly IEnumerable<string> UserProperties = new [] { FirstName, LastName, MiddleName, TelephoneNumber, IsLead, Password };
    
    public static readonly IEnumerable<string> AbleToUpdateUserProperties = new [] { FirstName, LastName, MiddleName, TelephoneNumber, IsLead };
}