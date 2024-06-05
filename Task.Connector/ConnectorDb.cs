using Task.Connector.Models;
using Task.Connector.Repositories;
using Task.Connector.Converters;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Repositories.Factory;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private UserConverter userConverter;
        private PropertyAttrConverter propConverter;
        private PermissionConverter permissionConverter;

        private IStorage storage;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            storage = RepositoryFactory.CreateRepositoryFrom(connectionString);
            userConverter = new UserConverter();
            propConverter = new PropertyAttrConverter();
            permissionConverter = new PermissionConverter();
        }

        public void CreateUser(UserToCreate user)
        {
            var data = userConverter.GetDataUser(user);
            storage.AddUser(data.usr, data.pass);
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var properties = new List<Property>();
            properties.AddRange(propConverter.GetAttributesFromType(typeof(User)));
            properties.AddRange(propConverter.GetAttributesFromType(typeof(Password)));
            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = storage.GetUserFromLogin(userLogin);
            var properties = userConverter.GetUserPropertiesFromUser(user);
            return properties;
        }

        public bool IsUserExists(string userLogin)
        {
            return storage.IsUserExists(userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = storage.GetUserFromLogin(userLogin);
            var userProps = userConverter.GetUserPropertiesFromUser(user);
            foreach (var prop in properties)
            {
                foreach (var userProp in userProps)
                {
                    if (prop.Name.Equals(userProp.Name)) userProp.Value = prop.Value;
                }
            }
            userConverter.SetUserProperties(user, userProps);
            storage.UpdateUser(user);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var roles = storage.GetAllItRoles();
            var rights = storage.GetAllItRequestRights();
            return permissionConverter.GetAllPermissionFrom(roles, rights);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var data = permissionConverter.SortPermissonsToData(userLogin, rightIds);
            if (data.userItRole != null && data.userItRole.Count != 0) storage.AddRolesToUser(userLogin, data.userItRole);
            if (data.userRequestRights != null && data.userRequestRights.Count != 0) storage.AddRequestRightsToUser(userLogin, data.userRequestRights);
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var data = permissionConverter.SortPermissonsToData(userLogin, rightIds);
            if (data.userItRole != null && data.userItRole.Count != 0) storage.RemoveRolesToUser(userLogin, data.userItRole);
            if (data.userRequestRights != null && data.userRequestRights.Count != 0) storage.RemoveRequestRightsToUser(userLogin, data.userRequestRights);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var roles = storage.GetItRolesFromUser(userLogin);
            var rights = storage.GetItRequestRightsFromUser(userLogin);
            var permissions = permissionConverter.GetAllPermissionFrom(roles, rights);
            var strings = new List<string>();
            foreach (var perm in permissions) strings.Add(perm.Name);
            return strings;
        }

    }
}
