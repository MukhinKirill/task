using Dapper;
using System.Data;
using Task.Connector.Models;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Commands
{
    internal class UpdateUserPropertiesCommand
    {
        private static readonly string updateUserQuery = $@"UPDATE {UserScheme.TableName} SET
            {UserScheme.lastName} = @{nameof(DbUser.LastName)}, 
            {UserScheme.firstName} = @{nameof(DbUser.FirstName)}, 
            {UserScheme.middleName} = @{nameof(DbUser.MiddleName)}, 
            {UserScheme.telephoneNumber} = @{nameof(DbUser.TelephoneNumber)}, 
            {UserScheme.isLead} = @{nameof(DbUser.IsLead)}
            WHERE {UserScheme.login} = @{nameof(DbUser.Login)}";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public UpdateUserPropertiesCommand(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public void Execute(IEnumerable<UserProperty> properties, string userLogin)
        {
            var userQuery = new GetUserQuery(_dbConnection, _logger);
            DbUser user;
            try
            {
                user = userQuery.Execute(userLogin);
            }
            catch (Exception) 
            {
                _logger.Error($"Failed to update non-existing user {userLogin}");
                return;
            }

            user.SetProperties(properties);
            _dbConnection.Execute(updateUserQuery, user);
        }
    }
}
