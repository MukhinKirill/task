using Dapper;
using System.Data;
using System.Data.Common;
using Task.Connector.Helpers;
using Task.Connector.Models;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Commands
{
    internal class GetAllPermissionsQuery
    {
        private static readonly string query = $@"SELECT
                {RequestRightScheme.name} AS {nameof(Permission.Name)},
                {RequestRightScheme.id} AS {nameof(Permission.Id)}
                FROM {RequestRightScheme.TableName} 
                UNION ALL SELECT
                {ItRoleScheme.name} AS {nameof(Permission.Name)},
                {ItRoleScheme.id} AS {nameof(Permission.Id)}
                FROM {ItRoleScheme.TableName}";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public GetAllPermissionsQuery(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public IEnumerable<Permission> Execute()
        {
            return _dbConnection.Query<dynamic>(query)
                .Select(x => new Permission(x.id.ToString(), x.name, string.Empty));
        }
    }
}
