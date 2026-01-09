// TR.Connector.Tests/PermissionTests.cs
using TR.Connectors.Api.Entities;
using Xunit;

namespace TR.Connector.Tests
{
    public class PermissionTests : TestBase
    {
        [Fact]
        public async Task GetAllPermissions_Ok()
        {
            var permissions = (await _connector.GetAllPermissionsAsync()).ToList();

            Assert.NotNull(permissions);

            var itRole9 = permissions.FirstOrDefault(_ => _.Name == "ITRole9");
            Assert.Equal("ItRole,9", itRole9!.Id);

            var requestRight5 = permissions.FirstOrDefault(_ => _.Name == "RequestRight5");
            Assert.Equal("RequestRight,5", requestRight5!.Id);
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
            var login = "Login1";
            var userRole = "ItRole,5";
            var userRight = "RequestRight,5";

            await _connector.AddUserPermissionsAsync(login, new List<string>() { userRole, userRight });

            var userPermissions = (await _connector.GetUserPermissionsAsync(login)).ToList();
            Assert.NotNull(userPermissions.FirstOrDefault(r => r.Contains(userRole)));
            Assert.NotNull(userPermissions.FirstOrDefault(r => r.Contains(userRight)));

            await _connector.RemoveUserPermissionsAsync(login, new List<string>() { userRole, userRight });

            userPermissions = (await _connector.GetUserPermissionsAsync(login)).ToList();
            Assert.Null(userPermissions.FirstOrDefault(_ => _.Contains(userRole)));
            Assert.Null(userPermissions.FirstOrDefault(_ => _.Contains(userRight)));
        }
    }
}
