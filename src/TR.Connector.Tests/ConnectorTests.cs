using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector.Tests
{
    public class ConnectorTests
    {
        private readonly IConnector _connector;
        private readonly string connectorString = "url=http://localhost:5000;login=login;password=password";

        public ConnectorTests()
        {
            _connector = new Connector();

            _connector.Logger = new ConsoleLogger();
            _connector.StartUp(connectorString);
        }

        [Fact]
        public void GetAllPermissions_Ok()
        {
            var permissions = _connector.GetAllPermissions();
            Assert.NotNull(permissions);

            var ItRole9 = permissions.FirstOrDefault(_ => _.Name == "ITRole9");
            Assert.Equal("ItRole,9", ItRole9.Id);

            var RequestRight5 = permissions.FirstOrDefault(_ => _.Name == "RequestRight5");
            Assert.Equal("RequestRight,5", RequestRight5.Id);
        }

        [Fact]
        public void GetUserPermissions_Ok()
        {
            var login = "Login3";
            var permissions = _connector.GetUserPermissions(login).ToList();

            Assert.NotNull(permissions);
            Assert.NotNull(permissions.FirstOrDefault(_ => _.Contains("ItRole")));
            Assert.NotNull(permissions.FirstOrDefault(_ => _.Contains("RequestRight")));
        }

        /*[Fact]
        public void GetUserPermissions1_Ok()
        {
            var login = "Login4"; //lock

            //Сейчас не пройдет. сервак вернет ошибку "Пользователь Login4 заблокирован".
            var permissions = _connector.GetUserPermissions(login);

            Assert.NotNull(permissions);
            Assert.Empty(permissions);
        }*/

        [Fact]
        public void Add_Drop_Permissions_Ok()
        {
            var login = "Login7";
            var userRole = "ItRole,5";
            var userRight = "RequestRight,5";
            _connector.AddUserPermissions(login, new List<string>(){userRole, userRight});

            var userPermissions = _connector.GetUserPermissions(login).ToList();
            Assert.NotNull(userPermissions.FirstOrDefault(_ => _.Contains(userRole)));
            Assert.NotNull(userPermissions.FirstOrDefault(_ => _.Contains(userRight)));

            _connector.RemoveUserPermissions(login, new List<string>(){userRole, userRight});

            userPermissions = _connector.GetUserPermissions(login).ToList();
            Assert.Null(userPermissions.FirstOrDefault(_ => _.Contains(userRole)));
            Assert.Null(userPermissions.FirstOrDefault(_ => _.Contains(userRight)));
        }

        [Fact]
        public void GetAllProperties_Ok()
        {
            var allProperties = _connector.GetAllProperties();

            Assert.NotNull(allProperties);
            Assert.NotNull(allProperties.FirstOrDefault(_ => _.Name.Contains("isLead")));
        }

        [Fact]
        public void Get_UpdateUserProperties_Ok()
        {
            var login = "Login3";
            var userProperties = _connector.GetUserProperties(login);
            Assert.NotNull(userProperties);

            Assert.Equal("FirstName3", userProperties.FirstOrDefault(_ => _.Name == "firstName").Value);
            Assert.Equal("TelephoneNumber3", userProperties.FirstOrDefault(_ => _.Name == "telephoneNumber").Value);

            var userProps = new List<UserProperty>()
            {
                new UserProperty("firstName", "FirstName13"),
                new UserProperty("telephoneNumber", "TelephoneNumber13"),
            };
            _connector.UpdateUserProperties(userProps, login);


            userProperties = _connector.GetUserProperties(login);
            Assert.NotNull(userProperties);

            Assert.Equal("FirstName13", userProperties.FirstOrDefault(_ => _.Name == "firstName").Value);
            Assert.Equal("TelephoneNumber13", userProperties.FirstOrDefault(_ => _.Name == "telephoneNumber").Value);
        }

        [Fact]
        public void Get_CreateUser_Ok()
        {
            var login = "Login100";

            var isUser = _connector.IsUserExists(login);
            Assert.False(isUser);

            var user = new UserToCreate(login, "Password100")
            {
                Properties = new List<UserProperty>()
                {
                    new UserProperty("firstName", "FirstName100"),
                    new UserProperty("lastName", ""),
                    new UserProperty("middleName", ""),
                    new UserProperty("telephoneNumber", ""),
                    new UserProperty("isLead", ""),
                }
            };

            _connector.CreateUser(user);

            isUser = _connector.IsUserExists(login);
            Assert.True(isUser);
        }
    }
}