using Task.Integration.Data.Models;
using Task.Connector.DAL;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Connector.Services.Interfaces;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ConnectorDbContext _dbContext;
        private readonly ILogger _logger;
        public UserService(ConnectorDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public void CreateUser(UserToCreate userToCreate)
        {
            if (userToCreate == null)
            {
                _logger.Error("userToCreate must be not null");
                return;
            }
            var user = new User
            {
                Login = userToCreate.Login,
                FirstName = "",
                LastName = "",
                MiddleName = "",
                TelephoneNumber = ""
            };


            var userType = typeof(User);
            var properties = userType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in userToCreate.Properties)
            {
                var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
                if (targetProperty.CanWrite)
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

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.Users.Add(user);
                _dbContext.Securities.Add(new Sequrity() { UserId = userToCreate.Login, Password = userToCreate.HashPassword });
                _dbContext.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.Error($"DB error: {ex.Message}");
                throw new Exception($"DB error: {ex.Message}");
            }

        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                return _dbContext.Users.Any(user => user.Login == userLogin);
            }
            catch (Exception ex)
            {
                _logger.Error($"DB error: {ex.Message}");
                throw new Exception($"DB error: {ex.Message}");
            }
        }
    }
}
