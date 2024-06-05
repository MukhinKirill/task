using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Models;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector.Repositories.MSSsql
{
    public class MSSqlRepository : IStorage
    {
        private readonly string connectionString;
        public MSSqlRepository(string _connectionString)
        {
            var str = _connectionString.Split("ConnectionString=\'")[1].Split("\'")[0];
            connectionString = str;
        }

        private MSSqlDbContext ConnectToDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MSSqlDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new MSSqlDbContext(optionsBuilder.Options);
        }

        public void AddUser(User user, Password password)
        {
            using (var db = ConnectToDatabase())
            {
                db.Users.Add(user);
                db.Passwords.Add(password);
                db.SaveChanges();
            }
        }

        public User GetUserFromLogin(string userLogin)
        {
            using (var db = ConnectToDatabase())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);
                return user ?? throw new NullReferenceException();
            }
        }
        public bool IsUserExists(string userLogin)
        {
            using (MSSqlDbContext db = ConnectToDatabase())
            {
                return db.Users.Any(u => u.Login == userLogin);
            }
        }

        public void UpdateUser(User user)
        {
            using (var db = ConnectToDatabase())
            {
                db.Users.Update(user);
                db.SaveChanges();
            }
        }

        public List<ItRole> GetAllItRoles()
        {
            using (var db = ConnectToDatabase())
            {
                return db.ItRoles.ToList();
            }
        }

        public List<RequestRight> GetAllItRequestRights()
        {
            using (var db = ConnectToDatabase())
            {
                return db.RequestRights.ToList();
            }
        }

        public List<ItRole> GetItRolesFromUser(string userLogin)
        {
            var userRoles = new List<ItRole>();
            using (var db = ConnectToDatabase())
            {
                var ids = db.UserItroles.Where(u => u.UserId == userLogin);
                foreach (var id in ids)
                {
                    userRoles.Add(db.ItRoles.Where(r => r.Id == id.RoleId).SingleOrDefault() ?? throw new NullReferenceException());
                }
            }
            return userRoles;
        }

        public List<RequestRight> GetItRequestRightsFromUser(string userLogin)
        {
            var userRequestRight = new List<RequestRight>();
            using (var db = ConnectToDatabase())
            {
                var ids = db.UserRequestRights.Where(u => u.UserId == userLogin);
                foreach (var id in ids)
                {
                    userRequestRight.Add(db.RequestRights.Where(r => r.Id == id.RightId).SingleOrDefault() ?? throw new NullReferenceException());
                }
            }
            return userRequestRight;
        }
        public void AddRolesToUser(string userLogin, List<UserItrole> userItRoles)
        {
            using (var db = ConnectToDatabase())
            {
                db.UserItroles.AddRange(userItRoles);
                db.SaveChanges();
            }
        }

        public void AddRequestRightsToUser(string userLogin, List<UserRequestRight> userRequestRights)
        {
            using (var db = ConnectToDatabase())
            {
                db.UserRequestRights.AddRange(userRequestRights);
                db.SaveChanges();
            }
        }

        public void RemoveRolesToUser(string userLogin, List<UserItrole> userItRoles)
        {
            using (var db = ConnectToDatabase())
            {
                db.UserItroles.RemoveRange(userItRoles);
                db.SaveChanges();
            }
        }

        public void RemoveRequestRightsToUser(string userLogin, List<UserRequestRight> userRequestRights)
        {
            using (var db = ConnectToDatabase())
            {
                db.UserRequestRights.RemoveRange(userRequestRights);
                db.SaveChanges();
            }
        }
    }
}
