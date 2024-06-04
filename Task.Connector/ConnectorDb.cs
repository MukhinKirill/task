using System.Data;
using Task.Connector.Models;
using Task.Connector.Repositories;
using Task.Connector.Services;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private readonly UserConverter userConverter;
        private readonly PropertyAttConverter propConverter;
        private readonly PermissionConverter permissionConverter;

        private IStorage storage;

        public ILogger Logger { get; set; }

        public ConnectorDb()
        {
            userConverter = new UserConverter();
            propConverter = new PropertyAttConverter();
            permissionConverter = new PermissionConverter();
        }
        public void StartUp(string connectionString)
        {
            storage = new Repository(connectionString, Logger);
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
            foreach(var rightId in rightIds)
            {
                var splitString = rightId.Split(":");
                if (splitString[0] == "Role")
                {
                    var id = int.Parse(splitString[1]);
                    var userItRole = new UserItrole()
                    {
                        RoleId = id,
                        UserId = userLogin
                    };
                    using (TestDbContext db = storage.ConnectToDatabase())
                    {
                        db.UserItroles.Add(userItRole);
                        db.SaveChanges();
                    }
                
                } else if (splitString[0] == "Request")
                {
                    var id = int.Parse(splitString[1]);
                    var userItRole = new UserRequestRight()
                    {
                        RightId = id,
                        UserId = userLogin
                    };
                    using (TestDbContext db = storage.ConnectToDatabase())
                    {
                        db.UserRequestRights.Add(userItRole);
                    }
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            foreach (var rightId in rightIds)
            {
                var splitString = rightId.Split(":");
                if (splitString[0] == "Role")
                {
                    var id = int.Parse(splitString[1]);
                    using (TestDbContext db = storage.ConnectToDatabase())
                    {
                        var userItRole = db.UserItroles.Where(u => userLogin == u.UserId && u.RoleId == id);
                        db.UserItroles.RemoveRange(userItRole);
                        db.SaveChanges();
                    }

                }
                else if (splitString[0] == "Request")
                {
                    var id = int.Parse(splitString[1]);
                    using (TestDbContext db = storage.ConnectToDatabase())
                    {
                        var userItRole = db.UserRequestRights.Where(u => userLogin == u.UserId && u.RightId == id);
                        db.UserRequestRights.RemoveRange(userItRole);
                        db.SaveChanges();
                    }
                }
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var roles = storage.GetItRolesFromUser(userLogin);
            var rights = storage.GetItRequestRightsFromUser(userLogin);
            var permissions = permissionConverter.GetAllPermissionFrom(roles, rights);
            List<string> strings = new();
            foreach (var perm in permissions) strings.Add(perm.Name);
            return strings;
        }

    }
}