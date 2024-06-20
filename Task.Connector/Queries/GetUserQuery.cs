using Dapper;
using System.Data;
using Task.Connector.Models;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;

namespace Task.Connector.Commands
{
    internal class GetUserQuery
    {
        private static readonly string query = $@"SELECT 
            {UserScheme.login} AS {nameof(DbUser.Login)}, 
            {UserScheme.lastName} AS {nameof(DbUser.LastName)}, 
            {UserScheme.firstName} AS {nameof(DbUser.FirstName)}, 
            {UserScheme.middleName} AS {nameof(DbUser.MiddleName)}, 
            {UserScheme.telephoneNumber} AS  {nameof(DbUser.TelephoneNumber)}, 
            {UserScheme.isLead} AS {nameof(DbUser.IsLead)}
            FROM {UserScheme.TableName}
            WHERE {UserScheme.login} = @userLogin";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public GetUserQuery(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public DbUser Execute(string userLogin)
        {
            return _dbConnection.QuerySingle<DbUser>(query, new { userLogin });
        }
    }
}
