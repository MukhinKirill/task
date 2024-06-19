using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }

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
            if(dataContext == null)
            {
                Logger?.Warn("dataContext not initialised");
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

            Logger?.Debug($"Trying to add new User: {user.Login}");
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
            throw new NotImplementedException();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
        }
    }
}