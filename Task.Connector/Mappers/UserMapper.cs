using Task.Connector.Entities;
using Task.Connector.Extensions;
using Task.Connector.Records;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mappers
{
    public class UserMapper : IMapper<UserToCreate, (User, Password)>
    {
        private readonly UserProperties _configuration;
        public UserMapper(ConfigureManager configureManager) 
        {
            _configuration = configureManager.DbProperties.UserProperties;
        }

        public (User, Password) Map(UserToCreate userToCreate)
        {
            var properties = userToCreate.Properties.ConvertToDict();

            var user = new User()
            {
                Login = userToCreate.Login,
                FirstName = properties.GetValueOrEmpty(_configuration.FirstNamePropertyName),
                MiddleName = properties.GetValueOrEmpty(_configuration.MiddleNamePropertyName),
                LastName = properties.GetValueOrEmpty(_configuration.LastNamePropertyName),
                TelephoneNumber = properties.GetValueOrEmpty(_configuration.TelephoneNumberPropertyName),
                IsLead = properties.GetValueOrEmpty(_configuration.IsLeadPropertyName).EqualsIgnoreCase(true.ToString())
            };

            var password = new Password()
            {
                UserId = userToCreate.Login,
                PasswordProperty = userToCreate.HashPassword
            };

            return (user, password);
        }
    }
}
