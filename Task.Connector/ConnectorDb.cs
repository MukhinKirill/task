using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;
using Task.Connector.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Task.Integration.Data.DbCommon.DbModels;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Diagnostics;
using System.Collections.Generic;


namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {

        private PooledDbContextFactory<DataContext>? _pooledDbContextFactory;
        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
            {
                //Logger.Error("Строка подключения не должна быть пустой");
                //(_pooledDbContextFactory, _dataContextOptions) = (null, null);
                ThrowHelper.ThrowArgumentException(nameof(connectionString), "Строка подключения не должна быть пустой");
            }
            else if (_pooledDbContextFactory == null)
            {
                _pooledDbContextFactory = new PooledDbContextFactory<DataContext>(new DbContextOptionsBuilder<DataContext>()
                            .UseNpgsql(connectionString)
                            .Options);
            }
        }

        public void CreateUser(UserToCreate user)
        {
            if (this.IsUserExists(user.Login))
            {
                Logger.Debug($"Пользователь {user.Login} не будет создан.");
            }
            else
            {
                try
                {
                    using (DataContext dataContext = _pooledDbContextFactory!.CreateDbContext())
                    {

                        User newUser = user.ConvertToUser();
                        dataContext.Users.Add(newUser);

                        Sequrity password = new Sequrity { UserId = user.Login, Password = user.HashPassword };
                        dataContext.Passwords.Add(password);

                        dataContext.SaveChanges();
                    }

                    Logger.Debug($"Пользователь {user.Login} успешно создан");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка создания пользователя {user.Login} : {ex.Message}");
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            IEnumerable<Property> ret;

            try
            {
                IEnumerable<Property> userProperties = DataContextExtension.GetDbModelProperties(typeof(User)).
                                                                            Values
                                                                            .Select(x => new Property(x.Name, x.GetCustomAttribute<DescriptionAttribute>() != null ? x.GetCustomAttribute<DescriptionAttribute>()!.Description : String.Empty));

                //на самом деле, это не очень хорошо, но если использовать foreign keys (которые, в теории, должны быть:)), то может сработать весьма не дурно в данной модели БД
                IEnumerable<Property> passwordProperties = DataContextExtension.GetDbModelProperties(typeof(Sequrity))
                                                                                .Values
                                                                                .Select(x => new Property(x.Name, x.GetCustomAttribute<DescriptionAttribute>() != null ? x.GetCustomAttribute<DescriptionAttribute>()!.Description : String.Empty))
                                                                                .Where(x => x.Name != nameof(Sequrity.UserId));

                ret = userProperties.Union(passwordProperties);

                Logger.Debug("Все свойства пользователя успешно получены");
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения всех свойств пользователя : {ex.Message}");

                ret = Enumerable.Empty<Property>();
            }

            return ret;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            IEnumerable<UserProperty> ret = Enumerable.Empty<UserProperty>();

            try
            {
                using (DataContext dataContext = _pooledDbContextFactory!.CreateDbContext())
                {
                    User? user = dataContext.Users.AsNoTracking().FirstOrDefault(x => x.Login == userLogin);

                    if (user != null)
                    {
                        ret = DataContextExtension.GetDbModelProperties(typeof(User)).Values.Select(x => new UserProperty(x.Name, x.GetValue(user)!.ToString()!));

                        Logger.Debug($"Cвойства пользователя \"{userLogin}\" успешно получены");
                    }
                    else
                    {
                        Logger.Error($"Невозможно получить основные свойства пользователя. Пользователь \"{userLogin}\" не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения основных свойств пользователя {userLogin} : {ex.Message}");
            }

            return ret;
        }


        public bool IsUserExists(string userLogin)
        {
            bool ret = false;

            try
            {
                using (DataContext dataContext = _pooledDbContextFactory!.CreateDbContext())
                {
                    ret = dataContext.Users.AsNoTracking().Any(x => x.Login == userLogin);
                }

                Logger.Debug(String.Format("Пользователь с логином {0} {1}", userLogin, ret ? "найден" : "отсутствует"));
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка проверки пользователя {userLogin} : {ex.Message}");
            }

            return ret;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!this.IsUserExists(userLogin))
            {
                Logger.Debug($"обновление свойств пользователю \"{userLogin}\" отменено.");
            }
            else
            {
                try
                {
                    using (DataContext dataContext = _pooledDbContextFactory!.CreateDbContext())
                    {

                        User user = dataContext.Users.First(x => x.Login == userLogin);
                        user.SetUserProperies(properties);
                        dataContext.SaveChanges();
                    }

                    Logger.Debug($"Обновление свойств пользователю \"{userLogin}\" успешно завершено");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка обновления свойств пользователя \"{userLogin}\" : {ex.Message}");
                }
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            IEnumerable<Permission> ret = Enumerable.Empty<Permission>();

            try
            {
                using (DataContext dataContext = _pooledDbContextFactory!.CreateDbContext())
                {
                    dataContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                    IEnumerable<Permission> itRoles = dataContext.ITRoles.Select(x => new Permission(x.Id.ToString()!, x.Name, x.CorporatePhoneNumber)).ToList();
                    IEnumerable<Permission> requestRight = dataContext.RequestRights.Select(x => new Permission(x.Id.ToString()!, x.Name, String.Empty)).ToList();

                    ret = itRoles.Union(requestRight);
                }
                Logger.Debug($"Права успешно получены");
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка при получении прав : {ex.Message}");
            }

            return ret;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!this.IsUserExists(userLogin))
            {
                Logger.Debug($"Невозможно создать права для пользователя \"{userLogin}\"");
            }
            else
            {
                try
                {
                    (IEnumerable<UserRequestRight> userRequestRights, IEnumerable<UserITRole> userITRoles) = DataContextExtension.GetPermissionRange(rightIds, userLogin);

                    if (userRequestRights.Count() > 0 || userITRoles.Count() > 0)
                    {
                        using (DataContext dataContext = _pooledDbContextFactory!.CreateDbContext())
                        {
                            if (userRequestRights.Count() > 0)
                                dataContext.UserRequestRights.AddRange(userRequestRights);

                            if (userITRoles.Count() > 0)
                                dataContext.UserITRoles.AddRange(userITRoles);

                            dataContext.SaveChanges();
                        }

                        Logger.Debug($"Права для пользователя \"{userLogin}\" успешно установлены");
                    }
                    else
                    {
                        Logger.Warn($"Не удалось добавить права для пользователя \"{userLogin}\"");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка при создании прав пользователя \"{userLogin}\" : {ex.Message}");
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!this.IsUserExists(userLogin))
            {
                Logger.Debug($"Невозможно создать права для пользователя \"{userLogin}\"");
            }
            else
            {
                try
                {
                    (IEnumerable<UserRequestRight> userRequestRights, IEnumerable<UserITRole> userITRoles) = DataContextExtension.GetPermissionRange(rightIds, userLogin);

                    if (userRequestRights.Count() > 0 || userITRoles.Count() > 0)
                    {
                        using (DataContext dataContext = _pooledDbContextFactory!.CreateDbContext())
                        {
                            if (userRequestRights.Count() > 0)
                                dataContext.UserRequestRights.RemoveRange(userRequestRights);

                            if (userITRoles.Count() > 0)
                                dataContext.UserITRoles.RemoveRange(userITRoles);

                            dataContext.SaveChanges();
                        }

                        Logger.Debug($"Права для пользователя \"{userLogin}\" успешно установлены");
                    }
                    else
                    {
                        Logger.Warn($"Не удалось добавить права для пользователя \"{userLogin}\"");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка при создании прав : {ex.Message}");
                }
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            IEnumerable<String> ret = Enumerable.Empty<String>();

            if (!this.IsUserExists(userLogin))
            {
                Logger.Warn($"Невозможно получить права для пользователя \"{userLogin}\"");
            }
            else
            {
                try
                {
                    using (DataContext dataContext = _pooledDbContextFactory!.CreateDbContext())
                    {
                        dataContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                        ret = dataContext.Users
                            .Where(x => x.Login == userLogin)
                            .Join(dataContext.UserRequestRights,
                                x => x.Login,
                                y => y.UserId,
                                (x, y) => new { Login = x.Login, RightId = y.RightId })
                                .Join(dataContext.RequestRights,
                                x => x.RightId,
                                y => y.Id,
                                (x, y) => new { Login = x.Login, Name = y.Name })
                            .Select(x => x.Name).ToList();
                    }

                    Logger.Debug($"Права для пользователя \"{userLogin}\" успешно получены");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка при получении прав пользователя \"{userLogin}\" : {ex.Message}");
                }
            }

            return ret;
        }
    }
}