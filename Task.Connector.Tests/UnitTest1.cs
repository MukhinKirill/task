using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Tests
{
    public class UnitTest1
    {
        static string requestRightGroupName = "Request";
        static string itRoleRightGroupName = "Role";
        static string delimiter = ":";
        static string mssqlConnectionString = "Server=127.0.0.1,11433;Database=testDb;User Id=sa;Password=813G76v!T71v8;TrustServerCertificate=True;";
        static string postgreConnectionString = "Server=127.0.0.1;Port=15432;Database=testDb;Username=testUser;Password=813G76v!T71v8;";
        static Dictionary<string, string> connectorsCS = new Dictionary<string, string>
        {
            { "MSSQL",$"ConnectionString='{mssqlConnectionString}';Provider='SqlServer.2019';SchemaName='AvanpostIntegrationTestTaskSchema';"},
            { "POSTGRE", $"ConnectionString='{postgreConnectionString}';Provider='PostgreSQL.9.5';SchemaName='AvanpostIntegrationTestTaskSchema';"}
        };
        static Dictionary<string, string> dataBasesCS = new Dictionary<string, string>
        {
            { "MSSQL",mssqlConnectionString},
            { "POSTGRE", postgreConnectionString}
        };

        public DataManager Init(string providerName)
        {
            var factory = new DbContextFactory(dataBasesCS[providerName]);
            var dataSetter = new DataManager(factory, providerName);
            dataSetter.PrepareDbForTest();
            return dataSetter;
        }

        public IMyConnector GetConnector(string provider)
        {
            IMyConnector connector = new ConnectorDb();
            connector.StartUp(connectorsCS[provider]);
            connector.Logger = new FileLogger($"{DateTime.Now.ToString("yyyy-MM-ddTH_mm_ss")}connector{provider}.Log", $"{DateTime.Now}connector{provider}");
            return connector;
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void CreateUser(string provider)
        {
            var dataSetter = Init(provider);
            using var connector = GetConnector(provider);
            connector.CreateUser(new UserToCreate("testUserToCreate", "testPassword") { Properties = new UserProperty[] { new UserProperty("isLead", "false") } });
            Assert.NotNull(dataSetter.GetUser("testUserToCreate"));
            Assert.Equal(DefaultData.MasterUserDefaultPassword, dataSetter.GetUserPassword(DefaultData.MasterUserLogin));
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void GetAllProperties(string provider)
        {
            Init(provider);
            using var connector = GetConnector(provider);
            var propInfos = connector.GetAllProperties();
            Assert.Equal(DefaultData.PropsCount + 1/*password too*/, propInfos.Count());
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void GetUserProperties(string provider)
        {
            Init(provider);
            using var connector = GetConnector(provider);
            var userInfo = connector.GetUserProperties(DefaultData.MasterUserLogin).ToList();
            Assert.NotNull(userInfo);
            Assert.Equal(5, userInfo.Count);
            Assert.True(userInfo.FirstOrDefault(u => u.Value.Equals(DefaultData.MasterUser.TelephoneNumber)) != null);
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void IsUserExists(string provider)
        {
            Init(provider);
            using var connector = GetConnector(provider);
            Assert.True(connector.IsUserExists(DefaultData.MasterUserLogin));
            Assert.False(connector.IsUserExists(TestData.NotExistingUserLogin));
        }

        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void UpdateUserProperties(string provider)
        {
            var dataSetter = Init(provider);
            using var connector = GetConnector(provider);
            connector.GetUserProperties(DefaultData.MasterUserLogin);
            var propertyName = connector.GetUserProperties(DefaultData.MasterUserLogin).First(p => p.Value.Equals(DefaultData.MasterUser.TelephoneNumber)).Name;
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
            Init(provider);
            using var connector = GetConnector(provider);
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
            using var connector = GetConnector(provider);
            var RoleId = $"{itRoleRightGroupName}{delimiter}{dataSetter.GetITRoleId()}";
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
            using var connector = GetConnector(provider);
            var requestRightIdToDrop = $"{requestRightGroupName}{delimiter}{dataSetter.GetRequestRightId(DefaultData.RequestRights[DefaultData.MasterUserRequestRights.First()].Name)}";
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
            using var connector = GetConnector(provider);
            var permissions = connector.GetUserPermissions(DefaultData.MasterUserLogin);
            Assert.Equal(DefaultData.MasterUserRequestRights.Length, permissions.Count());
        }
    }
}