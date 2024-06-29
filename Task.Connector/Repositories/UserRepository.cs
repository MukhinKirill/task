using Task.Connector.Entities;
using Task.Connector.Extensions;
using Task.Connector.Mappers;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private const string FirstNamePropertyName = "firstName";
        private const string MiddleNamePropertyName = "middletName";
        private const string LastNamePropertyName = "lastName";
        private const string TelephoneNumberPropertyName = "telephoneNumber";
        private const string IsLeadPropertyName = "isLead";
        private const string PasswordPropertyName = "password";

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
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.LastName), LastNamePropertyName), string.Empty),
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.MiddleName), MiddleNamePropertyName), string.Empty),
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.FirstName), FirstNamePropertyName), string.Empty),
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.TelephoneNumber), TelephoneNumberPropertyName), string.Empty),
                new Property(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(User.IsLead), IsLeadPropertyName), string.Empty),
                new Property(passwordEntityProperties.GetPropertyColumnNameOrDefault(nameof(Password.Password1), PasswordPropertyName), string.Empty)
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
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.LastName), LastNamePropertyName), user.LastName));
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.FirstName), FirstNamePropertyName), user.FirstName));
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.MiddleName), MiddleNamePropertyName), user.MiddleName));
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.TelephoneNumber), TelephoneNumberPropertyName), user.TelephoneNumber));
                userProperties.Add(new UserProperty(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.IsLead), IsLeadPropertyName), user.IsLead.ToString()));
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
                user.FirstName = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.LastName), LastNamePropertyName), user.FirstName);
                user.MiddleName = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.FirstName), FirstNamePropertyName), user.MiddleName);
                user.LastName = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.MiddleName), MiddleNamePropertyName), user.LastName);
                user.TelephoneNumber = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.TelephoneNumber), TelephoneNumberPropertyName), user.TelephoneNumber);
                user.IsLead = propertyDict.GetValueOrDefault(userEntityProperties.GetPropertyColumnNameOrDefault(nameof(user.IsLead), IsLeadPropertyName), user.IsLead.ToString())
                    .EqualsIgnoreCase(true.ToString()) ? true : false;

                _dbContext.SaveChanges();
            }
        }
    }
}
