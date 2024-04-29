
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;


namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DbContextFactory dbContextFactory;
        private DataContext dataContext;
        public void StartUp(string connectionString)
        {
            dbContextFactory = new DbContextFactory(connectionString);
            dataContext = dbContextFactory.GetContext("MSSQL");
        }

        public void CreateUser(UserToCreate user)
        {
            dataContext.Users.Add(new User()
            {
                Login = user.Login,
                FirstName = user.Properties.Where(i => i.Name == "firstName").ToList().Count > 0
                            ? (user.Properties.FirstOrDefault(i => i.Name == "firstName").Value)
                            : "",
                MiddleName = user.Properties.Where(i => i.Name == "middleName").ToList().Count > 0
                            ? (user.Properties.FirstOrDefault(i => i.Name == "middleName").Value)
                            : "",
                LastName = user.Properties.Where(i => i.Name == "lastName").ToList().Count > 0
                            ? (user.Properties.FirstOrDefault(i => i.Name == "lastName").Value)
                            : "",
                TelephoneNumber = user.Properties.Where(i => i.Name == "telephoneNumber").ToList().Count > 0
                            ? (user.Properties.FirstOrDefault(i => i.Name == "telephoneNumber").Value)
                            : "",
                IsLead = user.Properties.Where(i => i.Name == "isLead").ToList().Count > 0
                            ? bool.Parse(user.Properties.FirstOrDefault(i => i.Name == "isLead").Value)
                            : bool.Parse("false")
            });

            dataContext.Passwords.Add(new Sequrity()
            {
                Password = user.HashPassword,
                UserId = user.Login
            });

            try
            {
                Logger.Debug("Запись нового пользователя в БД");
                dataContext.SaveChanges();
                Logger.Debug("Запись нового пользователя прошла успешно");
            }
            catch (Exception ex)
            {
                Logger.Error("Неудача при попытке записи нового пользователя в БД");
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Получение всех свойств для пользователей");
            return typeof(User).GetProperties()
                .Select(i => new Property(i.Name, i.Name));
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            IsUserExists(userLogin);

            var user = dataContext.Users.FirstOrDefault((User i) => i.Login.Equals(userLogin));

            try
            {
                Logger.Debug("Возврат значений пользователя по Логину");
                return typeof(User).GetProperties()
                    .Where(i => i.Name != "Login") // Условие т.к. логин является поисковой единицей => ожидаться в проперти не будет
                    .Select(i => new UserProperty(i.Name, i.GetValue(user).ToString()));
            }
            catch (Exception ex)
            {
                Logger.Error("Неудача при попытке поиска данных пользователя по логину");

                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                Logger.Debug($"Пробуем найти пользователя с логином {userLogin}");
                return dataContext.Users.Any(i => i.Login == userLogin);
            }
            catch (Exception ex)
            {
                Logger.Error($"Пользователя с логином {userLogin} не существует");
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = dataContext.Users.FirstOrDefault(i => i.Login == userLogin);

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

            try
            {
                Logger.Debug("Запись нового пользователя в БД");
                dataContext.SaveChanges();
                Logger.Debug("Запись нового пользователя прошла успешно");
            }
            catch (Exception ex)
            {
                Logger.Error("Неудача при попытке записи нового пользователя в БД");
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var itRoles = dataContext.ITRoles.
                Select(i => new Permission(i.Id.ToString(), i.Name, "Все роли")).ToList();

            var requestRights = dataContext.RequestRights
                 .Select(i => new Permission(i.Id.ToString(), i.Name, "Все доступные права")).ToList();

            itRoles.AddRange(requestRights);

            Logger.Debug("Получение всех доступных прав");

            return itRoles;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            IsUserExists(userLogin);

            var userRequestRights = dataContext.UserRequestRights;
            var userITRoles = dataContext.UserITRoles;

            foreach (string rightId in rightIds)
            {
                var roleValues = rightId.Split(':', 2);
                var roleValue = int.Parse(roleValues[1]);

                switch (roleValues[0])
                {
                    case "Role":
                        userITRoles.Add(new UserITRole()
                        {
                            UserId = userLogin,
                            RoleId = roleValue
                        });

                        break;

                    case "Request":
                        userRequestRights.Add(new UserRequestRight()
                        {
                            UserId = userLogin,
                            RightId = roleValue
                        });
                        break;
                }
            }

            try
            {
                Logger.Debug("Запись нового пользователя в БД");
                dataContext.SaveChanges();
                Logger.Debug("Запись нового пользователя прошла успешно");

            }
            catch (Exception ex)
            {
                Logger.Error("Неудача при попытке записи нового пользователя в БД");
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var userRequestRights = dataContext.UserRequestRights;

            IsUserExists(userLogin);

            foreach (string rightId in rightIds)
            {
                var rID = int.Parse(rightId.Split(":")[1]);
                var roleByUser = userRequestRights.FirstOrDefault(i => i.UserId == userLogin && i.RightId == rID);

                userRequestRights.Remove(roleByUser);
            }

            try
            {
                Logger.Debug("Запись нового пользователя в БД");
                dataContext.SaveChanges();
                Logger.Debug("Запись нового пользователя прошла успешно");
            }
            catch (Exception ex)
            {
                Logger.Error("Неудача при попытке записи нового пользователя в БД");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var userRequestRights = dataContext.UserRequestRights;
            var requestRights = dataContext.RequestRights;

            IsUserExists(userLogin);

            try
            {
                Logger.Debug($"Попытка получить роли для пользователя {userLogin}");
                var rightId = userRequestRights.
                    Where(j => j.UserId == userLogin).
                    Select(x => x.RightId).ToList();

                return requestRights.
                    Where(i => rightId.Contains((int)i.Id)).
                    Select(y => y.Name).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Неудача при попытке поиска прав пользователя по логину");

                throw;
            }
        }

        public ILogger Logger { get; set; }
    }
}