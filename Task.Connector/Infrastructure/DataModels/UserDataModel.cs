using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure.DataModels;

public class UserDataModel
{
    public string Login { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string TelephoneNumber { get; set; }
    public bool IsLead { get; set; }
    public string Password { get; set; }
    
    public UserDataModel()
    {
    }
    
    public UserDataModel(UserToCreate userToCreate)
    {
        Login = string.IsNullOrWhiteSpace(userToCreate.Login) ? Guid.NewGuid().ToString() : userToCreate.Login;
        FirstName = userToCreate.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? string.Empty;
        MiddleName = userToCreate.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? string.Empty;
        LastName = userToCreate.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? string.Empty;
        TelephoneNumber = userToCreate.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? string.Empty;
        IsLead = bool.Parse(userToCreate.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value ?? "false") ;
        Password = string.IsNullOrWhiteSpace(userToCreate.HashPassword) ? "NoPass" : userToCreate.HashPassword;
    }

    public static IEnumerable<Property> GetProperties()
    {
        return new List<Property>()
        {
            new(nameof(FirstName), "Имя пользователя"),
            new(nameof(MiddleName), "Среднее имя пользователя"),
            new(nameof(LastName), "Фамилия пользователя"),
            new(nameof(TelephoneNumber), "Телефон пользователя"),
            new(nameof(Password), "Пароль пользователя"),
            new(nameof(IsLead), "Пользователь руководитель")
        };
    }
    
    public IEnumerable<UserProperty> GetUserProperties()
    {
        return new List<UserProperty>()
        {
            new(nameof(FirstName), FirstName),
            new(nameof(MiddleName), MiddleName),
            new(nameof(LastName), LastName),
            new(nameof(TelephoneNumber), TelephoneNumber),
            new(nameof(IsLead), IsLead.ToString())
        };
    }
}