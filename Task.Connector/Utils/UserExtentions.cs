using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Utils;

public static class UserExtentions
{
    public static bool WithProperty(this User user, string propName, object value)
    {
        switch (propName.ToLowerInvariant())
        {
            case "lastname":
                user.LastName = Convert.ToString(value);
                return true;
            case "firstname":
                user.FirstName = Convert.ToString(value);
                return true;
            case "middlename":
                user.MiddleName = Convert.ToString(value);
                return true;
            case "telephonenumber":
                user.TelephoneNumber = Convert.ToString(value);
                return true;
            case "islead":
                user.IsLead = Convert.ToBoolean(value);
                return true;
            default:
                return false;
        }
    }
}
