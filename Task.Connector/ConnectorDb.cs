using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Xml.Linq;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DataManager _dataManager;

        private DbContextFactory _dbContextFactory;

        private DataContext _context;

        public ILogger Logger { get; set; }

        public ConnectorDb()
        {
            
        }

        public void StartUp(string connectionString)
        {
            _dbContextFactory = new DbContextFactory(connectionString);
            _dataManager = new DataManager(_dbContextFactory, "POSTGRE");
            _context = _dbContextFactory.GetContext("POSTGRE");
        }

        public void CreateUser(UserToCreate user)
        {
            if (IsUserExists(user.Login))
            {
                Logger.Error("Пользователь уже существует");
                return;
            }    

            var properties = user.Properties;

            var userLogin = user.Login;
            var userFirstName = properties.Where(p => p.Name == nameof(User.FirstName)).FirstOrDefault()?.Value;
            var userLastName = properties.Where(p => p.Name == nameof(User.LastName)).FirstOrDefault()?.Value;
            var userMiddleName = properties.Where(p => p.Name == nameof(User.MiddleName)).FirstOrDefault()?.Value;
            var userIsLeadParsingResult = bool.TryParse(properties.Where(p => p.Name == nameof(User.IsLead)).FirstOrDefault()?.Value, out var userIsLead);
            var userTelephoneNumber = properties.Where(p => p.Name == nameof(User.TelephoneNumber)).FirstOrDefault()?.Value;

            var newUser = new User()
            {
                Login = userLogin,
                FirstName = userFirstName ?? "",
                LastName = userLastName ?? "",
                MiddleName = userMiddleName ?? "",
                IsLead = userIsLeadParsingResult ? userIsLead : false,
                TelephoneNumber = userTelephoneNumber ?? ""
            };

            try
            {
                _context.Passwords.Add(new Sequrity()
                {
                    UserId = user.Login,
                    Password = user.HashPassword
                });

                _context.Users.Add(newUser);

                _context.SaveChanges();

                Logger.Debug($"Пользователь {userLogin} добавлен в БД");
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось добавить пользователя в БД " + ex.Message);
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Получение всех свойств пользователей");
           
            return new List<Property>
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
            Logger.Debug($"Получение свойств пользователя {userLogin}");

            try
            {
                var user = _dataManager.GetUser(userLogin);

                return new List<UserProperty> 
                { 
                    new UserProperty(nameof(User.LastName), user.LastName),
                    new UserProperty(nameof(User.FirstName), user.FirstName),
                    new UserProperty(nameof(User.MiddleName), user.MiddleName),
                    new UserProperty(nameof(User.TelephoneNumber), user.TelephoneNumber),
                    new UserProperty(nameof(User.IsLead), user.IsLead.ToString())
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось получить свойства пользователя {userLogin} " + ex.Message);
                throw;
            }
            finally
            {
                Logger.Debug($"Свойства пользователя {userLogin} получены");
            }
        }

        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Проверка существования пользователя {userLogin}");

            try
            {
                return _context.Users.Any(x => x.Login == userLogin);
            }
            catch (Exception ex)
            {
                Logger.Error($"Проверка существования пользователя {userLogin} не прошла успешно " + ex.Message);
                throw;
            }
            finally
            {
                Logger.Debug($"Проверка существования пользователя {userLogin} прошла успешно");
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = _context.Users.FirstOrDefault((User _) => _.Login.Equals(userLogin));

                foreach (var property in properties)
                {
                    switch (property.Name)
                    {
                        case nameof(User.TelephoneNumber):
                            user.TelephoneNumber = property.Value;
                            break;
                        case nameof(User.FirstName):
                            user.FirstName = property.Value;
                            break;
                        case nameof(User.MiddleName):
                            user.MiddleName = property.Value;
                            break;
                        case nameof(User.LastName):
                            user.LastName = property.Value;
                            break;
                        case nameof(User.IsLead):
                            user.IsLead = bool.Parse(property.Value);
                            break;
                        case nameof(Sequrity.Password):
                            _context.Passwords.FirstOrDefault((Sequrity _) => _.UserId.Equals(userLogin)).Password = property.Value;
                            break;
                    };
                }
                _context.SaveChanges();

                Logger.Debug($"Обновлены свойства пользователя {userLogin}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось обновить свойства пользователя {userLogin}" + ex.Message);
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug("Получение всех прав пользователей");

            try
            {
                var itRoles = _context.ITRoles;
                var rights = _context.RequestRights;

                var x = itRoles.Select(r => new Permission(r.Id.ToString(), r.Name, r.CorporatePhoneNumber)).ToList();
                var y = rights.Select(r => new Permission(r.Id.ToString(), r.Name, "")).ToList();

                return x.Concat(y);
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось получить все права пользователей " + ex.Message);
                throw;
            }
            finally
            {
                Logger.Debug("Все права пользователей получены");
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Получение всех прав пользователя {userLogin}");

            var ITRolesToAdd = rightIds.Where(r => r.Contains("Role:")).Select(r => r.Replace("Role:", "")).ToArray();
            var RequestRightsToAdd = rightIds.Where(r => r.Contains("Request:")).Select(r => r.Replace("Request:", "")).ToArray();

            try
            {
                _context.UserITRoles.AddRange(ITRolesToAdd.Select(r => new UserITRole()
                {
                    RoleId = int.Parse(r),
                    UserId = userLogin
                }));

                _context.UserRequestRights.AddRange(RequestRightsToAdd.Select(r => new UserRequestRight()
                {
                    RightId = int.Parse(r),
                    UserId = userLogin
                }));

                _context.SaveChanges();

                Logger.Debug($"Все права пользователя {userLogin} получены"); ;
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось получить все права пользователя {userLogin} " + ex.Message);
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Удаление прав пользователя {userLogin}");

            var ITRolesToRemove = rightIds.Where(r => r.Contains("Role:")).Select(r => r.Replace("Role:", "")).ToArray();
            var RequestRightsToRemove = rightIds.Where(r => r.Contains("Request:")).Select(r => r.Replace("Request:", "")).ToArray();

            try
            {
                var ITRoles = _context.UserITRoles.Where((UserITRole _) => _.UserId.Equals(userLogin)).ToArray();
                var CRequestRights = _context.UserRequestRights.Where((UserRequestRight _) => _.UserId.Equals(userLogin)).ToArray();

                _context.UserRequestRights.RemoveRange(CRequestRights.Where(r => RequestRightsToRemove.Contains(r.RightId.ToString())));
                _context.UserITRoles.RemoveRange(ITRoles.Where(r => ITRolesToRemove.Contains(r.RoleId.ToString())));

                _context.SaveChanges();

                Logger.Debug($"Права пользователя {userLogin} удалены");
            }
            catch (Exception ex)
            {
                Logger.Error($"Не удалось удалить права пользователя {userLogin}");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Получение всех прав пользователя {userLogin}");

            try
            {
                var ITRoles = _context.UserITRoles.Where((UserITRole _) => _.UserId.Equals(userLogin)).Select(r => r.RoleId.ToString()).ToList();

                var CRequestRights = _context.UserRequestRights.Where((UserRequestRight _) => _.UserId.Equals(userLogin)).Select(r => r.RightId.ToString()).ToList();

                return ITRoles.Concat(CRequestRights);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Не удалось получить все права пользователя {userLogin} " + ex.Message);
                throw;
            }
            finally 
            {
                Logger.Debug($"Все права пользователя {userLogin} получены");
            }
        }
    }
}