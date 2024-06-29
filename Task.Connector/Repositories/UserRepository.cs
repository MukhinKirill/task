using Task.Connector.Entities;
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
            var userEntityProperties = _dbContext.Users.EntityType.GetProperties();
            var passwordEntityProperties = _dbContext.Passwords.EntityType.GetProperties();

            var properties = new List<Property>
            {
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.LastName), Constants.LastNamePropertyName), string.Empty),
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.MiddleName), Constants.MiddleNamePropertyName), string.Empty),
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.FirstName), Constants.FirstNamePropertyName), string.Empty),
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.TelephoneNumber), Constants.TelephoneNumberPropertyName), string.Empty),
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.IsLead), Constants.IsLeadPropertyName), string.Empty),
                new Property(passwordEntityProperties.GetPropertyColumnNameOrDefault(nameof(Password.Password1), Constants.PasswordPropertyName), string.Empty)
            };

            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var userProperties = new List<UserProperty>();
            var userEntityProperties = _dbContext.Users.EntityType.GetProperties();

            var user = _dbContext.Users.Where(user => user.Login == userLogin).FirstOrDefault();

            if(user != null)
            {
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.LastName), Constants.LastNamePropertyName), user.LastName));
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.FirstName), Constants.FirstNamePropertyName), user.FirstName));
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.MiddleName), Constants.MiddleNamePropertyName), user.MiddleName));
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.TelephoneNumber), Constants.TelephoneNumberPropertyName), user.TelephoneNumber));
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.IsLead), Constants.IsLeadPropertyName), user.IsLead.ToString()));
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
            var userEntityProperties = _dbContext.Users.EntityType.GetProperties();
            var user = _dbContext.Users
                .Where(user => user.Login == userLogin)
                .FirstOrDefault();

            if(user != null)
            {
                user.FirstName = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.LastName), Constants.LastNamePropertyName), user.FirstName);
                user.MiddleName = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.FirstName), Constants.FirstNamePropertyName), user.MiddleName);
                user.LastName = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.MiddleName), Constants.MiddleNamePropertyName), user.LastName);
                user.TelephoneNumber = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.TelephoneNumber), Constants.TelephoneNumberPropertyName), user.TelephoneNumber);
                user.IsLead = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.IsLead), Constants.IsLeadPropertyName), user.IsLead.ToString())
                    .EqualsIgnoreCase(true.ToString()) ? true : false;

                _dbContext.SaveChanges();
            }
        }
    }
}
