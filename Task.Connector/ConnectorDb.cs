using Task.Connector.DAL;
using Task.Connector.Services;
using Task.Connector.Services.Implementations;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private ConnectorDbContext _dbContext;
        private IUserService _userService;
        private IPropertyService _propertyService;
        private IPermissionService _permissionService;
        public ConnectorDb()
        { 
        }
        public void StartUp(string connectionString)
        {
            _dbContext = new ConnectorDbContext(connectionString);
            _userService = new UserService(_dbContext,Logger);
            _propertyService = new PropertyService(_dbContext, Logger);
            _permissionService = new PermissionService(_dbContext, Logger);
        }
        public void CreateUser(UserToCreate user) 
            => _userService.CreateUser(user);
        public bool IsUserExists(string userLogin) 
            => _userService.IsUserExists(userLogin);
        public IEnumerable<Property> GetAllProperties() 
            => _propertyService.GetAllProperties();
        public IEnumerable<UserProperty> GetUserProperties(string userLogin) 
            => _propertyService.GetUserProperties(userLogin);
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin) 
            => _propertyService.UpdateUserProperties(properties, userLogin);
        public IEnumerable<Permission> GetAllPermissions() 
            => _permissionService.GetAllPermissions();
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds) 
            => _permissionService.AddUserPermissions(userLogin, rightIds);   
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
            => _permissionService.RemoveUserPermissions(userLogin, rightIds);  
        public IEnumerable<string> GetUserPermissions(string userLogin)
            => _permissionService.GetUserPermissions(userLogin);
        public ILogger Logger { get; set; }
    }
}