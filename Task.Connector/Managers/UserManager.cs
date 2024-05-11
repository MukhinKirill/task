using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Managers
{
    public class UserManager
    {
        private DataContext dbContext;
        private ILogger _logger;

        public UserManager(DataContext dbContext, ILogger logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger; 
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                var newUser = new User
                {
                    Login = user.Login,
                    FirstName = user.Properties.FirstOrDefault(property => property.Name == "firstName")?.Value ?? "",
                    LastName = user.Properties.FirstOrDefault(property => property.Name == "lastName")?.Value ?? "",
                    MiddleName = user.Properties.FirstOrDefault(property => property.Name == "middleName")?.Value ?? "",
                    TelephoneNumber = user.Properties.FirstOrDefault(property => property.Name == "telephoneNumber")?.Value ?? "",
                    IsLead = user.Properties.FirstOrDefault(property => property.Name == "isLead").Value != null
                };

                dbContext.Users.Add(newUser);

                var newSequrityRecord = new Sequrity
                {
                    UserId = user.Login,
                    Password = user.HashPassword
                };

                dbContext.Passwords.Add(newSequrityRecord);

                dbContext.SaveChanges();

                _logger?.Debug($"Пользователь с логином {user.Login} успешно создан!");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при создании пользователя : {ex.Message}!");
                throw;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                var existingUser = dbContext.Users.FirstOrDefault(user => user.Login == userLogin);

                return existingUser != null;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Ошибка при проверке наличия пользователя: {ex.Message}");
                throw;
            }

        }

    }
}
