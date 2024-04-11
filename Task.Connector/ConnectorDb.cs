using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.DataModels;

namespace Task.Connector
{
    public interface IMyConnector : IConnector, IDisposable {}
    
    public class ConnectorDb : IMyConnector
    {
        public ILogger Logger { get; set; }

        private AppDatabaseContext _context;

        public void StartUp(string connectionString)
        {
            _context = new AppDatabaseContext(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            _context.Users.Add(new UserDataModel
            {
                Login = user.Login,
                FirstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "firstName",
                LastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "lastName",
                MiddleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "middleName",
                TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ??
                                  "telephoneNumber",
                IsLead = user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value == "true",
            });

            _context.Passwords.Add(new PasswordDataModel
            {
                UserId = user.Login,
                Password = user.HashPassword,
            });

            _context.SaveChanges();

            Logger.Warn($"A new user with login {user.Login} has been created!");
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var userEntityType = _context.Model.FindEntityType(typeof(UserDataModel));

            var userProperties = userEntityType!.GetProperties()
                .Where(t => !t.IsPrimaryKey())
                .Select(t =>
                {
                    var descriptionAttribute = t.PropertyInfo?.GetCustomAttribute<DescriptionAttribute>();

                    return new Property(
                        t.GetColumnName(),
                        descriptionAttribute is not null ? descriptionAttribute.Description : string.Empty
                    );
                });

            var passwordEntityType = _context.Model.FindEntityType(typeof(PasswordDataModel));

            var passwordProperty = passwordEntityType!.GetProperties()
                .First(p => p.Name == "Password");

            return userProperties.Union(new[]
                {
                    new Property(
                        passwordProperty.GetColumnName(),
                        passwordProperty.PropertyInfo!.GetCustomAttribute<DescriptionAttribute>()!.Description)
                }
            );
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);

            if (user == null)
            {
                Logger.Error($"User with login {userLogin} not found");

                return Enumerable.Empty<UserProperty>();
            }

            var userEntityType = _context.Model.FindEntityType(typeof(UserDataModel));
            var userProperties = userEntityType!.GetProperties()
                .Where(t => !t.IsPrimaryKey())
                .Select(p => new UserProperty(
                    p.GetColumnName(),
                    typeof(UserDataModel).GetProperty(p.Name)!.GetValue(user)!.ToString()!)
                );

            Logger.Warn($"The properties of the user with login {userLogin} were obtained.");
            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            return _context.Users.Any(user => user.Login == userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var userToUpdate = _context.Users.FirstOrDefault(u => u.Login == userLogin);
            properties.ToList().ForEach(p =>
            {
                var userEntityType = _context.Model.FindEntityType(typeof(UserDataModel));
                var property = userEntityType!
                    .GetProperties()
                    .FirstOrDefault(u => u.GetColumnName() == p.Name);

                if (property is null)
                {
                    Logger.Error($"Property is null");

                    throw new NullReferenceException("Property is null");
                }

                property.PropertyInfo!.SetValue(userToUpdate, p.Value);
            });

            _context.SaveChanges();

            Logger.Warn("Properties was updates");
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var requestRights = _context.RequestRights.ToList();
            var itRoles = _context.ItRoles.ToList();

            var requestRightsPermission = requestRights.Select(requestRight => new Permission
            (
                Guid.NewGuid().ToString(),
                requestRight.Name,
                "From requestRights"
            ));

            var itRolesPermission = itRoles.Select(requestRight => new Permission
            (
                Guid.NewGuid().ToString(),
                requestRight.Name,
                "From itRoles"
            ));

            Logger.Warn("All permissions have been obtained.");

            return requestRightsPermission.Union(itRolesPermission);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            foreach (var rightId in rightIds)
            {
                if (int.TryParse(rightId.Split(":").Skip(1).First(), out var numericRightId))
                {
                    var isRecordAlreadyExist = _context.UserRequestRights
                        .Any(userRequestRight =>
                            userRequestRight.UserId == userLogin && userRequestRight.RightId == numericRightId
                        );

                    if (!isRecordAlreadyExist)
                    {
                        _context.UserRequestRights.Add(new UserRequestRightDataModel
                        {
                            UserId = userLogin,
                            RightId = numericRightId
                        });
                    }

                    _context.UserITRoles.Add(new UserITRoleDataModel
                    {
                        UserId = userLogin,
                        RoleId = numericRightId
                    });
                }
                else
                {
                    Logger.Error("Can't parse rightId properly.");

                    throw new ArgumentException("Can't parse rightId properly.");
                }
            }

            _context.SaveChanges();

            Logger.Warn("User permissions have been added.");
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var userRequestRights = rightIds
                .Select(rightId => new UserRequestRightDataModel
                {
                    UserId = userLogin,
                    RightId = int.Parse(rightId
                        .Split(":")
                        .Skip(1)
                        .First()
                    )
                });

            _context.UserRequestRights.RemoveRange(userRequestRights);

            _context.SaveChanges();

            Logger.Warn("All user permissions have been deleted.");
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var userPermission = _context.UserRequestRights
                .Where(userRequestRight => userRequestRight.UserId == userLogin)
                .Join(
                    _context.RequestRights,
                    userRequestRight => userRequestRight.RightId,
                    requestRight => requestRight.Id,
                    (userRequestRight, requestRight) => requestRight.Name
                );

            Logger.Warn("All user permissions have been obtained.");

            return userPermission;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
    }
}