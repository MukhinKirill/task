using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Task.Integration.Data.DbCommon.DbModels;
using Microsoft.IdentityModel.Tokens;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _dbContext;
        private List<Property> _clientUserProperties;
        public void StartUp(string connectionString)
        {
            if(string.IsNullOrEmpty(connectionString))
            {
                Logger.Error("Empty connection string");
                throw new ArgumentNullException("Connection string null or empty");
            }

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            if(connectionString.Contains("SqlServer"))
            {
                optionsBuilder.UseSqlServer(connectionString);
                Logger.Debug("Using SQL server database provider");
            } 
            if(connectionString.Contains("PostgreSQL"))
            {
                optionsBuilder.UseNpgsql(connectionString);
                Logger.Debug("Using Npgsql database provider");
            } 

            try
            {
                _dbContext = new DataContext(optionsBuilder.Options);
                Logger.Debug("Database context created");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            // This might need to be different if i need the db's column names instead of the model property names
            _clientUserProperties = typeof(User).GetProperties().
                Select(prop => new Property(prop.Name, prop.Name)).
                ToList();
            
            _clientUserProperties.Add(new Property("Password", "Password"));
            
        }

        public void CreateUser(UserToCreate user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            if(!_clientUserProperties.IsNullOrEmpty()) return _clientUserProperties;
            string errorMessage = $"The list of user properties is null or empty, something went wrong on startup, or startup was not called";
            Logger.Error(errorMessage);
            throw new Exception(errorMessage);
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }

        public bool IsUserExists(string userLogin)
        {
            return _dbContext.Users.Find(userLogin) is not null;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            throw new NotImplementedException();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }

        public ILogger Logger { get; set; }
    }
}