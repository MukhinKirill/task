using Task.Connector.Connectors;
using Task.Connector.Constants;
using Task.Connector.Tests.Constants;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;


namespace Task.Connector.Tests
{
    public class DatabaseTests : IClassFixture<DatabaseFixture>
    {
        readonly string mssqlConnectionString;
        readonly string postgreConnectionString;
        readonly Dictionary<string, string> connectorsCS;
        readonly Dictionary<string, string> dataBasesCS;

        public DatabaseTests(DatabaseFixture dbFixture)
        {
            mssqlConnectionString = ""/*dbFixture.MsSqlContainer.GetConnectionString()*/;
            postgreConnectionString = dbFixture.PostgreSqlContainer.GetConnectionString();
            connectorsCS = new Dictionary<string, string>
            {
                { DatabaseConnectors.MSSQL_PROVIDER, DatabaseConnectors.GetMssqlConfiguration(mssqlConnectionString) },
                { DatabaseConnectors.POSGRE_PROVIDER,  DatabaseConnectors.GetPostgresConfiguration(postgreConnectionString)}
            };

            dataBasesCS = new Dictionary<string, string>
            {
                { DatabaseConnectors.MSSQL_PROVIDER, mssqlConnectionString},
                { DatabaseConnectors.POSGRE_PROVIDER, postgreConnectionString}
            };
        }
        
        public DataManager Init(string providerName)
        {
            var factory = new DbContextFactory(dataBasesCS[providerName]);
            var dataSetter = new DataManager(factory, providerName);
            dataSetter.PrepareDbForTest();
            return dataSetter;
        }

        public IConnector GetConnector(string provider)
        {
            IConnector connector = new ConnectorDb();
            connector.StartUp(connectorsCS[provider]);
            connector.Logger = new FileLogger($"{DateTime.Now}connector{provider}.Log", $"{DateTime.Now}connector{provider}");
            return connector;
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void CreateUser(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            connector.CreateUser(new UserToCreate("testUserToCreate", "testPassword") { Properties = new UserProperty[] { new UserProperty("isLead", "false") } });
            Assert.NotNull(dataSetter.GetUser("testUserToCreate"));
            Assert.Equal(DefaultData.MasterUserDefaultPassword, dataSetter.GetUserPassword(DefaultData.MasterUserLogin));
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void GetAllProperties(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var propInfos = connector.GetAllProperties();
            Assert.Equal(DefaultData.PropsCount + 1/*password too*/, propInfos.Count());
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void GetUserProperties(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var userInfo = connector.GetUserProperties(DefaultData.MasterUserLogin);
            Assert.NotNull(userInfo);
            Assert.Equal(6, userInfo.Count());
            Assert.True(userInfo.FirstOrDefault(_ => _.Value.Equals(DefaultData.MasterUser.TelephoneNumber)) != null);
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void IsUserExists(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            Assert.True(connector.IsUserExists(DefaultData.MasterUserLogin));
            Assert.False(connector.IsUserExists(TestData.NotExistingUserLogin));
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void UpdateUserProperties(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var userInfo = connector.GetUserProperties(DefaultData.MasterUserLogin);
            var propertyName = connector.GetUserProperties(DefaultData.MasterUserLogin).First(_ => _.Value.Equals(DefaultData.MasterUser.TelephoneNumber)).Name;
            var propsToUpdate = new UserProperty[]
            {
                new UserProperty(propertyName,TestData.NewPhoneValueForMasterUser)
            };
            connector.UpdateUserProperties(propsToUpdate, DefaultData.MasterUserLogin);
            Assert.Equal(TestData.NewPhoneValueForMasterUser, dataSetter.GetUser(DefaultData.MasterUserLogin).TelephoneNumber);
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void GetAllPermissions(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var permissions = connector.GetAllPermissions();
            Assert.NotNull(permissions);
            Assert.Equal(DefaultData.RequestRights.Length + DefaultData.ITRoles.Length, permissions.Count());
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void AddUserPermissions(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var RoleId = $"{RightConstants.IT_ROLE_RIGHT_GROUP_NAME}{RightConstants.DELIMETER}{dataSetter.GetITRoleId()}";
            connector.AddUserPermissions(
                DefaultData.MasterUserLogin,
                new[] { RoleId });
            Assert.True(dataSetter.MasterUserHasITRole(dataSetter.GetITRoleId().ToString()));
            Assert.True(dataSetter.MasterUserHasRequestRight(dataSetter.GetRequestRightId(DefaultData.RequestRights[DefaultData.MasterUserRequestRights.First()].Name).ToString()));
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void RemoveUserPermissions(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var requestRightIdToDrop = $"{RightConstants.REQUEST_RIGHT_GROUP_NAME}{RightConstants.DELIMETER}{dataSetter.GetRequestRightId(DefaultData.RequestRights[DefaultData.MasterUserRequestRights.First()].Name)}";
            connector.RemoveUserPermissions(
                DefaultData.MasterUserLogin,
                new[] { requestRightIdToDrop });
            Assert.False(dataSetter.MasterUserHasITRole(dataSetter.GetITRoleId().ToString()));
            Assert.False(dataSetter.MasterUserHasRequestRight(dataSetter.GetRequestRightId(DefaultData.RequestRights[DefaultData.MasterUserRequestRights.First()].Name).ToString()));
        }
        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void GetUserPermissions(string provider)
        {
            Init(provider);
            var connector = GetConnector(provider);
            var permissions = connector.GetUserPermissions(DefaultData.MasterUserLogin);
            Assert.Equal(DefaultData.MasterUserRequestRights.Length, permissions.Count());
        }
    }
}