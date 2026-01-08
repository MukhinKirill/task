using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector.Tests
{
    public class ConnectorTests : IAsyncLifetime
    {
        private readonly IConnector _connector;
        private readonly string _connectorString = "url=http://localhost:5000;login=login;password=password";

        public ConnectorTests()
        {
            _connector = new Connector();
            _connector.Logger = new ConsoleLogger();
        }

        public async Task InitializeAsync()
        {
            await _connector.StartUpAsync(_connectorString);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task GetAllPermissions_Ok()
        {
            var permissions = await _connector.GetAllPermissionsAsync();
            Assert.NotNull(permissions);

            var ItRole9 = permissions.FirstOrDefault(_ => _.Name == "ITRole9");
            Assert.Equal("ItRole,9", ItRole9?.Id);

            var RequestRight5 = permissions.FirstOrDefault(_ => _.Name == "RequestRight5");
            Assert.Equal("RequestRight,5", RequestRight5?.Id);
        }

        [Fact]
        public async Task GetUserPermissions_Ok()
        {
            var login = "Login3";
            var permissions = (await _connector.GetUserPermissionsAsync(login)).ToList();

            Assert.NotNull(permissions);
            Assert.NotNull(permissions.FirstOrDefault(_ => _.Contains("ItRole")));
            Assert.NotNull(permissions.FirstOrDefault(_ => _.Contains("RequestRight")));
        }

        [Fact]
        public async Task Add_Drop_Permissions_Ok()
        {
            var login = "Login7";
            var userRole = "ItRole,5";
            var userRight = "RequestRight,5";
            await _connector.AddUserPermissionsAsync(login, new List<string>() { userRole, userRight });

            var userPermissions = (await _connector.GetUserPermissionsAsync(login)).ToList();
            Assert.NotNull(userPermissions.FirstOrDefault(_ => _.Contains(userRole)));
            Assert.NotNull(userPermissions.FirstOrDefault(_ => _.Contains(userRight)));

            await _connector.RemoveUserPermissionsAsync(login, new List<string>() { userRole, userRight });

            userPermissions = (await _connector.GetUserPermissionsAsync(login)).ToList();
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
        public async Task Get_UpdateUserProperties_Ok()
        {
            var login = "Login3";
            var userProperties = await _connector.GetUserPropertiesAsync(login);
            Assert.NotNull(userProperties);

            Assert.Equal("FirstName3", userProperties.FirstOrDefault(_ => _.Name == "firstName")?.Value);
            Assert.Equal("TelephoneNumber3", userProperties.FirstOrDefault(_ => _.Name == "telephoneNumber")?.Value);

            var userProps = new List<UserProperty>()
            {
                new UserProperty("firstName", "FirstName13"),
                new UserProperty("telephoneNumber", "TelephoneNumber13"),
            };
            await _connector.UpdateUserPropertiesAsync(userProps, login);


            userProperties = await _connector.GetUserPropertiesAsync(login);
            Assert.NotNull(userProperties);

            Assert.Equal("FirstName13", userProperties.FirstOrDefault(_ => _.Name == "firstName")?.Value);
            Assert.Equal("TelephoneNumber13", userProperties.FirstOrDefault(_ => _.Name == "telephoneNumber")?.Value);
        }

        [Fact]
        public async Task Get_CreateUser_Ok()
        {
            var login = "Login100";

            var isUser = await _connector.IsUserExistsAsync(login);
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

            await _connector.CreateUserAsync(user);

            isUser = await _connector.IsUserExistsAsync(login);
            Assert.True(isUser);
        }
    }
}
