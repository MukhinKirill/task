using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger? Logger { get; set; }

        private DataContext? dataContext;

        public ConnectorDb() { }

        public void StartUp(string connectionString)
        {
            var updatedConnectionString = connectionString.Split('\'')[1];
            var factory = new DbContextFactory(updatedConnectionString);
            dataContext = factory.GetContext("POSTGRE");
        }

        public void CreateUser(UserToCreate user)
        {
            Logger?.Debug($"Request for adding user: {user.Login}");

            if (dataContext == null)
            {
                Logger?.Error($"{nameof(CreateUser)}: dataContext not initialised");
                return;
            }

            if (IsUserExists(user.Login))
            {
                Logger?.Warn($"User with login: {user.Login} - alredy exists");
                return;
            }
            
            var newUser = new User
            {
                Login = user.Login,
                LastName = user.Properties.FirstOrDefault(x => x.Name == "lastName")?.Value ?? "",
                FirstName = user.Properties.FirstOrDefault(x => x.Name == "firstName")?.Value ?? "",
                MiddleName = user.Properties.FirstOrDefault(x => x.Name == "middleName")?.Value ?? "",
                TelephoneNumber = user.Properties.FirstOrDefault(x => x.Name == "telephoneNumber")?.Value ?? "",
                IsLead = user.Properties.FirstOrDefault(x => x.Name == "isLead")?.Value == "true",
            };

            var newPass = new Sequrity
            {
                UserId = user.Login,
                Password = user.HashPassword,
            };

            dataContext.Users.Add(newUser);
            dataContext.Passwords.Add(newPass);

            Logger?.Debug($"Trying to save new User: {user.Login}");

            try
            {
                dataContext.SaveChanges();
            }
            catch (Exception)
            {
                Logger?.Error($"User: {user.Login} - not saved");
                return;
            }

            Logger?.Debug($"Successfully aded user: {user.Login}");
        }

        public bool IsUserExists(string userLogin)
        {
            return dataContext!.Users.Any(x => x.Login == userLogin);
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

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
        }

        /*GetAll methods*/

        public IEnumerable<Permission> GetAllPermissions()
        {
            var permissions = new List<Permission>();
            Logger?.Debug("Request for all permissions");

            if (dataContext == null)
            {
                Logger?.Error($"{nameof(GetAllPermissions)}: dataContext not initialised");
                return permissions;
            }

            permissions.AddRange(dataContext.RequestRights.Select(x => new Permission (x.Id.ToString(), x.Name, "description")));
            permissions.AddRange(dataContext.ITRoles.Select(x => new Permission(x.Id.ToString(), x.Name, "description")));

            Logger?.Debug("Done - request for all permissions");

            return permissions;
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger?.Warn("List of properties was requested");
            return new Property[]
            {
                new("lastName ", "Last Name"),
                new("firstName", "First Name"),
                new("middleName", "Middle Name"),
                new("telephoneNumber", "Telephone Number"),
                new("isLead", "Is Lead"),
                new("password", "Password"),
            };
        }
    }
}