using Microsoft.EntityFrameworkCore;
using Task.Connector.Extensions;
using Task.Connector.Mappers;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private TaskDbContext _dbContext;
        private UserMapper _mapper;

        public UserRepository(TaskDbContext dbContext, UserMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public void CreateUser(UserToCreate user)
        {
            _dbContext.Users.Add(_mapper.Map(user));
            _dbContext.SaveChanges();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var properties = new List<Property>
            {
                new Property("lastName", string.Empty),
                new Property("firstName", string.Empty),
                new Property("middleName", string.Empty),
                new Property("telephoneNumber", string.Empty),
                new Property("isLead", string.Empty),
                new Property("password", string.Empty)
            };

            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var userProperties = new List<UserProperty>();

            var user = _dbContext.Users.Where(user => user.Login == userLogin).FirstOrDefault();

            if(user != null)
            {
                userProperties.Add(new UserProperty("lastName", user.LastName));
                userProperties.Add(new UserProperty("firstName", user.FirstName));
                userProperties.Add(new UserProperty("middleName", user.MiddleName));
                userProperties.Add(new UserProperty("telephoneNumber", user.TelephoneNumber));
                userProperties.Add(new UserProperty("isLead", user.IsLead.ToString()));
            }

            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            return _dbContext.Users.Where(user => user.Login == userLogin).FirstOrDefault() != null;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var propertyDict = new Dictionary<string, string>();

            foreach (var property in properties)
            {
                propertyDict.Add(property.Name, property.Value);
            }

            _dbContext.Users
                .Where(user => user.Login == userLogin)
                .ExecuteUpdate(set => set
                    .SetProperty(user => user.FirstName, propertyDict.GetValueOrEmpty("firstName"))
                    .SetProperty(user => user.MiddleName, propertyDict.GetValueOrEmpty("middleName"))
                    .SetProperty(user => user.LastName, propertyDict.GetValueOrEmpty("lastName"))
                    .SetProperty(user => user.TelephoneNumber, propertyDict.GetValueOrEmpty("telephoneNumber"))
                    .SetProperty(user => user.IsLead, propertyDict.GetValueOrEmpty("isLead") == "True" ? true : false)
                );
        }
    }
}
