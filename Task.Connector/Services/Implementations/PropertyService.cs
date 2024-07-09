using System.Reflection;
using Task.Connector.DAL;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.DbCommon.DbModels;
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
            var propertyList = new List<Property>();
            var properties = _dbContext.Users.First().GetType().GetProperties();
            //var properties = new User().GetType().GetProperties(); а можно так
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    propertyList.Add(new Property(property.Name, property.PropertyType.ToString()));
                }
            }
            return propertyList;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var userPropertyList = new List<UserProperty>();
            var user = new User();
            try
            {
                user = _dbContext.Users.FirstOrDefault(u => u.Login == userLogin);
            }
            catch (Exception ex)
            {
                _logger.Error($"DB error {ex.Message}");
            }
            if (user != null)
            {
                var userProperties = user.GetType().GetProperties();
                foreach (var property in userProperties)
                {
                    if (!property.Name.Equals("Login")) //Login  свойством не считаем
                    {
                        userPropertyList.Add(new UserProperty(property.Name, property.GetValue(user).ToString()));
                    }
                }
            }
            return userPropertyList;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user != null)
                {
                    var userProperties = user.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        var targetProperty = typeof(User).GetProperties()
                            .FirstOrDefault(p => p.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

                        if (targetProperty != null && targetProperty.CanWrite)
                        {
                            if (targetProperty.PropertyType == typeof(bool))
                            {
                                targetProperty.SetValue(user, bool.Parse(property.Value));
                            }
                            else if (targetProperty.PropertyType == typeof(string))
                            {
                                targetProperty.SetValue(user, property.Value);
                            }
                        }
                    }
                    _dbContext.SaveChanges();
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.Error($"DB error: {ex.Message}");
                throw new Exception($"DB error: {ex.Message}");
            }
        }
    }
}
