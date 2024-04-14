using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Task.Connector.DataBase;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public void StartUp(string connectionString)
        {
            DbContext = new(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            UserBuilder builder = new(DbContext);
            InvokeWithLogger(()=> builder.AddProperty("Login", user.Login));
            InvokeWithLogger(() => builder.AddPassword(user.HashPassword));

            UpdateUserProperties(user.Properties, builder);

            DbContext.Users.Add(builder.Build());
            DbContext.SaveChanges();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return typeof(User).GetProperties()
                .Select(p => p.GetCustomAttribute<DbItemPropertyAttribute>())
                .Where(p => p != null)
                .Select(p => new Property(p.Name, p.Description));
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = getUser(userLogin);
            return typeof(User).GetProperties()
                .Select(p => new { property = p, attr = p.GetCustomAttribute<DbItemPropertyAttribute>() })
                .Where(p => p.attr != null)
                .Select(p => new UserProperty(p.attr.Name, p.property.GetValue(user)?.ToString()));
        }

        public bool IsUserExists(string userLogin)
        {
            return DbContext.Users.Any(i=>i.Login == userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = getUser(userLogin);
            if(user != null)
            {
                UserBuilder builder = new(DbContext, user);
                UpdateUserProperties(properties, builder);
            }
        }

        private void UpdateUserProperties(IEnumerable<UserProperty> properties, UserBuilder builder)
        {
            foreach (var property in properties)
            {
                InvokeWithLogger(() =>
                {
                    switch (property.Name)
                    {
                        case "ItRole":
                            builder.AddItRole(property.Value);
                            break;
                        case "RequestRight":
                            builder.AddRequestRight(property.Value);
                            break;
                        default:
                            builder.AddProperty(property.Name, property.Value);
                            break;
                    }
                });
            }
            Logger.Debug("update properties: " + string.Join(", ", properties
                .Select(i=>i.Name + "=" + i.Value)) + $" to {builder.Build().Login}");
        }


        public IEnumerable<Permission> GetAllPermissions()
        {
            return DbContext.RequestRights
                .Select(i => new Permission(i.Id.ToString(), i.Name, "Request right"))
                .Union(
                    DbContext.ItRoles
                    .Select(i => new Permission(i.Id.ToString(), i.Name, "It role"))
                );
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = getUser(userLogin);
            if(user != null)
            {
                user.RequestRights = user.RequestRights
                    .Union(DbContext.RequestRights
                    .Where(i => rightIds.Contains(i.Name)))
                    .Distinct()
                    .ToList();

                user.Roles = user.Roles
                    .Union(DbContext.ItRoles
                    .Where(i => rightIds.Contains(i.Name)))
                    .Distinct()
                    .ToList();
                Logger.Debug("add rights: " + string.Join(", ", rightIds) + $" to {userLogin}");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var user = getUser(userLogin);
            if(user != null)
            {
                user.RequestRights = user.RequestRights.Where(i => !rightIds.Contains(i.Name)).ToList();
                DbContext.SaveChanges();
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var  user = getUser(userLogin);
            return user?.RequestRights.Select(i => i.Name);
        }
        
        private User? getUser(string login)
        {
            var user = DbContext.Users.Include(i => i.RequestRights).FirstOrDefault(i => i.Login == login);
            if (user == null)
                Logger?.Warn($"user with login {login} not found");
            return user;
        }

        public ILogger Logger 
        {
            get 
            {
                return DbContext.logger;
            }
            set
            {
                DbContext.logger = value;
            }
        }

        private Context? _dbContext;
        private Context DbContext
        {
            get 
            {
                if (_dbContext == null)
                    throw new NullReferenceException("База данных не инициализирована. Пожалуйста вызовите StartUp для инициализации");
                return _dbContext;
            }
            set 
            {
                _dbContext = value;
            }
        }

        private void InvokeWithLogger(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Logger?.Warn(e.Message);
            }
        }
    }
}