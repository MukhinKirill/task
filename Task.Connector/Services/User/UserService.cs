using System.Data;
using System.Reflection;
using Task.Connector.Database;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using UserEntity = Task.Integration.Data.DbCommon.DbModels.User;

namespace Task.Connector.Services.User
{
    public class UserService : IUserService
    {
        private readonly DataBaseContext _db;
        private ILogger _logger;

        public UserService(DataBaseContext db, ILogger logger)
        {
            (_db, _logger) = (db, logger);
        }

        public void CreateUser(UserToCreate user)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var userEntity = new UserEntity()
                    {
                        Login = user.Login
                    };

                    var userEntityInitialized = UserHelper.InitializeUser(userEntity, user.Properties);
                    _db.Users.Add(userEntityInitialized);

                    var sequrity = new Sequrity()
                    {
                        UserId = userEntity.Login,
                        Password = user.HashPassword
                    };
                    _db.Sequrities.Add(sequrity);

                    _db.SaveChanges();

                    transaction.Commit();
                    _logger?.Debug("[User][Create] - success");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger?.Error($"[User][Create] - error: {ex.Message}");
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var userProperties = typeof(UserEntity)
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute(typeof(System.ComponentModel.DataAnnotations.KeyAttribute)) is null)
                    .Select(p => new Property(p.Name, string.Empty));

                var passwordPropertyInfo = typeof(Sequrity).GetProperty("password", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (passwordPropertyInfo is null)
                {
                    throw new NullReferenceException("Password property is null");
                }
                var passwordProperty = new Property(passwordPropertyInfo.Name, string.Empty);

                _logger?.Debug("[User][GetAllProperties] - success");
                return userProperties.Append(passwordProperty);
            }
            catch (Exception ex)
            {
                _logger?.Error($"[User][GetAllProperties] - error: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user is null)
                {
                    throw new DataException("User doesn't exist");
                }

                var userProperties = user
                    .GetType()
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute(typeof(System.ComponentModel.DataAnnotations.KeyAttribute)) is null)
                    .Select(p => new UserProperty(p.Name, p.GetValue(user).ToString()));

                _logger?.Debug("[User][GetProperties] - success");
                return userProperties;
            }
            catch (Exception ex)
            {
                _logger?.Error($"[User][GetProperties] - error: {ex.Message}");
                return null;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                var userState = _db.Users.Any(u => u.Login == userLogin);
                _logger?.Debug("[User][IsExists] - success");
                return userState;
            }
            catch (Exception ex)
            {
                _logger?.Error($"[User][IsExists] - error: {ex.Message}");
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var user = _db.Users.FirstOrDefault(u => u.Login == userLogin);
                    if (user is null)
                    {
                        throw new DataException("User doesn't exist");
                    }

                    properties
                        .ToList()
                        .ForEach(p => user
                            .GetType()
                            .GetProperty(p.Name)
                            .SetValue(user, p.Value));

                    _db.SaveChanges();
                    transaction.Commit();

                    _logger?.Debug("[User][UpdateProperties] - success");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger?.Error($"[User][UpdateProperties] - error: {ex.Message}");
                }
            }
        }
    }
}