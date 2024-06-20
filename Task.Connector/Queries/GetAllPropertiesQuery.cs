using Dapper;
using System.Data;
using System.Reflection;
using Task.Connector.ClientSchemes;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Commands
{
    internal class GetAllPropertiesQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public GetAllPropertiesQuery(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public IEnumerable<Property> Execute()
        {
            return ClientUserScheme.PropertyFields.Select(p => new Property(p, string.Empty)).ToArray();
        }
    }
}
