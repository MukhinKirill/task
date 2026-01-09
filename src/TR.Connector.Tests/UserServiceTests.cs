// TR.Connector.Tests/UserPropertiesTests.cs
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Entities.DTO.Requests;
using Xunit;

namespace TR.Connector.Tests
{
    public class UserPropertiesTests : TestBase
    {
        [Fact]
        public Task GetAllProperties_Ok()
        {
            var allProperties = _connector.GetAllProperties();

            Assert.NotNull(allProperties);
            Assert.NotNull(allProperties.FirstOrDefault(_ => _.Name.Contains("isLead")));

            return Task.CompletedTask;
        }

        [Fact]
        public async Task Get_UpdateUserProperties_Ok()
        {
            var login = "Login1";

            var userProperties = (await _connector.GetUserPropertiesAsync(login)).ToList();

            Assert.NotNull(userProperties);
            Assert.Equal("FirstName1", GetPropertyValue(userProperties, "firstName"));
            Assert.Equal("TelephoneNumber1", GetPropertyValue(userProperties, "telephoneNumber"));

            var userProps = new List<UserProperty>()
            {
                new UserProperty("firstName", "FirstName13"),
                new UserProperty("telephoneNumber", "TelephoneNumber13"),
            };
            await _connector.UpdateUserPropertiesAsync(userProps, login);

            userProperties = (await _connector.GetUserPropertiesAsync(login)).ToList();

            Assert.NotNull(userProperties);
            Assert.Equal("FirstName13", GetPropertyValue(userProperties, "firstName"));
            Assert.Equal("TelephoneNumber13", GetPropertyValue(userProperties, "telephoneNumber"));
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

        [Fact]
        public async Task Put_LockUser_UserLocked()
        {
            var login = "Login3";

            var userProperties = (await _connector.GetUserPropertiesAsync(login, CancellationToken.None)).ToList();
            Assert.NotNull(userProperties);
            Assert.Equal("Unlock", GetPropertyValue(userProperties, "status"));

            await _connector.LockUserAsync(login);

            userProperties = (await _connector.GetUserPropertiesAsync(login, CancellationToken.None)).ToList();
            Assert.Equal("Lock", GetPropertyValue(userProperties, "status"));
        }

        [Fact]
        public async Task Put_UnlockUser_UserUnlocked()
        {
            var login = "Login8";

            var userProperties = (await _connector.GetUserPropertiesAsync(login, CancellationToken.None)).ToList();
            Assert.NotNull(userProperties);
            Assert.Equal("Lock", GetPropertyValue(userProperties, "status"));

            await _connector.UnlockUserAsync(login);

            userProperties = (await _connector.GetUserPropertiesAsync(login, CancellationToken.None)).ToList();
            Assert.Equal("Unlock", GetPropertyValue(userProperties, "status"));
        }
    }
}
