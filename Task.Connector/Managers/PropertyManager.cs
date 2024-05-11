using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.DbCommon;
using System.ComponentModel.DataAnnotations;

namespace Task.Connector.Managers
{
    public class PropertyManager
    {
        private DataContext dbContext;
        private ILogger _logger;

        public PropertyManager(DataContext dbContext, ILogger logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger;
        }
        private IEnumerable<Property> GetProperties(Type entityType, bool ignoreKeyAttribute = false)
        {
            return entityType.GetProperties()
                .Where(property => property.CanRead && (ignoreKeyAttribute || !property.GetCustomAttributes(typeof(KeyAttribute), false).Any()))
                .Select(property => new Property(property.Name, string.Empty));
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var userProperties = GetProperties(typeof(User));

                var userPasswordProperty = GetProperties(typeof(Sequrity))
                    .Where(property => property.Name.ToLower() == "password");

                var allUserProperties = userProperties.Concat(userPasswordProperty);

                _logger?.Debug("Все свойства пользователя успешно получены!");

                return allUserProperties;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Ошибка при получении свойств пользователя: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = dbContext.Users.FirstOrDefault(user => user.Login == userLogin);

                var userProperties = GetProperties(typeof(User)).
                    Select(property => new UserProperty(property.Name, typeof(User).GetProperty(property.Name).GetValue(user)?.ToString() ?? string.Empty));

                _logger?.Debug("Свойства пользователя успешно получены!");

                return userProperties;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Ошибка получения свойств пользователя: {ex.Message}!");
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = dbContext.Users.FirstOrDefault(user => user.Login == userLogin);

                foreach (var property in properties)
                {
                    var userProperty = typeof(User).GetProperty(property.Name);

                    if (userProperty != null)
                    {
                        userProperty.SetValue(user, Convert.ChangeType(property.Value, userProperty.PropertyType));
                    }
                    else
                    {
                        _logger?.Error($"Свойство {property.Name} не существует в модели пользователя");
                        throw new Exception($"Свойство {property.Name} не существует в модели пользователя");
                    }

                }

                dbContext.SaveChanges();

                _logger?.Debug("Свойства пользователя успешно обновлены!");

            }
            catch (Exception ex)
            {
                _logger?.Error($"Ошибка обновления свойств пользователя: {ex.Message}");
                throw;
            }
        }
    }
}
