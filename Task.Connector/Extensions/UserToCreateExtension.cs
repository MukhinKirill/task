using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;
using CommunityToolkit.Diagnostics;

namespace Task.Connector.Extensions
{
    public static class UserToCreateExtension
    {
        public static User ConvertToUser(this UserToCreate value)
        {
            if (String.IsNullOrWhiteSpace(value.Login))
                ThrowHelper.ThrowInvalidDataException($"Не корректное значение логина \"{value.Login}\"");

            MaxLengthAttribute stringMaxLenghtAttr = typeof(User).GetProperty(nameof(User.Login))!.GetCustomAttribute<MaxLengthAttribute>()!;

            if (stringMaxLenghtAttr != null
                && !stringMaxLenghtAttr.IsValid(value.Login)
                )
                ThrowHelper.ThrowInvalidDataException($"Значение логина не может превышать \"{stringMaxLenghtAttr.Length}\"");

            User user = new User()
            {
                Login = value.Login,
                FirstName = String.Empty,
                LastName = String.Empty,
                MiddleName = String.Empty,
                TelephoneNumber = String.Empty
            };


            user.Login = value.Login;
            user.SetUserProperies(value.Properties);

            return user;
        }
    }
}
