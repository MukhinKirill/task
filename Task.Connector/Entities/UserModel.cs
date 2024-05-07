using Task.Connector.Common;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Entities;
public sealed class UserModel
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string TelephoneNumber { get; set; }
    public bool IsLead { get; set; }

    public UserModel()
    {
        Login = Password = LastName = FirstName = MiddleName = TelephoneNumber = string.Empty;
        IsLead = false;
    }

    public UserModel(UserToCreate userToCreate, int countUsers = 0)
    {
        Login = string.IsNullOrWhiteSpace(userToCreate.Login) ? $"user_{countUsers + 1}" : userToCreate.Login.Trim();

        Password = string.IsNullOrWhiteSpace(userToCreate.HashPassword) ? "Password" : userToCreate.HashPassword.Trim();
        string? lastName = GetPropertyByName(userToCreate, PropertyName.LastName);

        LastName = string.IsNullOrWhiteSpace(lastName) ? string.Empty : lastName;
        string? firstName = GetPropertyByName(userToCreate, PropertyName.FirstName);

        FirstName = string.IsNullOrWhiteSpace(firstName) ? string.Empty : firstName;
        string? middleName = GetPropertyByName(userToCreate, PropertyName.FirstName);

        MiddleName = string.IsNullOrWhiteSpace(middleName) ? string.Empty : middleName;
        string? telephoneNumber = GetPropertyByName(userToCreate, PropertyName.TelephoneNumber);

        TelephoneNumber = string.IsNullOrWhiteSpace(telephoneNumber) ? string.Empty : telephoneNumber;
        string? isLead = GetPropertyByName(userToCreate, PropertyName.TelephoneNumber);

        IsLead = bool.TryParse(isLead, out bool isLeadBool) && isLeadBool;
    }

    public static IEnumerable<Property> GetPropertiesName()
    {
        yield return new Property(PropertyName.Password, "User password");
        yield return new Property(PropertyName.LastName, "User last name");
        yield return new Property(PropertyName.FirstName, "User first name");
        yield return new Property(PropertyName.MiddleName, "User middle name");
        yield return new Property(PropertyName.TelephoneNumber, "User telephone number");
        yield return new Property(PropertyName.IsLead, "Indicates whether the user is a lead");
    }

    public IEnumerable<UserProperty> GetProperties()
    {
        yield return new UserProperty(PropertyName.LastName, LastName);
        yield return new UserProperty(PropertyName.FirstName, FirstName);
        yield return new UserProperty(PropertyName.MiddleName, MiddleName);
        yield return new UserProperty(PropertyName.TelephoneNumber, TelephoneNumber);
        yield return new UserProperty(PropertyName.IsLead, IsLead.ToString());
    }

    private static string? GetPropertyByName(UserToCreate userToCreate, string propertyName)
    {
        return userToCreate.Properties.SingleOrDefault(property => property.Name == propertyName)?.Value?.Trim();
    }
}
