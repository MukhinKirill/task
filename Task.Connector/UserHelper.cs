using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

public static class UserHelper
{
    public static void SetPropertiesToUser(this User user, IEnumerable<UserProperty> properties)
    {
        var isLead = false;
        var isLeadString = GetPropertyValueOrEmpty(properties, "isLead");

        if (isLeadString != null && !bool.TryParse(isLeadString, out isLead))
        {
            throw new ArgumentException("isLead property should be boolean.");
        } 

        user.FirstName = GetPropertyValueOrEmpty(properties, "firstName");
        user.LastName = GetPropertyValueOrEmpty(properties, "lastName");
        user.MiddleName = GetPropertyValueOrEmpty(properties, "middleName");
        user.TelephoneNumber = GetPropertyValueOrEmpty(properties, "telephoneNumber");
        user.IsLead = isLead;
    }

    private static string GetPropertyValueOrEmpty(IEnumerable<UserProperty> properties, string propertyName)
    {
        var value = properties.FirstOrDefault(x => x.Name == propertyName)?.Value;
        return value ?? "";
    }
}
