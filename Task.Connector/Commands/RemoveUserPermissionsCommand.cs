using Dapper;
using System.Data;
using Task.Connector.Helpers;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;

namespace Task.Connector.Commands
{
    internal class RemoveUserPermissionsCommand
    {
        private static readonly string removeRolesQuery = $@"DELETE FROM {UserItRoleScheme.TableName} 
                WHERE {UserItRoleScheme.roleId} = @roleId AND {UserItRoleScheme.userId} = @userId";

        private static readonly string removeRightsQuery = $@"DELETE FROM {UserRequestRightScheme.TableName} 
                WHERE {UserRequestRightScheme.rightId} = @rightId AND {UserRequestRightScheme.userId} = @userId";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public RemoveUserPermissionsCommand(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public void Execute(string userLogin, IEnumerable<string> rightIds)
        {
            var (roles, rights) = PermissionParser.Parse(rightIds);

            using var transaction = _dbConnection.BeginTransaction();

            foreach (var id in roles)
            {
                _dbConnection.Execute(removeRolesQuery, new { roleId = id, userId = userLogin }, transaction);
            }

            foreach (var id in rights)
            {
                _dbConnection.Execute(removeRightsQuery, new { rightId = id, userId = userLogin }, transaction);
            }

            transaction.Commit();
        }
    }
}
