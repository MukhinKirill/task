using Dapper;
using System.Data;
using Task.Connector.Models;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Commands
{
    internal class GetUserPropertiesQuery
    {
        private static readonly string query = $@"SELECT
            {UserScheme.firstName} AS {nameof(DbUser.FirstName)},
            {UserScheme.lastName} AS {nameof(DbUser.LastName)},
            {UserScheme.middleName} AS {nameof(DbUser.MiddleName)},
            {UserScheme.telephoneNumber} AS {nameof(DbUser.TelephoneNumber)},
            {UserScheme.isLead} AS {nameof(DbUser.IsLead)}
            FROM {UserScheme.TableName}
            WHERE login = @userLogin";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public GetUserPropertiesQuery(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public IEnumerable<UserProperty> Execute(string userLogin)
        {
            var user = _dbConnection.QuerySingle<DbUser>(query, new { userLogin });
            return user.GetProperties();
        }
    }
}
