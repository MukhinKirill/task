using Task.Connector.Entities;
using Task.Connector.Extensions;
using Task.Connector.Records;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mappers
{
    public class UserMapper : IMapper<UserToCreate, User>
    {
        private UserProperties _configuration;
        public UserMapper(ConfigureManager configureManager) 
        {
            _configuration = configureManager.DbProperties.UserProperties;
        }

        public User Map(UserToCreate userToCreate)
        {
            var properties = userToCreate.Properties.ConvertToDict();
            var result = new User()
            {
                Login = userToCreate.Login,
                FirstName = properties.GetValueOrEmpty(_configuration.FirstNamePropertyName),
                MiddleName = properties.GetValueOrEmpty(_configuration.MiddleNamePropertyName),
                LastName = properties.GetValueOrEmpty(_configuration.LastNamePropertyName),
                TelephoneNumber = properties.GetValueOrEmpty(_configuration.TelephoneNumberPropertyName),
                IsLead = properties.GetValueOrEmpty(_configuration.IsLeadPropertyName).EqualsIgnoreCase(true.ToString()) ? true : false
            };

            return result;
        }
    }
}
