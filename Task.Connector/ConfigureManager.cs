using Task.Connector.Entities;
using Task.Connector.Extensions;
using Task.Connector.Parsers.Records;
using Task.Connector.Records;

namespace Task.Connector
{
    public class ConfigureManager
    {
        //User table properties name const
        private const string FirstNamePropertyName = "firstName";
        private const string MiddleNamePropertyName = "middletName";
        private const string LastNamePropertyName = "lastName";
        private const string TelephoneNumberPropertyName = "telephoneNumber";
        private const string IsLeadPropertyName = "isLead";

        //Password table properties name const
        private const string PasswordPropertyName = "password";

        //Permission parser configure const
        private const string RequestRightGroupName = "Request";
        private const string ItRoleRightGroupName = "Role";
        private const string Delimeter = ":";

        public readonly DbProperties DbProperties;
        public readonly PermissionParserConfiguration PermissionParserConfiguration;

        private readonly TaskDbContext _dbContext;

        public ConfigureManager(TaskDbContext dbContext)
        {
            _dbContext = dbContext;

            var userProperties = _dbContext.Users.EntityType.GetProperties();
            var passwordProperties = _dbContext.Passwords.EntityType.GetProperties();

            DbProperties = new DbProperties(
            new UserProperties(
                    userProperties.GetPropertyColumnNameOrDefault(nameof(User.FirstName), FirstNamePropertyName),
                    userProperties.GetPropertyColumnNameOrDefault(nameof(User.MiddleName), MiddleNamePropertyName),
                    userProperties.GetPropertyColumnNameOrDefault(nameof(User.LastName), LastNamePropertyName),
                    userProperties.GetPropertyColumnNameOrDefault(nameof(User.TelephoneNumber), TelephoneNumberPropertyName),
                    userProperties.GetPropertyColumnNameOrDefault(nameof(User.IsLead), IsLeadPropertyName)
                    ),
                new PasswordProperties(
                    passwordProperties.GetPropertyColumnNameOrDefault(nameof(Password.PasswordProperty), PasswordPropertyName)
                    )
                );

            PermissionParserConfiguration = new PermissionParserConfiguration(
                RequestRightGroupName,
                ItRoleRightGroupName,
                Delimeter
                );
        }
    }
}
