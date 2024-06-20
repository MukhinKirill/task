using Dapper;
using System.Data;
using Task.Connector.Helpers;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;

namespace Task.Connector.Commands
{
    internal class AddUserPermissionsCommand
    {
        private static readonly string getRolesQuery = $@"SELECT
                {UserItRoleScheme.roleId} AS id
                FROM {UserItRoleScheme.TableName}
                WHERE {UserItRoleScheme.userId} = @userId";

        private static readonly string getRightsQuery = $@"SELECT
                {UserRequestRightScheme.rightId} AS id
                FROM {UserRequestRightScheme.TableName}
                WHERE {UserRequestRightScheme.userId} = @userId";

        private static readonly string addRolesQuery = $@"INSERT INTO {UserItRoleScheme.TableName} 
                ({UserItRoleScheme.roleId}, {UserItRoleScheme.userId})
                VALUES 
                (@roleId, @userId)";

        private static readonly string addRightsQuery = $@"INSERT INTO {UserRequestRightScheme.TableName} 
                ({UserRequestRightScheme.rightId}, {UserRequestRightScheme.userId})
                VALUES 
                (@rightId, @userId)";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public AddUserPermissionsCommand(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public void Execute(string userLogin, IEnumerable<string> rightIds)
        {
            var (roles, rights) = PermissionParser.Parse(rightIds);

            using var transaction = _dbConnection.BeginTransaction();

            if (roles.Count > 0)
            {
                var existingRoles = _dbConnection.Query<dynamic>(getRolesQuery, new { userId = userLogin }, transaction).Select(x => (int)x.id);
                var newRoles = roles.Except(existingRoles).ToArray();

                if (newRoles.Length == 0) 
                {
                    _logger.Warn($"Trying to add existing roles to a user '{userLogin}'");
                }

                foreach (var role in newRoles)
                {
                    _dbConnection.Execute(addRolesQuery, new { roleId = role, userId = userLogin }, transaction);
                }
            }
            
            if (rights.Count > 0)
            {
                var existingRights = _dbConnection.Query<dynamic>(getRightsQuery, new { userId = userLogin }, transaction).Select(x => (int)x.id);
                var newRights = rights.Except(existingRights).ToArray();
                foreach (var right in newRights)
                {
                    _dbConnection.Execute(addRightsQuery, new { rightId = right, userId = userLogin }, transaction);
                }

                if (newRights.Length == 0)
                {
                    _logger.Warn($"Trying to add existing roles to a user '{userLogin}'");
                }
            }

            transaction.Commit();
        }
    }
}
