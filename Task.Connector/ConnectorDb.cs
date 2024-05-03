using System.Security.Permissions;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.Globalization;
using System.Reflection;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private const string _providerString = "POSTGRE";
        private const string _isLeadPropertyName = "IsLead";
        private const string _requestRightPermissionName = "Request";
        private const string _itRolePermissionName = "Role";

        private DataContext _dbContext;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            var splittedArray = connectionString.Split('\'');
            var updatedConnectionString = splittedArray[1];
            var dbContextFactory = new DbContextFactory(updatedConnectionString);
            _dbContext = dbContextFactory.GetContext(_providerString);
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                if(IsUserExists(user.Login))
                {
                    Logger.Error($"Пользователь с логином {user.Login} уже существует");
                    return;
                }
                var userModel = new User
                {
                    Login = user.Login,
                    LastName = "",
                    MiddleName = "",
                    TelephoneNumber = "",
                    FirstName = "",
                    IsLead = false,
                };
                ChangeUserProperties(userModel, user.Properties);
                var passwordModel = new Sequrity { Password = user.HashPassword, UserId = user.Login };
                _dbContext.Users.Add(userModel);
                _dbContext.Passwords.Add(passwordModel);
                _dbContext.SaveChanges();
                Logger.Debug($"Создание пользователя с логином {user.Login}");
            }
            catch(Exception ex)
            {
                Logger.Warn($"{ex.Message}\n Не удвлось создать пользователя");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                var user = new User();
                var password = new Sequrity();
                var properties = new List<Property>
                {
                    new Property(nameof(user.FirstName), ""),
                    new Property(nameof(user.LastName), ""),
                    new Property(nameof(user.MiddleName), ""),
                    new Property(nameof(user.TelephoneNumber), ""),
                    new Property(nameof(user.IsLead), ""),
                    new Property(nameof(password.Password), "")
                };
                Logger.Debug("Получение свойств");
                return properties;
            }
            catch(Exception ex)
            {
                Logger.Warn($"{ex.Message} \n Ошибка при получении свойств");
                return null;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var user = _dbContext.Users.Where(x => x.Login == userLogin).FirstOrDefault();
                if(user == null)
                    throw new Exception($"Пользователя с логином {userLogin} не существует");
                var password = _dbContext.Passwords.Where(x => x.UserId == userLogin).FirstOrDefault();
                var properties = new List<UserProperty>
                {
                    new UserProperty(nameof(user.FirstName), user.FirstName),
                    new UserProperty(nameof(user.LastName), user.LastName),
                    new UserProperty(nameof(user.MiddleName), user.MiddleName),
                    new UserProperty(nameof(user.TelephoneNumber), user.TelephoneNumber),
                    new UserProperty(nameof(user.IsLead), user.IsLead.ToString()),
                };
                Logger.Debug($"Получение свойств пользователя с логином {userLogin}");
                return properties;
            }
            catch(Exception ex)
            {
                Logger.Warn($"{ex.Message} \n Ошибка при получении свойств пользователя с логином {userLogin}");
                return null;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            var user = _dbContext.Users.Where(x => x.Login == userLogin).FirstOrDefault();
            return user != null;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = _dbContext.Users.Where(x => x.Login == userLogin).FirstOrDefault();
                if(user == null)
                    throw new Exception($"Пользователя с логином {userLogin} не существует");
                ChangeUserProperties(user, properties);
                _dbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                Logger.Warn($"{ex.Message} \n Ошибка при изменении свойств пользователя с логином {userLogin}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var requestRightPermissions = _dbContext.RequestRights.Select(x => new Permission(x.Id.ToString(), x.Name,
                _requestRightPermissionName)).ToList();
                var itRolePermissions = _dbContext.ITRoles.Select(x => new Permission(x.Id.ToString(), x.Name,
                    _itRolePermissionName)).ToList();
                var result = requestRightPermissions.Concat(itRolePermissions).ToList();
                Logger.Debug("Получение списка прав");
                return result;
            }
            catch(Exception ex)
            {
                Logger.Warn($"{ex.Message} \n Не удалось получить список прав");
                return null;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var user = _dbContext.Users.Where(x => x.Login == userLogin).FirstOrDefault();
                if (user == null) throw new Exception($"Пользователя с логином {userLogin} не существует");
                foreach (var rightId in rightIds)
                {
                    var splittedRightId = rightId.Split(':');
                    var permissionId = int.Parse(splittedRightId[1]);
                    switch(splittedRightId[0])
                    {
                        case _requestRightPermissionName:
                            var right = _dbContext.RequestRights.Where(x => x.Id == permissionId).FirstOrDefault();
                            if (right != null)
                            {
                                var userRequest = new UserRequestRight { UserId = userLogin, RightId = permissionId };
                                _dbContext.UserRequestRights.Add(userRequest);
                            }
                            break;
                        case _itRolePermissionName:
                            var role = _dbContext.ITRoles.Where(x => x.Id == permissionId).FirstOrDefault();
                            if (role != null)
                            {
                                var userItRole = new UserITRole { UserId = userLogin, RoleId = permissionId };
                                _dbContext.UserITRoles.Add(userItRole);
                            }
                            break;
                    }
                } 
                _dbContext.SaveChanges();
                Logger.Debug($"Успешное добавление прав для пользователя с логином {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"{ex.Message} \n Не удалось добавить права для пользователя с логином {userLogin}");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var user = _dbContext.Users.Where(x => x.Login == userLogin).FirstOrDefault();
                if (user == null) throw new Exception($"Пользователя с логином {userLogin} не существует");
                foreach (var rightId in rightIds)
                {
                    var splittedRightId = rightId.Split(':');
                    var permissionId = int.Parse(splittedRightId[1]);
                    switch (splittedRightId[0])
                    {
                        case _requestRightPermissionName:
                            var userRequesrRight = _dbContext.UserRequestRights.Where(x => x.UserId == userLogin 
                            && x.RightId == permissionId).FirstOrDefault();
                            if(userRequesrRight != null)
                                _dbContext.UserRequestRights.Remove(userRequesrRight);
                            break;
                        case _itRolePermissionName:
                            var userItRole = _dbContext.UserITRoles.Where(x => x.UserId == userLogin
                            && x.RoleId == permissionId).FirstOrDefault();
                            if (userItRole != null)
                                _dbContext.UserITRoles.Remove(userItRole);
                            break;
                    }
                }
                _dbContext.SaveChanges();
                Logger.Debug($"Успешное удвление прав для пользователя с логином {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"{ex.Message} \n Не удалось удалить права для пользователя с логином {userLogin}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                var requestRightIds = _dbContext.UserRequestRights.Where(x => x.UserId == userLogin).Select(x => x.RightId).ToList();
                var requestRightNames = _dbContext.RequestRights.Where(x => requestRightIds.Contains(x.Id.Value)).Select(
                    x => x.Name).ToList();
                var itRoleIds = _dbContext.UserITRoles.Where(x => x.UserId == userLogin).Select(x => x.RoleId).ToList();
                var itRoleNames = _dbContext.ITRoles.Where(x => itRoleIds.Contains(x.Id.Value)).Select(
                    x => x.Name).ToList();
                var result = requestRightNames.Concat(itRoleNames).ToList();
                Logger.Debug($"Получение списка прав пользователя с логином {userLogin}");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn($"{ex.Message} \n Не удалось получить список прав пользователя с логином {userLogin}");
                return null;
            }
        }

        private void ChangeUserProperties(User user, IEnumerable<UserProperty> userProperties)
        {
            var type = user.GetType();
            foreach (var property in userProperties)
            {
                var propertyInfo = type.GetProperty(property.Name);
                propertyInfo.SetValue(user, property.Name == _isLeadPropertyName ? bool.Parse(property.Value) :
                    property.Value);
            }
        }
    }
}