using Dapper;
using System.Data;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;

namespace Task.Connector.Commands
{
    internal class GetUserPermissionsQuery
    {
        private static readonly string Query = $@"SELECT
                {UserRequestRightScheme.rightId} AS permissionId
                FROM {UserRequestRightScheme.TableName}
                WHERE {UserRequestRightScheme.userId} = @userLogin
                UNION ALL SELECT
                {UserItRoleScheme.roleId} AS permissionId
                FROM {UserItRoleScheme.TableName}
                WHERE {UserItRoleScheme.userId} = @userLogin";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public GetUserPermissionsQuery(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public IEnumerable<string> Execute(string userLogin)
        {
            return _dbConnection.Query<dynamic>(Query, new { userLogin }).Select(x => (string)x.permissionId);
        }
    }
}
