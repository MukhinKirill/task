using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Task.Connector.Service;
using Task.Connector.Service.Interface;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private string _requestRightGroupName = "Request";
        private string _itRoleRightGroupName = "Role";
        private string delimeter = ":";
        public ILogger Logger { get; set; }
        private DataContext _dbContext;
        private IUserService _userService;
        private IPermissionService _permissionService;
        public void StartUp(string connectionString)
        {
            DbContextOptionsBuilder<DataContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<DataContext>();

            dbContextOptionsBuilder.UseNpgsql(this.GetConnectionString(connectionString));
            _dbContext = new DataContext(dbContextOptionsBuilder.Options);

            if (_dbContext.Database.CanConnect() == false)
            {
                Logger?.Error("No connection to the database");
            }

            _userService = new UserService();
            _permissionService = new PermissionService();
        }

        private string GetConnectionString(string connect)
        {
            int startIndex = connect.IndexOf("ConnectionString='") + "ConnectionString='".Length;
            int endIndex = connect.IndexOf("';", startIndex);

            return connect.Substring(startIndex, endIndex - startIndex);
        }

        public void CreateUser(UserToCreate user)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    Task.Connector.Models.PropertyModel property = _userService.ParseProperties(user.Properties);

                    User entity = new User() { Login = user.Login, IsLead = property.IsLead ?? false, LastName = property.LastName ?? string.Empty, FirstName = property.FirstName ?? string.Empty, MiddleName = property.MiddleName ?? string.Empty, TelephoneNumber = property.TelephoneNumber ?? string.Empty };
                    _dbContext.Users.Add(entity);

                    _dbContext.Passwords.Add(new Sequrity { UserId = user.Login, Password = user.HashPassword });

                    _dbContext.SaveChanges();

                    transaction.Commit();

                    Logger.Debug("Create user success");


                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"Failed to create a user {this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}"); ;
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            List<Property> properties = new List<Property>();

            IEnumerable<PropertyInfo> userProperties = typeof(User).GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() == null);

            foreach (var userProperty in userProperties)
            {
                properties.Add(new Property(userProperty.Name, userProperty.PropertyType.ToString()));
            }

            PropertyInfo passwordProperty = typeof(Sequrity).GetProperty("Password");

            properties.Add(new Property(passwordProperty.Name, passwordProperty.PropertyType.ToString()));

            Logger.Debug("Get all properties success");

            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                User user = _dbContext.Users.Find(userLogin);
                if (user == null)
                {
                    throw new NullReferenceException();
                }
                return _userService.SerializeUserPropertyFromUser(user);


            }
            catch (NullReferenceException ex)
            {
                Logger.Warn("User not found");
                return null;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            User user = _dbContext.Users.Find(userLogin);
            return user is null ? false : true;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                if (!this.IsUserExists(userLogin))
                    throw new NullReferenceException();

                User user = _dbContext.Users.Find(userLogin);

                Task.Connector.Models.PropertyModel property = _userService.ParseProperties(properties);

                user.LastName = property.LastName ?? user.LastName;
                user.FirstName = property.FirstName ?? user.FirstName;
                user.MiddleName = property.MiddleName ?? user.MiddleName;
                user.TelephoneNumber = property.TelephoneNumber ?? user.TelephoneNumber;
                user.IsLead = property.IsLead ?? user.IsLead;

                _dbContext.Users.Update(user);
                _dbContext.SaveChanges();

            }
            catch (NullReferenceException ex)
            {
                Logger.Warn("User not found");
            }
            catch (Exception ex)
            {
                Logger.Error("The user's data has not been updated");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                List<Permission> permissions = new List<Permission>();

                IQueryable<RequestRight> requestRights = _dbContext.RequestRights;
                IQueryable<ITRole> iTRoles = _dbContext.ITRoles;

                foreach (var item in requestRights)
                {
                    permissions.Add(new Permission(item.Id.ToString(), $"{_requestRightGroupName}{delimeter}{item.Name}", ""));
                }
                foreach (var item in iTRoles)
                {
                    permissions.Add(new Permission(item.Id.ToString(), $"{_itRoleRightGroupName}{delimeter}{item.Name}", ""));

                }
                Logger.Debug("Get all permissions success");
                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred while receiving" + ex.Message);
                return null;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                if (!this.IsUserExists(userLogin))
                    throw new NullReferenceException();
                IEnumerable<int> requestRightIds;
                IEnumerable<int> itRoleIds;
                _permissionService.ParsePermission(rightIds, out requestRightIds, out itRoleIds);
                foreach (var item in requestRightIds)
                {
                    _dbContext.UserRequestRights.Add(new UserRequestRight() { RightId = item, UserId = userLogin });
                }
                foreach (var item in itRoleIds)
                {
                    _dbContext.UserITRoles.Add(new UserITRole() { RoleId = item, UserId = userLogin });
                }

                _dbContext.SaveChanges();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn("User not found");
            }
            catch (ArgumentException ex)
            {
                Logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                if (!this.IsUserExists(userLogin))
                    throw new NullReferenceException();
                IEnumerable<int> requestRightIds;
                IEnumerable<int> itRoleIds;
                _permissionService.ParsePermission(rightIds, out requestRightIds, out itRoleIds);
                foreach (var item in requestRightIds)
                {
                    _dbContext.UserRequestRights.Remove(new UserRequestRight() { RightId = item, UserId = userLogin });
                }
                foreach (var item in itRoleIds)
                {
                    _dbContext.UserITRoles.Remove(new UserITRole() { RoleId = item, UserId = userLogin });
                }

                _dbContext.SaveChanges();
            }
            catch (NullReferenceException ex)
            {
                Logger.Warn("User not found");
            }
            catch (ArgumentException ex)
            {
                Logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            List<string> result = new List<string>();
            try
            {
                if (!this.IsUserExists(userLogin))
                    throw new NullReferenceException();

                IQueryable<UserRequestRight> userRequestRights = _dbContext.UserRequestRights.Where(u => u.UserId == userLogin);

                IQueryable<RequestRight> requestRights = _dbContext.RequestRights.Where(u => userRequestRights.Select(x => x.RightId).Any(x => x == u.Id));

                IQueryable<UserITRole> userITRoles = _dbContext.UserITRoles.Where(u => u.UserId == userLogin);

                IQueryable<ITRole> iTRoles = _dbContext.ITRoles.Where(u => userITRoles.Select(x => x.RoleId).Any(x => x == u.Id));

                foreach (var item in requestRights)
                {
                    result.Add($"{_requestRightGroupName}{delimeter}{item}");
                }
                foreach (var item in iTRoles)
                {
                    result.Add($"{_itRoleRightGroupName}{delimeter}{item}");
                }

                return result;

            }
            catch (NullReferenceException ex)
            {
                return result;
                Logger.Warn("User not found");
            }
            catch (Exception ex)
            {
                return result;
                Logger.Error(ex.Message);
            }
        }

    }
}