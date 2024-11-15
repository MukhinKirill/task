using Task.Connector.Contexts;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        private ConnectorDbContext _context;
        public async void StartUp(string connectionString)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                bool isAvalaible = await db.Database.CanConnectAsync();
                if (isAvalaible) Logger?.Debug("Database context initialized.");
                else Logger?.Debug("Database context couldn't be initialized.");
            }
        }

        public async void CreateUser(UserToCreate user)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
            }
        }

        public  IEnumerable<Property> GetAllProperties()
        {
            throw new InvalidCastException();
        }

        public  IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new InvalidCastException();
        }

        public  bool IsUserExists(string userLogin)
        {
            throw new InvalidCastException();
        }

        public  void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new InvalidCastException();
        }

        public  IEnumerable<Permission> GetAllPermissions()
        {
            throw new InvalidCastException();
        }

        public  void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new InvalidCastException();
        }

        public  void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new InvalidCastException();
        }

        public  IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new InvalidCastException();
        }
    }
}