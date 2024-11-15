using System;
using Task.Connector.Contexts;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Helpers;
using System.Data.Entity;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        private ConnectorDbContext _context;
        public async void StartUp(string connectionString)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                bool isAvalaible = await db.Database.CanConnectAsync();
                if (isAvalaible) Logger?.Debug("Database context initialized.");
                else Logger?.Debug("Database context couldn't be initialized.");
            }
        }

        public async void CreateUser(UserToCreate user)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {

                if (IsUserExists(user.Login))
                {
                    Logger.Warn($"Пользователь с логином {user.Login} уже существует.");
                    return;
                }

                var newUser = new User
                {
                    Login = user.Login,
                    FirstName = user.Properties.FirstOrDefault(p => p.Name == "FirstName")?.Value ?? "FirstName",
                    LastName = user.Properties.FirstOrDefault(p => p.Name == "LastName")?.Value ?? "LastName",
                    MiddleName = user.Properties.FirstOrDefault(p => p.Name == "MiddleName")?.Value ?? "MiddleName",
                    TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "TelephoneNumber")?.Value ?? "TelephoneNumber",
                    IsLead = user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value == "false",
                };

                db.Users.Add(newUser);

                var password = new Password
                {
                    UserId = user.Login,
                    Password1 = user.HashPassword,
                };

                db.Passwords.Add(password);

                db.SaveChanges();

                Logger.Debug($"Создан новый пользователь - {user.Login}");
            }
        }

        //public IEnumerable<Property> GetAllProperties()
        //{
        //    using (ConnectorDbContext db = new ConnectorDbContext())
        //    {
        //        var userProperties = from p in db.Users.
                
        //    }
        //}

        public  IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);

                if (user == null)
                {
                    Logger?.Warn($"User with login {userLogin} not found.");
                    return Enumerable.Empty<UserProperty>();
                }

                var userProperties = new List<UserProperty>
            {
                new UserProperty("LastName:", user.LastName),
                new UserProperty("FirstName:", user.FirstName),
                new UserProperty("MiddleName:", user.MiddleName),
                new UserProperty("PhoneNumber:", user.TelephoneNumber),
                new UserProperty("IsLead:", user.IsLead.ToString())
            };

                Logger?.Debug($"{userLogin}: {userProperties.Count}");
                return userProperties;
            }
        }

        public  bool IsUserExists(string userLogin)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);
                return user == null;
            }
        }

        public  void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new InvalidCastException();
        }

        public  IEnumerable<Permission> GetAllPermissions()
        {
            throw new InvalidCastException();
        }

        public  void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new InvalidCastException();
        }

        public  void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new InvalidCastException();
        }

        public  IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new InvalidCastException();
        }
    }
}