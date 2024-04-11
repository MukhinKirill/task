using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Helpers;
public static class UserHelper
{
    public static User Map(UserToCreate source)
    {
        var user = new User()
        {
            Login = source.Login,
        };

        LoadUserProperties(user, source.Properties);

        return user;
    }

    public static void LoadUserProperties(User user, IEnumerable<UserProperty> userProperties)
    {
        user.IsLead = userProperties
            .FirstOrDefault(p => p.Name.ToLower() == nameof(user.IsLead).ToLower())?.Value == "true";
        user.FirstName = userProperties.FirstOrDefault(p => p.Name.ToLower() == nameof(user.FirstName).ToLower())?.Value
            ?? user.FirstName ?? string.Empty;
        user.LastName = userProperties.FirstOrDefault(p => p.Name.ToLower() == nameof(user.LastName).ToLower())?.Value
            ?? user.LastName ?? string.Empty;
        user.MiddleName = userProperties.FirstOrDefault(p => p.Name.ToLower() == nameof(user.MiddleName).ToLower())?.Value
            ?? user.MiddleName ?? string.Empty;
        user.TelephoneNumber = userProperties.FirstOrDefault(p => p.Name.ToLower() == nameof(user.TelephoneNumber).ToLower())?.Value
            ?? user.TelephoneNumber ?? string.Empty;
    }
}
