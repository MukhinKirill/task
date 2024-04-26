using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _db;

        public void StartUp(string connectionString)
        {
            var connectionInfo = new ConnectionInfo(connectionString);
            var dbContextFactory = new DbContextFactory(connectionInfo.ConnectionString);
            _db = dbContextFactory.GetContext(connectionInfo.Provider);
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                Logger.Debug($"Добавление пользователя с логином {user.Login}");

                if (IsUserExists(user.Login))
                {
                    Logger.Warn($"Пользователь с логином {user.Login} уже существует");
                    return;
                }

                _db.Passwords.Add(new Sequrity()
                {
                    Password = user.HashPassword,
                    UserId = user.Login,
                });

                var toAdd = CreateUser(user.Properties, user.Login);

                _db.Users.Add(toAdd);
                _db.SaveChanges();

                Logger.Debug($"Добавлен новый пользователь с логином {toAdd.Login}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Получение списка названий всех свойств");

            return new List<Property>()
            {
                new Property(nameof(User.LastName), ""),
                new Property(nameof(User.FirstName), ""),
                new Property(nameof(User.MiddleName), ""),
                new Property(nameof(User.TelephoneNumber), ""),
                new Property(nameof(User.IsLead), ""),
                new Property(nameof(Sequrity.Password), "")
            };
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                Logger.Debug($"Получение списка значений всех свойств {userLogin}");

                if (!IsUserExists(userLogin))
                {
                    Logger.Warn($"Пользователя с логином {userLogin} не существует");
                    return Enumerable.Empty<UserProperty>();
                }

                var user = _db.Users.FirstOrDefault(x => x.Login == userLogin);

                return new List<UserProperty>()
                {
                    new UserProperty(nameof(User.LastName), user.LastName),
                    new UserProperty(nameof(User.FirstName), user.FirstName),
                    new UserProperty(nameof(User.MiddleName), user.MiddleName),
                    new UserProperty(nameof(User.TelephoneNumber), user.TelephoneNumber),
                    new UserProperty(nameof(User.IsLead), user.IsLead.ToString()),
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                return _db.Users.Any(x => x.Login == userLogin);
            }
            catch(Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                Logger.Debug($"Обновление свойств пользователя с логином {userLogin}");

                if (!IsUserExists(userLogin))
                {
                    Logger.Warn($"Пользователя с логином {userLogin} не существует");
                    return;
                }

                var user = _db.Users.FirstOrDefault(x => x.Login == userLogin);

                UpdateUserProperties(user, properties);

                _db.SaveChanges();

                Logger.Debug($"Обновлены свойства пользователя с логином {user.Login}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var itRoles = _db.ITRoles
                    .Select(x => new Permission(x.Id.ToString(), x.Name, "It role"))
                    .ToList();

                return _db.RequestRights
                    .Select(x => new Permission(x.Id.ToString(), x.Name, "Request right"))
                    .ToList()
                    .Concat(itRoles);
            }
            catch (Exception ex) 
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger.Debug($"Добавление прав для пользователя {userLogin}");

                if (!IsUserExists(userLogin))
                {
                    Logger.Warn($"Пользователя с логином {userLogin} не существует");
                    return;
                }

                foreach (var permissionId in rightIds.Select(x => new PermissionId(x)))
                {
                    if (permissionId.IsRoleId)
                    {
                        AddUserITRole(userLogin, permissionId.Id);
                    }
                    else if (permissionId.IsRequestRightId)
                    {
                        AddUserRequestRight(userLogin, permissionId.Id);
                    }
                }

                _db.SaveChanges();

                Logger.Debug($"Добавлены права для пользователя {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger.Debug($"Удаление прав у пользователя {userLogin}");

                if (!IsUserExists(userLogin))
                {
                    Logger.Warn($"Пользователя с логином {userLogin} не существует");
                    return;
                }

                foreach (var permissionId in rightIds.Select(x => new PermissionId(x)))
                {
                    if (permissionId.IsRoleId)
                    {
                        RemoveUserITRole(userLogin, permissionId.Id);
                    }
                    else if (permissionId.IsRequestRightId)
                    {
                        RemoveUserRequestRight(userLogin, permissionId.Id);
                    }
                }

                _db.SaveChanges();

                Logger.Debug($"Удаление прав у пользователя {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                Logger.Debug($"Получение списка прав пользователя {userLogin}");

                if (!IsUserExists(userLogin))
                {
                    Logger.Warn($"Пользователя с логином {userLogin} не существует");
                    return Enumerable.Empty<string>();
                }

                var permissions = new List<string>();

                var roleIds = _db.UserITRoles
                    .Where(x => x.UserId == userLogin)
                    .Select(x => x.RoleId);

                var roles = _db.ITRoles
                    .Where(x => roleIds.Any(i => i == x.Id))
                    .Select(x => x.Name);

                permissions.AddRange(roles);

                var requestRightIds = _db.UserRequestRights
                    .Where(x => x.UserId == userLogin)
                    .Select(x => x.RightId);

                var requestRights = _db.RequestRights
                    .Where(x => requestRightIds.Any(i => i == x.Id))
                    .Select(x => x.Name);

                permissions.AddRange(requestRights);

                return permissions;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        public ILogger Logger { get; set; }

        #region PrivateMethods
        private User CreateUser(IEnumerable<UserProperty> properties, string login)
        {
            var user = new User();

            user.Login = login;
            user.LastName = GetUserPropertyValue(properties, nameof(user.LastName)) ?? string.Empty;
            user.FirstName = GetUserPropertyValue(properties, nameof(user.FirstName)) ?? string.Empty;
            user.MiddleName = GetUserPropertyValue(properties, nameof(user.MiddleName)) ?? string.Empty;
            user.TelephoneNumber = GetUserPropertyValue(properties, nameof(user.TelephoneNumber)) ?? string.Empty;

            var isLead = GetUserPropertyValue(properties, nameof(user.IsLead));
            user.IsLead = isLead is null ? default(bool) : bool.Parse(isLead);

            return user;
        }

        private void UpdateUserProperties(User user, IEnumerable<UserProperty> properties)
        {
            user.LastName = GetUserPropertyValue(properties, nameof(user.LastName)) ?? user.LastName;
            user.FirstName = GetUserPropertyValue(properties, nameof(user.FirstName)) ?? user.FirstName;
            user.MiddleName = GetUserPropertyValue(properties, nameof(user.MiddleName)) ?? user.MiddleName;
            user.TelephoneNumber = GetUserPropertyValue(properties, nameof(user.TelephoneNumber)) ?? user.TelephoneNumber;

            var isLead = GetUserPropertyValue(properties, nameof(user.IsLead));
            user.IsLead = isLead is null ? user.IsLead : bool.Parse(isLead);
        }

        private string? GetUserPropertyValue(IEnumerable<UserProperty> properties, string name)
        {
            return properties.FirstOrDefault(x => x.Name.ToLower() == name.ToLower())?.Value;
        }

        private bool IsItRoleExists(int itRoleId)
        {
            return _db.ITRoles.Any(x => x.Id == itRoleId);
        }

        private bool IsRequestRightExists(int requestRightId)
        {
            return _db.RequestRights.Any(x => x.Id == requestRightId);
        }

        private void RemoveUserITRole(string userLogin, int roleId)
        {
            var toRemove = _db.UserITRoles
                .FirstOrDefault(x => x.UserId == userLogin && x.RoleId == roleId);

            if (toRemove == null)
            {
                Logger.Warn($"У пользователя с логином {userLogin} нет ItRole с id={roleId}");
            }
            else
            {
                _db.UserITRoles.Remove(toRemove);
            }
        }

        private void RemoveUserRequestRight(string userLogin, int requestRightId)
        {
            var toRemove = _db.UserRequestRights
                .FirstOrDefault(x => x.UserId == userLogin && x.RightId == requestRightId);

            if (toRemove == null)
            {
                Logger.Warn($"У пользователя с логином {userLogin} нет RequestRight c id={requestRightId}");
            }
            else
            {
                _db.UserRequestRights.Remove(toRemove);
            }
        } 

        private void AddUserITRole(string userLogin, int roleId)
        {
            if (!IsItRoleExists(roleId))
            {
                Logger.Warn($"ItRole с id={roleId} не существует");
            }
            else
            {
                var toAdd = new UserITRole
                {
                    RoleId = roleId,
                    UserId = userLogin,
                };

                _db.UserITRoles.Add(toAdd);
            }
        }

        private void AddUserRequestRight(string userLogin, int requestRightId)
        {
            if (!IsRequestRightExists(requestRightId))
            {
                Logger.Warn($"RequestRight с id={requestRightId} не существует");
            }
            else
            {
                var toAdd = new UserRequestRight
                {
                    RightId = requestRightId,
                    UserId = userLogin,
                };

                _db.UserRequestRights.Add(toAdd);
            }
        }
        #endregion
    }
}