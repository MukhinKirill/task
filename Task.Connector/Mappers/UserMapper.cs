using Task.Connector.Entities;
using Task.Connector.Extensions;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mappers
{
    public class UserMapper : IMapper<UserToCreate, User>
    {
        public User Map(UserToCreate userToCreate)
        {
            var properties = userToCreate.Properties.ConvertToDict();
            var result = new User()
            {
                Login = userToCreate.Login,
                FirstName = properties.GetValueOrEmpty(Constants.FirstNamePropertyName),
                MiddleName = properties.GetValueOrEmpty(Constants.MiddleNamePropertyName),
                LastName = properties.GetValueOrEmpty(Constants.LastNamePropertyName),
                TelephoneNumber = properties.GetValueOrEmpty(Constants.TelephoneNumberPropertyName),
                IsLead = properties.GetValueOrEmpty(Constants.IsLeadPropertyName).EqualsIgnoreCase(true.ToString()) ? true : false
            };

            return result;
        }
    }
}
