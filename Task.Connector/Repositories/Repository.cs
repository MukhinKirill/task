using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Models;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector.Repositories
{
    public class Repository : IStorage
    {
        private readonly string connectionString;
        private readonly ILogger logger;
        public Repository(string _connectionString, ILogger _logger)
        {
            logger = _logger;
            var str = _connectionString.Split("ConnectionString=\'")[1].Split("\'")[0];
            connectionString = str;
        }

        public TestDbContext ConnectToDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new TestDbContext(optionsBuilder.Options);
        }

        public void AddUser(User user, Password password) 
        {
            using (TestDbContext db = ConnectToDatabase())
            {
                db.Users.Add(user);
                db.Passwords.Add(password);
                db.SaveChanges();
            }
        }

        public User GetUserFromLogin(string userLogin)
        {
            using (TestDbContext db = ConnectToDatabase())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);
                return user;
            }
        }
        public bool IsUserExists(string userLogin)
        {
            using (TestDbContext db = ConnectToDatabase())
            {
                return db.Users.Any(u => u.Login == userLogin);
            }
        }

        public void UpdateUser(User user)
        {
            using (TestDbContext db = ConnectToDatabase())
            {
                db.Users.Update(user);
                db.SaveChanges();
            }
        }
    }
}
