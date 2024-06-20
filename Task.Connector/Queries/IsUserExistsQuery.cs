using Dapper;
using System.Data;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;

namespace Task.Connector.Commands
{
    internal class IsUserExistsQuery
    {
        private static readonly string query = $@"SELECT COUNT(1)
            FROM {UserScheme.TableName}
            WHERE {UserScheme.login} = @userLogin";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public IsUserExistsQuery(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public bool Execute(string userLogin)
        {
            return _dbConnection.ExecuteScalar<bool>(query, new { userLogin });
        }
    }
}
