using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Task.Connector.Helpers.Permission;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services
{
    public interface IUserService
    {
        bool IsUserExists(string userLogin);
        void CreateUser(UserToCreate user);
        IEnumerable<UserProperty> GetUserProperties(string userLogin);
        void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
        IEnumerable<string> GetUserPermissions(string userLogin);
        void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
        void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);

    }
    internal class UserService : BaseService, IUserService
    {
        public UserService(DataContext context) : base(context) { }

        public bool IsUserExists(string userLogin)
        {
            return Context.Users.Any(u => u.Login == userLogin);
        }
        public void CreateUser(UserToCreate user)
        {
            if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.HashPassword))
                throw new Exception("Values must be not null");

            var newUser = new User() { Login = user.Login };

            foreach (var newUserProperty in typeof(User).GetProperties())
            {
                if (newUserProperty.Name == "Login") //изменить: не имя свойства, а ключевой атрибут
                    continue;
                var propertyFound = user.Properties.Where(x => x.Name.ToLowerInvariant() == newUserProperty.Name.ToLowerInvariant()).FirstOrDefault();
                object value = newUserProperty.PropertyType.Name == "Boolean" ? false : "";
                if (propertyFound != null)
                    value = Convert.ChangeType(propertyFound.Value, newUserProperty.PropertyType);
                newUserProperty.SetValue(newUser, value);
            }
            using (var transaction = Context.Database.BeginTransaction())
            {
                Context.Users.Add(newUser);
                Context.SaveChanges();

                Context.Passwords.Add(new Sequrity { UserId = user.Login, Password = user.HashPassword });
                Context.SaveChanges();

                transaction.Commit();
            }
        }
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (!IsUserExists(userLogin))
                throw new Exception("Not found");

            var user = Context.Users.Where(x => x.Login == userLogin).First();
            var userProps = user.GetType().GetProperties()
                .Where(prop => prop.CanRead && prop.Name != "Login")
                .Select(prop => new UserProperty(prop.Name, prop.GetValue(user).ToString()));

            var result = userProps;
            return result;
        }
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!IsUserExists(userLogin))
                throw new Exception("Not found");
            using (var transaction = Context.Database.BeginTransaction())
            {
                var user = Context.Users.Where(x => x.Login == userLogin).First();
                foreach (var userProperty in user.GetType().GetProperties())
                {
                    if (userProperty.Name == "Login")
                        continue;

                    var propertyFound = properties.Where(x =>
                        x.Name.ToLowerInvariant() == userProperty.Name.ToLowerInvariant()).FirstOrDefault();
                    if (propertyFound != null)
                    {
                        object value = Convert.ChangeType(propertyFound.Value, userProperty.PropertyType);
                        userProperty.SetValue(user, value);
                    }
                }
                Context.Update(user);
                Context.SaveChanges();

                transaction.Commit();
            }
        }
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var rights = Context.UserRequestRights.Where(x => x.UserId == userLogin)
                .Select(x => $"{{\"RequestRight\": \"{x.RightId}\"}}").ToList();
            var roles = Context.UserITRoles.Where(x => x.UserId == userLogin)
                .Select(x => $"{{\"ITRole\": \"{x.RoleId}\"}}").ToList();
            return rights.Concat(roles);
        }
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var parsedPermissions = PermissionHelper.PreparePermissions
                (PermissionHelper.ParsePermissionStrings(rightIds), userLogin);
            using (var transaction = Context.Database.BeginTransaction())
            {
                foreach (var right in parsedPermissions.rights)
                    if (!Context.UserRequestRights.Contains(right))
                        Context.UserRequestRights.Add(right);
                Context.SaveChanges();

                foreach (var role in parsedPermissions.roles)
                    if (!Context.UserITRoles.Contains(role))
                        Context.UserITRoles.Add(role);
                Context.SaveChanges();

                transaction.Commit();
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var parsedPermissions = PermissionHelper.PreparePermissions
                (PermissionHelper.ParsePermissionStrings(rightIds), userLogin);
            using (var transaction = Context.Database.BeginTransaction())
            {
                foreach (var right in parsedPermissions.rights)
                    if (Context.UserRequestRights.Contains(right))
                        Context.UserRequestRights.Remove(right);
                Context.SaveChanges();

                foreach (var role in parsedPermissions.roles)
                    if (Context.UserITRoles.Contains(role))
                        Context.UserITRoles.Remove(role);
                Context.SaveChanges();

                transaction.Commit();
            }
        }
    }
}
