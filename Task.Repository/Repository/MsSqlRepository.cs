using System.Data;
using System.Reflection;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;
using Microsoft.EntityFrameworkCore;
using Task.Utilities;
using Task.Repository.Interfaces;

namespace Task.Repository.Repository
{
    public class MsSqlRepository : IRepository
    {
        readonly string _providerName = "MSSQL";
        readonly private string _connectionString;
        readonly private DbContextFactory _contextFactory;

        public MsSqlRepository(string connectionString)
        {
            _connectionString = ConnectionStringService.GetConnectionString(connectionString);

            _contextFactory = new DbContextFactory(_connectionString);
        }

        public bool IsUserExists(string userLogin)
        {

            if (userLogin == null) return false;

            using DataContext context = _contextFactory.GetContext(_providerName);

            User user = context.Users.Where(x => x.Login == userLogin).AsNoTracking().FirstOrDefault();

            return user != null;
        }

        public void CreateUser(User user)
        {
            using DataContext context = _contextFactory.GetContext(_providerName);

            context.Users.Add(user);
            context.SaveChanges();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            using DataContext context = _contextFactory.GetContext(_providerName);

            if (!IsUserExists(userLogin)) return new List<UserProperty>();

            User user = context.Users.Where(x => x.Login == userLogin).AsNoTracking().FirstOrDefault();

            IEnumerable<PropertyInfo> props = typeof(User).GetProperties().Where(x => x.Name != nameof(User.Login));

            return props.Select(property =>
            {
                string value = property.GetValue(user)?.ToString() ?? "null";
                return new UserProperty(property.Name, value);
            });
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!IsUserExists(userLogin)) return;

            using DataContext context = _contextFactory.GetContext(_providerName);

            Dictionary<string, PropertyInfo> userProperties = typeof(User).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .ToDictionary(prop => prop.Name, prop => prop);

            User user = context.Users.FirstOrDefault(x => x.Login == userLogin);

            foreach (var property in properties)
            {
                if (userProperties.TryGetValue(property.Name, out PropertyInfo userProperty))
                {
                    userProperty.SetValue(user, Convert.ChangeType(property.Value, userProperty.PropertyType));
                }
            }

            context.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            using DataContext context = _contextFactory.GetContext(_providerName);

            List<Permission> permissions = new List<Permission>();

            List<ITRole> itRoles = context.ITRoles.AsNoTracking().ToList(); ;
            IQueryable<RequestRight> itRights = context.RequestRights.AsNoTracking();

            foreach (var role in itRoles)
            {
                permissions.Add(new Permission(role.Id.ToString(), role.Name, ""));
            }
            foreach (var right in itRights)
            {
                permissions.Add(new Permission(right.Id.ToString(), right.Name, ""));
            }

            return permissions.AsEnumerable();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin)) return;

            using DataContext context = _contextFactory.GetContext(_providerName);

            foreach (var role in rightIds)
            {
                string[] separateString = role.Split(':');
                if (separateString[0] == "Role")
                {
                    ITRole itRole = context.ITRoles.Where(x => x.Id == int.Parse(separateString[1])).AsNoTracking().FirstOrDefault();
                    if (itRole == null) continue;

                    context.UserITRoles.Add(new UserITRole() { RoleId = (int)itRole.Id, UserId = userLogin });
                }
                else if (separateString[0] == "Request")
                {
                    RequestRight right = context.RequestRights.Where(x => x.Id == int.Parse(separateString[1])).AsNoTracking().FirstOrDefault();
                    if (right == null) continue;
                    context.UserRequestRights.Add(new UserRequestRight() { RightId = (int)right.Id, UserId = userLogin });
                }
                else
                {
                    //unknow
                }
                context.SaveChanges();
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin)) return;

            using DataContext context = _contextFactory.GetContext("MSSQL");

            foreach (var role in rightIds)
            {
                var separateString = role.Split(':');
                if (separateString[0] == "Role")
                {
                    UserITRole userItRole = context.UserITRoles.Where(x => x.UserId == userLogin && x.RoleId == int.Parse(separateString[1])).FirstOrDefault();
                    if (userItRole == null) continue;

                    context.Remove(userItRole);
                }
                else if (separateString[0] == "Request")
                {
                    UserRequestRight userRequestRight = context.UserRequestRights.Where(x => x.UserId == userLogin && x.RightId == int.Parse(separateString[1])).FirstOrDefault();
                    if (userRequestRight == null) continue;

                    context.Remove(userRequestRight);
                }
                else
                {
                    //unknow
                }
                context.SaveChanges();
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin)) return new List<string>();

            using DataContext context = _contextFactory.GetContext(_providerName);

            return context.UserITRoles.AsNoTracking().Where(x => x.UserId == userLogin).Select(x => x.RoleId.ToString())
                .Union(context.UserRequestRights.AsNoTracking().Where(x => x.UserId == userLogin).Select(x => x.RightId.ToString())).ToList();

        }
    }
}