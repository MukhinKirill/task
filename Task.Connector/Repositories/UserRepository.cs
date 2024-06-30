using Task.Connector.Extensions;
using Task.Connector.Mappers;
using Task.Connector.Records;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly TaskDbContext _dbContext;
        private readonly UserMapper _mapper;
        private readonly DbProperties _dbProperties;

        public UserRepository(TaskDbContext dbContext, UserMapper mapper, ConfigureManager configureManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _dbProperties = configureManager.DbProperties;
        }

        public void CreateUser(UserToCreate user)
        {
            (var userEntity, var password) = _mapper.Map(user);

            _dbContext.Users.Add(userEntity);
            _dbContext.Passwords.Add(password);

            _dbContext.SaveChanges();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var properties = new List<Property>
            {
                new Property(_dbProperties.UserProperties.FirstNamePropertyName, string.Empty),
                new Property(_dbProperties.UserProperties.MiddleNamePropertyName, string.Empty),
                new Property(_dbProperties.UserProperties.LastNamePropertyName, string.Empty),
                new Property(_dbProperties.UserProperties.TelephoneNumberPropertyName, string.Empty),
                new Property(_dbProperties.UserProperties.IsLeadPropertyName, string.Empty),
                new Property(_dbProperties.PasswordProperties.PasswordPropertyName, string.Empty)
            };

            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var userProperties = new List<UserProperty>();

            var user = _dbContext.Users.Where(user => user.Login == userLogin).FirstOrDefault();

            if(user != null)
            {
                userProperties.Add(new UserProperty(_dbProperties.UserProperties.FirstNamePropertyName, user.FirstName));
                userProperties.Add(new UserProperty(_dbProperties.UserProperties.MiddleNamePropertyName, user.MiddleName));
                userProperties.Add(new UserProperty(_dbProperties.UserProperties.LastNamePropertyName, user.LastName));
                userProperties.Add(new UserProperty(_dbProperties.UserProperties.TelephoneNumberPropertyName, user.TelephoneNumber));
                userProperties.Add(new UserProperty(_dbProperties.UserProperties.IsLeadPropertyName, user.IsLead.ToString()));
            }

            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            return _dbContext.Users.Where(user => user.Login == userLogin).FirstOrDefault() != null;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var propertyDict = properties.ConvertToDict();
            var user = _dbContext.Users
                .Where(user => user.Login == userLogin)
                .FirstOrDefault();

            if(user != null)
            {
                user.FirstName = propertyDict.GetValueOrDefault(_dbProperties.UserProperties.FirstNamePropertyName, user.FirstName);
                user.MiddleName = propertyDict.GetValueOrDefault(_dbProperties.UserProperties.MiddleNamePropertyName, user.MiddleName);
                user.LastName = propertyDict.GetValueOrDefault(_dbProperties.UserProperties.LastNamePropertyName, user.LastName);
                user.TelephoneNumber = propertyDict.GetValueOrDefault(_dbProperties.UserProperties.TelephoneNumberPropertyName, user.TelephoneNumber);
                user.IsLead = propertyDict.GetValueOrDefault(_dbProperties.UserProperties.IsLeadPropertyName, user.IsLead.ToString()).EqualsIgnoreCase(true.ToString()) ? true : false;

                _dbContext.SaveChanges();
            }
        }
    }
}
