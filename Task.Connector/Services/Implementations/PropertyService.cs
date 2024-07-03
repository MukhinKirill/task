using Task.Connector.DAL;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.Implementations
{
    public class PropertyService : IPropertyService
    {
        private readonly ConnectorDbContext _dbContext;
        private readonly ILogger _logger;
        public PropertyService(ConnectorDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IEnumerable<Property> GetAllProperties()
        {
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
        }
    }
}
