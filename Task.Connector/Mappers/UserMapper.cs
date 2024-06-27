using Task.Connector.Entities;
using Task.Connector.Extensions;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mappers
{
    public class UserMapper : IMapper<UserToCreate, User>
    {
        public User Map(UserToCreate userToCreate)
        {
            var properties = new Dictionary<string, string>();
            var result = new User();
            result.Login = userToCreate.Login;

            foreach(var property in userToCreate.Properties)
                properties.Add(property.Name, property.Value);

            result.FirstName = properties.GetValueOrEmpty("firstName");
            result.MiddleName = properties.GetValueOrEmpty("middleName");
            result.LastName = properties.GetValueOrEmpty("lastName");
            result.TelephoneNumber = properties.GetValueOrEmpty("telephoneNumber");
            result.IsLead = properties.GetValueOrEmpty("isLead").ToLower() == "true" ? true : false;

            return result;
        }
    }
}
