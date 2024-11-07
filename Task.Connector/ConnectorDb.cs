using Task.Connector.Implementation;
using Task.Connector.Infrastructure;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private ConnectorUoW _connectorUoW;
        private DataManager _dataManager;
        private DataContext _context;
        private Random _random = new Random();
        public void StartUp(string connectionString)
        {
            var startIdx = connectionString.IndexOf("ConnectionString='") + "ConnectionString='".Length;
            var endIdx = connectionString.IndexOf("';", startIdx);
            var extractedConnectionString = connectionString.Substring(startIdx, endIdx - startIdx);

            var providerStartIdx = connectionString.IndexOf("Provider='") + "Provider='".Length;
            var providerEndIdx = connectionString.IndexOf("';", providerStartIdx);
            var rawProvider = connectionString.Substring(providerStartIdx, providerEndIdx - providerStartIdx);

            var provider = rawProvider.StartsWith("PostgreSQL", StringComparison.OrdinalIgnoreCase) ? "POSTGRE" : rawProvider.ToUpper();

            var dbContextFactory = new DbContextFactory(extractedConnectionString);
            _context = dbContextFactory.GetContext(provider);
            _connectorUoW = new ConnectorUoW(_context);
        }
        public void CreateUser(UserToCreate user)
        {
            _connectorUoW.UserRepository.Add(new Task.Integration.Data.DbCommon.DbModels.User()
            {
                Login = user.Login,
                LastName = GenerateRandomString(8),
                FirstName = GenerateRandomString(6),
                MiddleName = GenerateRandomString(7),
                TelephoneNumber = string.Format($"+7({0})-{1}", _random.Next(900, 999), _random.Next(100000, 999999)),
                IsLead = _random.Next(2) == 1

            });

            _connectorUoW.PasswordRepository.Add(new Sequrity()
            {
                UserId = user.Login,
                Password = user.HashPassword
            });
            _connectorUoW.Commit();
        }
        //todo: доработать, чтобы возвращал пароли
        public IEnumerable<Property> GetAllProperties()
        {
            return PropertyHelper.GetAllProperties();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _connectorUoW.UserRepository.GetById(userLogin);
            return PropertyHelper.GetUserProperties(user);
        }

        public bool IsUserExists(string userLogin)
        {
            if (_connectorUoW.UserRepository.GetById(userLogin) != null)
                return true;
            return false;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = _connectorUoW.UserRepository.GetById(userLogin);
            if (user != null)
            {
                user = PropertyHelper.UpdateUserProperties(properties, user);
            }
            _connectorUoW.Commit();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var userRights = _connectorUoW.RequestRightRepository.GetAll().ToList();
            var permissions = new List<Permission>();
            foreach (var permission in userRights)
            {
                permissions.Add(new Permission(permission.Id.ToString(), permission.Name, "RequestRight"));
            }
            var itRoles = _connectorUoW.ITRoleRepository.GetAll().ToList();
            foreach (var itRole in itRoles)
            {
                permissions.Add(new Permission(itRole.Id.ToString(), itRole.Name, "ItRole"));
            }
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            foreach (var permission in rightIds)
            {
                var permissionType = permission.Split(":", StringSplitOptions.RemoveEmptyEntries)[0];
                var permissionId = permission.Split(":", StringSplitOptions.RemoveEmptyEntries)[1];
                if (permissionType.Contains("Request", StringComparison.OrdinalIgnoreCase))
                {
                    _connectorUoW.UserRequestRightRepository.AddUserPermissions(userLogin, permissionId);
                }
                else if (permissionType.Contains("Role", StringComparison.OrdinalIgnoreCase))
                {
                    _connectorUoW.UserITRoleRepository.AddUserRoles(userLogin, permissionId);
                }
            }

            _connectorUoW.Commit();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            _connectorUoW.UserRequestRightRepository.RemoveUserPermissions(userLogin, rightIds);
            _connectorUoW.Commit();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var userRequestRight = _connectorUoW.UserRequestRightRepository.GetAll().Where(x => x.UserId == userLogin).ToList();
            return _connectorUoW.RequestRightRepository.GetRequestRightsNames(userRequestRight);

        }

        public ILogger Logger { get; set; }
        private string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(chars.Substring(_random.Next(0, chars.Length - 1)));
        }
        string GetValue(string input, string key)
        {
            var startIdx = input.IndexOf(key + "'") + (key + "'").Length;
            var endIdx = input.IndexOf("';", startIdx);
            return input.Substring(startIdx, endIdx - startIdx);
        }
    }
}