using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure.DataModels;

/// <summary>
/// Модель пользователя.
/// </summary>
public sealed class UserDataModel
{
    /// <summary>
    /// Логин.
    /// </summary>
    public string Login { get; set; }
    /// <summary>
    /// Имя.
    /// </summary>
    public string FirstName { get; set; }
    /// <summary>
    /// Среднее имя.
    /// </summary>
    public string MiddleName { get; set; }
    /// <summary>
    /// Фамилия.
    /// </summary>
    public string LastName { get; set; }
    /// <summary>
    /// Номер телефона.
    /// </summary>
    public string TelephoneNumber { get; set; }
    /// <summary>
    /// Управляющий.
    /// </summary>
    public bool IsLead { get; set; }
    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// Пустой конструткор.
    /// </summary>
    public UserDataModel()
    {
    }
    
    /// <summary>
    /// Конструктор пользователя.
    /// </summary>
    /// <param name="userToCreate">Пользователь.</param>
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

    /// <summary>
    /// Получить все свойства.
    /// </summary>
    /// <returns>Свойства IEnumerable&lt;Property&gt;.</returns>
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
    
    /// <summary>
    /// Получить все свойства пользователя и их значения.
    /// </summary>
    /// <returns>Свойства пользователя IEnumerable&lt;Property&gt;.</returns>
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