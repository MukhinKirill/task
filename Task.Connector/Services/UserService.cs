using Task.Connector.Repositories;
using Task.Connector.Repositories.Interfaces;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services
{
    public class UserService : IUserService
    {
        private const string LastNameProperty = "LastName";
        private const string FirstNameProperty = "FirstName";
        private const string MiddleNameProperty = "MiddleName";
        private const string TelephoneNumberProperty = "TelephoneNumber";
        private const string IsLeadProperty = "IsLead";

        private readonly IUserRepository _userRepository;

        public UserService(string connectionString)
        {
            _userRepository = new UserRepository(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            var newUser = new User
            {
                Login = user.Login,
                LastName = string.Empty,
                FirstName = string.Empty,
                MiddleName = string.Empty,
                TelephoneNumber = string.Empty,
                IsLead = false
            };

            _userRepository.CreateUser(newUser);
        }

        public bool IsUserExists(string userLogin)
        {
            return _userRepository.IsUserExists(userLogin);
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return typeof(User).GetProperties()
                .Select(x => new Property(x.Name, x.PropertyType.Name));
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _userRepository.GetUserByLogin(userLogin);

            var userProperties = user
                .GetType()
                .GetProperties()
                .Where(prop => prop.Name != nameof(User.Login))
                .Select(x => new UserProperty(x.Name, x.GetValue(user)?.ToString()))
                .ToList();

            return userProperties;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _userRepository.GetUserByLogin(userLogin);

            foreach (var property in properties)
            {
                switch (property.Name)
                {
                    case LastNameProperty:
                        user.LastName = property.Value;
                        break;
                    case FirstNameProperty:
                        user.FirstName = property.Value;
                        break;
                    case MiddleNameProperty:
                        user.MiddleName = property.Value;
                        break;
                    case TelephoneNumberProperty:
                        user.TelephoneNumber = property.Value;
                        break;
                    case IsLeadProperty:
                        user.IsLead = bool.Parse(property.Value);
                        break;
                }
            }

            _userRepository.UpdateUserProperties(user);
        }
    }
}
