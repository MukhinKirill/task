using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

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
            if (connectionString.Contains("SqlServer"))
            {
                dataContext = factory.GetContext("MSSQL");
                return;
            }
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

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger?.Debug($"Request for get user permissions: {userLogin}");

            if (dataContext == null)
            {
                Logger?.Error($"{nameof(GetUserPermissions)}: dataContext not initialised");
                return Enumerable.Empty<string>();
            }

            if (!IsUserExists(userLogin))
            {
                Logger?.Warn($"User with login: {userLogin} - not exists");
                return Enumerable.Empty<string>();
            }

            var userPermissions = dataContext.UserRequestRights.Where(x => x.UserId == userLogin)
                .Join(dataContext.RequestRights, x => x.RightId, y => y.Id, (x, y) => y.Name);

            Logger?.Debug($"User permissions successfully sended: {userLogin}");

            return userPermissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger?.Debug($"Request for add permission for user: {userLogin}");

            if (dataContext == null)
            {
                Logger?.Error($"{nameof(AddUserPermissions)}: dataContext not initialised");
                return;
            }

            if (!IsUserExists(userLogin))
            {
                Logger?.Warn($"User with login: {userLogin} - not exists");
                return;
            }

            foreach (var rightId in rightIds)
            {
                var splitedRightId = rightId.Split(':');

                if (!int.TryParse(splitedRightId[1], out int val))
                {
                    Logger?.Error($"RightId isn't int. userLogin: {userLogin}");
                    return;
                }

                if(!dataContext.UserRequestRights.Any(x => x.UserId == userLogin && x.RightId == val))
                {
                    dataContext.UserRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = val });
                    Logger?.Debug($"Successfully added new request right: {userLogin} - {val}");
                }
                
                dataContext.UserITRoles.Add(new UserITRole { UserId = userLogin, RoleId = val});
                Logger?.Debug($"Successfully added new role: {userLogin} - {val}");
            }

            Logger?.Debug($"Trying to save User rights: {userLogin}");

            try
            {
                dataContext.SaveChanges();
            }
            catch (Exception)
            {
                Logger?.Error($"New rights for user: {userLogin} - not saved");
                return;
            }

            Logger?.Debug($"Successfully saved rights for: {userLogin}");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger?.Debug($"Request for remove permission for user: {userLogin}");

            if (dataContext == null)
            {
                Logger?.Error($"{nameof(RemoveUserPermissions)}: dataContext not initialised");
                return;
            }

            if (!IsUserExists(userLogin))
            {
                Logger?.Warn($"User with login: {userLogin} - not exists");
                return;
            }

            foreach (var rightId in rightIds)
            {
                var splitedRightId = rightId.Split(':');

                if (!int.TryParse(splitedRightId[1], out int val))
                {
                    Logger?.Error($"RightId isn't int. userLogin: {userLogin}");
                    return;
                }

                if (dataContext.UserRequestRights.Any(x => x.UserId == userLogin && x.RightId == val))
                {
                    dataContext.UserRequestRights.Remove(new UserRequestRight { UserId = userLogin, RightId = val });
                    Logger?.Debug($"Successfully remove right: {userLogin} - {val}");
                }
            }

            Logger?.Debug($"Trying to save User removed rights: {userLogin}");

            try
            {
                dataContext.SaveChanges();
            }
            catch (Exception)
            {
                Logger?.Error($"Removed rights for user: {userLogin} - not saved");
                return;
            }

            Logger?.Debug($"Successfully saved rights for: {userLogin}");
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger?.Debug($"Request for get user properties: {userLogin}");

            if (dataContext == null)
            {
                Logger?.Error($"{nameof(GetUserProperties)}: dataContext not initialised");
                return Enumerable.Empty<UserProperty>();
            }

            if (!IsUserExists(userLogin))
            {
                Logger?.Warn($"User with login: {userLogin} - not exists");
                return Enumerable.Empty<UserProperty>();
            }

            var user = dataContext.Users.Find(userLogin);

            var userProperties = new UserProperty[]
            {
                new("lastName ", user!.LastName ?? ""),
                new("firstName", user!.FirstName ?? ""),
                new("middleName", user!.MiddleName ?? ""),
                new("telephoneNumber", user!.TelephoneNumber ?? ""),
                new("isLead", user!.IsLead.ToString()),
            };

            Logger?.Debug($"User properties successfully sended: {userLogin}");

            return userProperties;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger?.Debug($"Request for update user properties: {userLogin}");

            if (dataContext == null)
            {
                Logger?.Error($"{nameof(UpdateUserProperties)}: dataContext not initialised");
                return ;
            }

            if (!IsUserExists(userLogin))
            {
                Logger?.Warn($"User with login: {userLogin} - not exists");
                return ;
            }

            var user = dataContext.Users.Find(userLogin);

            foreach (var property in properties)
            {
                if(property == null)
                {
                    Logger?.Error($"Property is null");
                    return;
                }
                if (property.Name == "lastName") user!.LastName = property.Value;
                else if (property.Name == "firstName") user!.FirstName = property.Value;
                else if (property.Name == "middleName") user!.MiddleName = property.Value;
                else if (property.Name == "telephoneNumber") user!.TelephoneNumber = property.Value;
                else if (property.Name == "isLead")
                {
                    if(!bool.TryParse(property.Value, out bool result))
                    {
                        Logger?.Error($"Property isn't bool");
                        return;
                    }
                    user!.IsLead = result;
                }
            }

            Logger?.Debug($"Trying to save updated User properties: {userLogin}");

            try
            {
                dataContext.SaveChanges();
            }
            catch (Exception)
            {
                Logger?.Error($"New properties for user: {userLogin} - not saved");
                return;
            }

            Logger?.Debug($"User properties successfully updated: {userLogin}");
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