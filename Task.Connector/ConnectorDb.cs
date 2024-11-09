using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _dbContext;
        public void StartUp(string connectionString)
        {
            if(string.IsNullOrEmpty(connectionString))
            {
                Logger.Error("Empty connection string");
                throw new ArgumentNullException("Connection string null or empty");
            }

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            if(connectionString.Contains("SqlServer")) optionsBuilder.UseSqlServer(connectionString);
            if(connectionString.Contains("PostgreSQL")) optionsBuilder.UseNpgsql(connectionString); 

            try
            {
                _dbContext = new DataContext(optionsBuilder.Options);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        public void CreateUser(UserToCreate user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
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