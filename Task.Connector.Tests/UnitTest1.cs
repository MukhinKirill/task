using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Tests
{
    public class UnitTest1
    {
        private const string _requestRightGroupName = "Request";
        private const string _itRoleRightGroupName = "Role";
        private const string _delimeter = ":";
        private const string _mssqlConnectionString = "Server=localhost;Database=testDb;User Id=sa;Password=!PassWord#;TrustServerCertificate=True;";
        private const string _postgreConnectionString = "Server=127.0.0.1;Port=5432;Database=testDb;User ID=postgres;Password=!PassWord#";
        private readonly static Dictionary<string, string> _connectorsCS = new ()
        {
            { "MSSQL",$"ConnectionString='{_mssqlConnectionString}';Provider='SqlServer.2019';SchemaName='AvanpostIntegrationTestTaskSchema';"},
            { "POSTGRE", $"ConnectionString='{_postgreConnectionString}';Provider='PostgreSQL.9.5';SchemaName='AvanpostIntegrationTestTaskSchema';"}
        };
        private readonly static Dictionary<string, string> _dataBasesCS = new()
        {
            { "MSSQL",_mssqlConnectionString},
            { "POSTGRE", _postgreConnectionString}
        };

        private static DataManager Init(string providerName)
        {
            var factory = new DbContextFactory(_dataBasesCS[providerName]);
            var dataSetter = new DataManager(factory, providerName);
            dataSetter.PrepareDbForTest();
            return dataSetter;
        }

        private static IConnector GetConnector(string provider)
        {
            IConnector connector = new ConnectorDb() { Logger = new FileLogger($"{DateTime.UtcNow.ToShortDateString()}connector{provider}.Log", $"{DateTime.UtcNow}connector{provider}")};
            connector.StartUp(_connectorsCS[provider]);
            return connector;
        }


        [Theory]
        [InlineData("MSSQL")]
        [InlineData("POSTGRE")]
        public void CreateUser(string provider)
        {
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            connector.CreateUser(new UserToCreate("testUserToCreate", "testPassword") { Properties = new UserProperty[] { 
                new ("isLead", "false"), 
                new("firstName", "Ivan"), 
                new ("lastName", "Ivanov"), 
                new ("middleName", "Ivanovich"),
                new ("telephoneNumber", "999")
            } });
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
            Assert.Equal(DefaultData.PropsCount+1/*password too*/, propInfos.Count());
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
            Assert.Equal(5, userInfo.Count());
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
                new (propertyName,TestData.NewPhoneValueForMasterUser)
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
            var RoleId = $"{_itRoleRightGroupName}{_delimeter}{dataSetter.GetITRoleId()}";
            connector.AddUserPermissions(
                DefaultData.MasterUserLogin,
                new [] { RoleId });
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
            var requestRightIdToDrop = $"{_requestRightGroupName}{_delimeter}{dataSetter.GetRequestRightId(DefaultData.RequestRights[DefaultData.MasterUserRequestRights.First()].Name)}";
            connector.RemoveUserPermissions(
                DefaultData.MasterUserLogin,
                new [] { requestRightIdToDrop });
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