using System.Data.Common;
using Task.Connector.Helpers;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataContext _dataContext;
        
        public ILogger Logger { get; set; }
        
        public void StartUp(string connectionString)
        {
            var dbConnectionStringBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };
            
            var contextFactory = new DbContextFactory(dbConnectionStringBuilder["ConnectionString"].ToString());
            
            _dataContext = contextFactory.GetContext(dbConnectionStringBuilder["Provider"].ToString().GetProvider());
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                Logger.Debug($"Попытка создать пользователя с логином: {user.Login}");

                if (IsUserExists(user.Login))
                    throw new ArgumentException($"Пользователь с логином {user.Login} уже существует");
                
                _dataContext.Users.Add(user.CreateUser());

                _dataContext.Passwords.Add(new Sequrity
                {
                    UserId = user.Login,
                    Password = user.HashPassword
                });

                _dataContext.SaveChanges();

                Logger.Debug($"Пользователь с логином {user.Login} был создан");
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось создать пользователя с логином {user.Login}, с ошибкой: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Попытка вернуть все свойства");
            
            yield return new Property("FirstName", "Имя");

            yield return new Property("MiddleName", "Среднее имя");

            yield return new Property("LastName", "Фамилия");

            yield return new Property("TelephoneNumber", "Номер телефона");

            yield return new Property("IsLead", "Является ли руководителем");

            yield return new Property("HashPassword", "Пароль");
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                Logger.Debug($"Попытка вернуть свойства пользователя с логином: {userLogin}");
                
                var user = _dataContext.Users.FirstOrDefault(p => p.Login == userLogin)
                           ?? throw new NullReferenceException($"Не удалось найти пользователя с логином: {userLogin}");

                return typeof(User).GetProperties()
                    .Where(p => p.Name != "Login")
                    .Select(p => new UserProperty(p.Name, p.GetValue(user)?.ToString() ?? string.Empty));
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось вернуть свойства пользователя с логином {userLogin}, с ошибкой: {ex.Message}");
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                Logger.Debug($"Проверка существования пользователя по логину: {userLogin}");

                return _dataContext.Users.Any(p => p.Login == userLogin);
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось найти пользователя с логином {userLogin}, с ошибкой: {ex.Message}");
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                Logger.Debug($"Попытка обновить данные пользователся с логином: {userLogin}");
                
                var user = _dataContext.Users.FirstOrDefault(p => p.Login == userLogin)
                           ?? throw new NullReferenceException($"Не удалось найти пользователя с логином: {userLogin}");

                foreach (var property in properties)
                {
                    switch (property.Name)
                    {
                        case "FirstName":
                            user.FirstName = property.Value;
                            break;
                        case "MiddleName":
                            user.MiddleName = property.Value;
                            break;
                        case "LastName":
                            user.LastName = property.Value;
                            break;
                        case "TelephoneNumber":
                            user.TelephoneNumber = property.Value;
                            break;
                        case "IsLead":
                            user.IsLead = bool.Parse(property.Value);
                            break;
                    }
                }

                _dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось обновить данные пользователя с логином {userLogin} с ошибкой: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                Logger.Debug("Попытка вернуть все права пользователя");

                var rolesPermissions = _dataContext.ITRoles
                    .Select(p => new Permission(p.Id.ToString(), p.Name, "ITRole permission"))
                    .ToList();

                var rightsPermissions = _dataContext.RequestRights
                    .Select(p => new Permission(p.Id.ToString(), p.Name, "Rights permission"))
                    .ToList();

                return rightsPermissions.Concat(rolesPermissions);
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось вернуть права пользователя с ошибкой: {ex.Message}");
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger.Debug($"Попытка добавить права пользователю с логином: {userLogin}");
                foreach (var rightId in rightIds)
                {
                    var rightIdArray = rightId.Split(":");

                    if (rightIdArray[0] == "Role")
                    {
                        _dataContext.UserITRoles.Add(new UserITRole
                        {
                            UserId = userLogin,
                            RoleId = int.Parse(rightIdArray[1])
                        });
                    }
                    else
                    {
                        _dataContext.UserRequestRights.Add(new UserRequestRight
                        {
                            UserId = userLogin,
                            RightId = int.Parse(rightIdArray[1])
                        });
                    }
                }

                _dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось добавить права пользователю с логином {userLogin}, с ошибкой: {ex.Message}");
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger.Debug($"Попытка удалить права с пользователя с логином: {userLogin}");
                
                foreach (var rightId in rightIds)
                {
                    var rightIdArray = rightId.Split(":");

                    if (rightIdArray[0] == "Role")
                    {
                        var roleToRemove = _dataContext.UserITRoles.FirstOrDefault(p =>
                            p.UserId == userLogin &&
                            p.RoleId == int.Parse(rightIdArray[1]));

                        if(roleToRemove is not null)
                            _dataContext.UserITRoles.Remove(roleToRemove);
                    }
                    else
                    {
                        var rightToRemove = _dataContext.UserRequestRights.FirstOrDefault(p =>
                            p.UserId == userLogin &&
                            p.RightId == int.Parse(rightIdArray[1]));

                        if(rightToRemove is not null)
                            _dataContext.UserRequestRights.Remove(rightToRemove);
                    }

                    _dataContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось удалить права у пользователя с логином {userLogin}, с ошибкой: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                Logger.Debug($"Попытка вернуть права пользователя с логином: {userLogin}");
                
                if (!IsUserExists(userLogin))
                    throw new NullReferenceException($"Не удалось найти пользователя с логином: {userLogin}");

                var itRoles = _dataContext.UserITRoles
                    .Where(p => p.UserId == userLogin)
                    .Select(p => p.RoleId.ToString())
                    .ToList();

                var requestRights = _dataContext.UserRequestRights
                    .Where(p => p.UserId == userLogin)
                    .Select(p => p.RightId.ToString())
                    .ToList();

                return requestRights.Concat(itRoles);
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось вернуть права пользователя с логином {userLogin}, с ошибкой: {ex.Message}");
                throw;
            }
        }
    }
}