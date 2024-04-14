using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.ComponentModel.DataAnnotations;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private string requestRightGroupName { get; } = "Request";
        private string itRoleRightGroupName { get; } = "Role";
        private string m_ConnectionString;
        private DataContext m_Context;
        public Exception m_LastException { get; set; }
        public void StartUp(string _connectionString)
        {
            try
            {
                Logger.Debug($"StartUp - Init");

                m_ConnectionString = ConnectorHelper.DefaultConnectionString(_connectionString);
                if (m_ConnectionString == null)
                {
                    throw new Exception($"Нарушена целостность строки подключения!\r\nУбедитесь в правильности строки подключения\r\n\r\n{ConnectorHelper.LastException.Message}\r\n\r\n{ConnectorHelper.LastException.StackTrace}");
                }
                string provider = ConnectorHelper.GetProvider(_connectionString);
                if (provider == null)
                {
                    throw new Exception($"Нарушена целостность строки подключения!\r\nУбедитесь в правильности строки подключения\r\n\r\n{ConnectorHelper.LastException.Message}\r\n\r\n{ConnectorHelper.LastException.StackTrace}");
                }
                m_Context = new DbContextFactory(m_ConnectionString).GetContext(provider);

                Logger.Debug($"StartUp - Successfull");
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
            }
        }

        public void CreateUser(UserToCreate _user)
        {
            try
            {
                Logger.Debug($"CreateUser - Init");

                if (_user is null || string.IsNullOrWhiteSpace(_user.Login) || string.IsNullOrEmpty(_user.Login))
                {
                    throw new Exception($"Отсутствует информация о пользователе");
                }
                var password = new Sequrity()
                {
                    UserId = _user.Login,
                    Password = _user.HashPassword,
                };

                User newUser = new User();

                newUser.Login = _user.Login;
                newUser.FirstName = _user.Properties.FirstOrDefault(x => x.Name.ToLower() == nameof(newUser.FirstName).ToLower())?.Value ?? "Test1";
                newUser.MiddleName = _user.Properties.FirstOrDefault(x => x.Name.ToLower() == nameof(newUser.MiddleName).ToLower())?.Value ?? "Test2";
                newUser.LastName = _user.Properties.FirstOrDefault(x => x.Name.ToLower() == nameof(newUser.LastName).ToLower())?.Value ?? "Test3";
                newUser.TelephoneNumber = _user.Properties.FirstOrDefault(x => x.Name.ToLower() == nameof(newUser.TelephoneNumber).ToLower())?.Value ?? "TestPhone";
                newUser.IsLead = _user.Properties.FirstOrDefault(x => x.Name.ToLower() == nameof(newUser.IsLead).ToLower())?.Value.ToLower() == "true";

                m_Context.Users.Add(newUser);
                m_Context.Passwords.Add(password);
                m_Context.SaveChanges();

                Logger.Debug($"CreateUser - Successfull");
            }
            catch(Exception ex) 
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                Logger.Debug($"GetAllProperties - Init");

                var allProperties = typeof(User).GetProperties().Where(x => x.Name != nameof(User.Login));
                var allPropertiesName = allProperties.Select(x => new Property(x.Name, string.Empty))
                    .Append(new Property(nameof(Sequrity.Password), string.Empty));

                Logger.Debug($"GetAllProperties - Successfull");
                return allPropertiesName;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
                return null;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string _userLogin)
        {
            try
            {
                Logger.Debug($"GetUserProperties - Init");

                var user = m_Context.Users.AsNoTracking().FirstOrDefault(x => x.Login == _userLogin);
                if (user is null)
                {
                    throw new Exception($"Пользователь с данным логином не найден\r\n");
                }
                
                var allProperties = typeof(User).GetProperties().Where(x => x.Name != nameof(User.Login));
                var resultProperties = new List<UserProperty>();

                foreach (var item in allProperties)
                {
                    resultProperties.Add(new UserProperty(item.Name, item.GetValue(user)?.ToString() ?? ""));
                }

                Logger.Debug($"GetUserProperties - Successfull");
                return resultProperties;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
                return null;
            }
        }

        public bool IsUserExists(string _userLogin)
        {
            try
            {
                Logger.Debug($"IsUserExists - Init");

                bool result = m_Context.Users.AsNoTracking().Any(u => u.Login == _userLogin);

                Logger.Debug($"IsUserExists - Successfull");
                return result;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> _properties, string _userLogin)
        {
            try
            {
                Logger.Debug($"UpdateUserProperties - Init");

                var user = m_Context.Users.FirstOrDefault(x => x.Login == _userLogin);
                if (user is null)
                {
                    throw new Exception($"Пользователь с данным логином не найден\r\n");
                }

                user.FirstName = _properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.FirstName).ToLower())?.Value ?? user.FirstName;
                user.MiddleName = _properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.MiddleName).ToLower())?.Value ?? user.MiddleName;
                user.LastName = _properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.LastName).ToLower())?.Value ?? user.LastName;
                user.TelephoneNumber = _properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.TelephoneNumber).ToLower())?.Value ?? user.TelephoneNumber;
                string IsLead = user.IsLead.ToString().ToLower();
                IsLead = _properties.FirstOrDefault(x => x.Name.ToLower() == nameof(user.IsLead).ToLower())?.Value.ToLower() ?? IsLead;
                user.IsLead = IsLead == "true"
                    ;

                m_Context.SaveChanges();

                Logger.Debug($"UpdateUserProperties - Successfull");
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                Logger.Debug($"GetAllPermissions - Init");

                var requestRights = m_Context.RequestRights.AsNoTracking().Select(x=> new Permission(x.Id!.Value.ToString() ?? "", x.Name, string.Empty)).ToList();
                var itRoles = m_Context.ITRoles.AsNoTracking().Select(x => new Permission(x.Id!.Value.ToString() ?? "", x.Name, string.Empty)).ToList();
                var resultPermissions = requestRights.Union(itRoles);

                Logger.Debug($"GetAllPermissions - Successfull");
                return resultPermissions;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
                return null;
            }
        }

        public void AddUserPermissions(string _userLogin, IEnumerable<string> _rightIds)
        {
            try
            {
                Logger.Debug($"AddUserPermissions - Init");

                var user = m_Context.Users.FirstOrDefault(x => x.Login == _userLogin);
                if (user is null)
                {
                    throw new Exception($"Пользователь с данным логином не найден\r\n");
                }

                if (!_rightIds.Any())
                {
                    throw new Exception($"Не задан список прав\r\n");
                }
                foreach (var item in _rightIds)
                {
                    var rightId = item.Split(':');
                    if (!rightId.Any() || rightId.Length != 2)
                    {
                        Logger.Warn($"AddUserPermissions - Warning\r\nРоль/Право: {item} имеет не верный формат и будет пропущена");
                        continue;
                    }        
                    if (rightId[0].Equals(this.itRoleRightGroupName))
                    {
                        if (!m_Context.UserITRoles.Any(x => x.RoleId == Convert.ToInt32(rightId[1].Trim()) && x.UserId == _userLogin))
                        {
                            m_Context.UserITRoles.Add(new() { RoleId = Convert.ToInt32(rightId[1].Trim()), UserId = _userLogin });
                        }
                        else
                        {
                            Logger.Warn($"AddUserPermissions - Warning\r\nРоль: {rightId[1].Trim()} уже существует и будет пропущена");
                        }
                    }
                    else
                    {
                        if (!m_Context.UserRequestRights.Any(x => x.RightId == Convert.ToInt32(rightId[1].Trim()) && x.UserId == _userLogin))
                        {
                            m_Context.UserRequestRights.Add(new() { RightId = Convert.ToInt32(rightId[1].Trim()), UserId = _userLogin });
                        }
                        else
                        {
                            Logger.Warn($"AddUserPermissions - Warning\r\nПраво: {rightId[1].Trim()} уже существует и будет пропущено");
                        }
                    }
                }
                m_Context.SaveChanges();

                Logger.Debug($"AddUserPermissions - Successfull");

            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
            }
        }

        public void RemoveUserPermissions(string _userLogin, IEnumerable<string> _rightIds)
        {
            try
            {
                var user = m_Context.Users.AsNoTracking().FirstOrDefault(x => x.Login == _userLogin);
                if (user is null)
                {
                    throw new Exception($"Пользователь с данным логином не найден\r\n");
                }

                if (!_rightIds.Any())
                {
                    throw new Exception($"Не задан список прав\r\n");
                }
                foreach (var item in _rightIds)
                {
                    var rightId = item.Split(':');
                    if (!rightId.Any() || rightId.Length != 2)
                    {
                        Logger.Warn($"RemoveUserPermissions - Warning\r\nРоль/Право: {item} имеет не верный формат и будет пропущена");
                        continue;
                    }
                    if (rightId[0].Equals(this.itRoleRightGroupName))
                    {
                        if (m_Context.UserITRoles.Any(x => x.RoleId == Convert.ToInt32(rightId[1].Trim()) && x.UserId == _userLogin))
                        {
                            m_Context.UserITRoles.Remove(new UserITRole() { RoleId = Convert.ToInt32(rightId[1]), UserId = _userLogin });
                        }
                        else
                        {
                            Logger.Warn($"RemoveUserPermissions - Warning\r\nРоль: {rightId[1].Trim()} не найдена");
                        }
                    }
                    else
                    {
                        if (m_Context.UserRequestRights.Any(x => x.RightId == Convert.ToInt32(rightId[1].Trim()) && x.UserId == _userLogin))
                        {
                            m_Context.UserRequestRights.Remove(new UserRequestRight() { RightId = Convert.ToInt32(rightId[1]), UserId = _userLogin });
                        }
                        else
                        {
                            Logger.Warn($"RemoveUserPermissions - Warning\r\nПраво: {rightId[1].Trim()} не найдено");
                        }
                    }
                }
                m_Context.SaveChanges();
                Logger.Debug($"RemoveUserPermissions - Successfull");
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string _userLogin)
        {
            try
            {
                Logger.Debug($"GetUserPermissions - Init");

                var user = m_Context.Users.AsNoTracking().FirstOrDefault(x => x.Login == _userLogin);
                if (user is null)
                {
                    throw new Exception($"Пользователь с данным логином не найден\r\n");
                }

                var userRequestRights = m_Context.UserRequestRights.AsNoTracking()
                    .Where(x => x.UserId == _userLogin)
                    .Select(x => x.RightId.ToString());

                var userItRoles = m_Context.UserITRoles.AsNoTracking()
                    .Where(x => x.UserId == _userLogin)
                    .Select(x => x.RoleId.ToString());

                var resultPermissions = userRequestRights.Union(userItRoles);

                Logger.Debug($"GetUserPermissions - Successfull");
                return resultPermissions;
            }
            catch (Exception ex)
            {
                m_LastException = ex;
                Logger.Error($"{m_LastException.Message}\r\n\r\n{m_LastException.StackTrace}");
                return null;
            }
        }

        public ILogger Logger { get; set; }
    }
}