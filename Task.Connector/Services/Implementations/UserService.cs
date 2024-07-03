using Task.Integration.Data.Models;
using Task.Connector.DAL;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Connector.Services.Interfaces;

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

        public void CreateUser(UserToCreate user)
        {
            var userToCreateProperties = user.GetType().GetProperties();

            var userToAdd = new User();

            var userToAddProperties = userToAdd.GetType().GetProperties();

            var b = userToAdd.GetType();

            foreach (var property in userToCreateProperties)
            {
                if (userToAddProperties.Any(p => p.Name == property.Name))
                {
                    var changeProperty = userToAddProperties.First(p => p.Name == property.Name);
                    changeProperty.SetValue(changeProperty, property.GetValue(property) );
                }
                //TODO
                else { /*_logger.Error($"invalid entity property {property.Name}");*/ }
            }

            var a = userToAdd.GetType().GetProperties();

            _dbContext.Securities.Add(new Sequrity() { UserId = user.Login, Password = user.HashPassword });


        }

        public bool IsUserExists(string userLogin) => _dbContext.Users.Any(user => user.Login == userLogin);


    }
}
