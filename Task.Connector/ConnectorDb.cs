using Task.Connector.Entities;
using Task.Connector.Persistence;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector, IDisposable
    {
        private bool _disposed;
        private DataContext _context = null!;
        private UserRepository _userRepository = null!;
        public ILogger Logger { get; set; } = null!;

        public void StartUp(string connectionString)
        {
            DataContextFactory factory = new("Server=localhost;Port=7900;Database=Avanpost;Username=Avanpost;Password=Avanpost;", Logger);
            _context = factory.GetContext();
            _userRepository = new(_context);
        }

        public void CreateUser(UserToCreate user)
        {
            UserModel userModel = new(
                user,
                _userRepository.GetCountUsers());

            string isLeadString = userModel.IsLead ? "User is a lead" : "User is not a lead";
            Logger.Debug($"Attempt to create a user with login '{userModel.Login}' and other properties: firstName - '{userModel.FirstName}'; middleName - '{userModel.MiddleName}'; lastName - '{userModel.LastName}'; telephoneNumber - '{userModel.TelephoneNumber}'. {isLeadString}");

            _userRepository.Create(userModel);

            Logger.Debug($"User {userModel.Login} was created");
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
            throw new NotImplementedException();
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }

        ~ConnectorDb()
        {
            Dispose(false);
        }
    }
}