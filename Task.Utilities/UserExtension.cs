using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Utilities
{
    public enum UserPropertyType
    {
        IsLead = 0,
        FirstName = 1,
        LastName = 2,
        MiddleName = 3,
        TelephoneNumber = 4
    }

    public static class UserExtension
    {
        public static void Parse(this User user, UserToCreate userToCreate)
        {
            if (userToCreate == null || userToCreate.Login == null) throw new ArgumentNullException("Fail to parse UserToCreate");

            var props = userToCreate.Properties.ToDictionary(x => x.Name.ToLower(), x => x.Value);

            user.Login = userToCreate.Login;
            user.FirstName = props.GetValueOrDefault(UserPropertyType.FirstName.ToString().ToLower()) ?? user.FirstName ?? "";
            user.LastName = props.GetValueOrDefault(UserPropertyType.LastName.ToString().ToLower()) ?? user.LastName ?? "";
            user.MiddleName = props.GetValueOrDefault(UserPropertyType.MiddleName.ToString().ToLower()) ?? user.MiddleName ?? "";
            user.IsLead = props.GetValueOrDefault(UserPropertyType.IsLead.ToString().ToLower()) == "true";
            user.TelephoneNumber = props.GetValueOrDefault(UserPropertyType.TelephoneNumber.ToString().ToLower()) ?? user.TelephoneNumber ?? "";

        }
    }
}
