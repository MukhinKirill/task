using ApplicationExceptions;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public sealed class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        private const string _providerName = "POSTGRE";
        private const string _connectionString = "Server=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=1";
        private static DbContextFactory _contextFactory = new DbContextFactory(_connectionString);
        private readonly DataContext _dataContext = _contextFactory.GetContext(_providerName);

        public ConnectorDb() { }

        public void StartUp(string connectionString)
        { }

        public void CreateUser(UserToCreate userToCreate)
        {
            var login = userToCreate.Login;

            if (!IsUserExists(login))
            {
                var newUser = new User()
                {
                    FirstName = "Ivan",
                    LastName = "Ivanov",
                    MiddleName = "Ivanovich",
                    TelephoneNumber = "89185062206",
                    IsLead = false,
                    Login = login
                };

                _dataContext.Users.Add(newUser);

                _dataContext.SaveChanges();
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var properties = new Property[]
            {
                new Property("Property_1","Description_1"),
                new Property("Property_2","Description_2"),
                new Property("Property_3","Description_3"),
                new Property("Property_4","Description_4"),
                new Property("Property_5","Description_5"),
                new Property("Property_6","Description_6"),
            };
            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var properties = new UserProperty[]
            {
                new UserProperty("Lead","88005553535"),
                new UserProperty("Lead","88005553535"),
                new UserProperty("Lead","88005553535"),
                new UserProperty("Lead","88005553535"),
                new UserProperty("Lead","88005553535"),
            };

            return properties;
        }

        public bool IsUserExists(string userLogin)
        {
            return _dataContext.Users.FirstOrDefault(user => user.Login == userLogin) != null
                ? true
                : false;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (userLogin == null) throw new UserNotFoundException();

            var user = _dataContext.Users.FirstOrDefault(user => user.Login == userLogin)
                ?? throw new UserNotFoundException();

            user.TelephoneNumber = "88003221337";
            _dataContext.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var permission = new Permission[]
            {
                new Permission("1","Ivan","88005553535"),
                new Permission("2","Ivan","88005553535"),
                new Permission("3","Ivan","88005553535"),
                new Permission("4","Ivan","88005553535"),
                new Permission("5","Ivan","88005553535"),
                new Permission("6","Ivan","88005553535"),
                new Permission("7","Ivan","88005553535"),
                new Permission("8","Ivan","88005553535"),
                new Permission("9","Ivan","88005553535"),
                new Permission("10","Ivan","88005553535"),
                new Permission("11","Ivan","88005553535"),
                new Permission("12","Ivan","88005553535"),
                new Permission("13","Ivan","88005553535"),
                new Permission("14","Ivan","88005553535"),
            };

            return permission;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var role = new UserITRole() { UserId = userLogin, RoleId = 1 };
            _dataContext.UserITRoles.Add(role);
            _dataContext.SaveChanges();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var allRigths = _dataContext.UserRequestRights.ToArray()
                ?? throw new UserRightsNotExistException();

            _dataContext.UserRequestRights.RemoveRange(allRigths);
            _dataContext.SaveChanges();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var permissions = new string[] { "value1", "value2", "value3", "value4", "value5", "value6" };
            return permissions;
        }
    }
}