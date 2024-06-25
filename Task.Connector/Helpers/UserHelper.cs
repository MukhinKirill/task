using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Helpers;

public static class UserHelper
{
    public static readonly Property[] AllProperties = new Property[] {
        new("firstName", "First name"),
        new("lastName", "Last name"),
        new("middleName", "Middle name"),
        new("telephoneNumber", "Telephone number"),
        new("isLead", "Is the user a lead?"),
        new("password", "User password"),
    };

    public static void SetPropertiesToUser(this User user, IEnumerable<UserProperty> properties)
    {
        user.FirstName = GetPropertyValueOrEmpty(properties, "firstName");
        user.LastName = GetPropertyValueOrEmpty(properties, "lastName");
        user.MiddleName = GetPropertyValueOrEmpty(properties, "middleName");
        user.TelephoneNumber = GetPropertyValueOrEmpty(properties, "telephoneNumber");
        user.IsLead = GetPropertyValueOrEmpty(properties, "isLead") == "true";
    }

    private static string GetPropertyValueOrEmpty(IEnumerable<UserProperty> properties, string propertyName)
    {
        var value = properties.FirstOrDefault(x => x.Name == propertyName)?.Value;
        return value ?? "";
    }
}
