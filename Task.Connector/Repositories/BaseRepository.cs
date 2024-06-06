using Task.Connector.Context;
using Task.Connector.Models;

namespace Task.Connector.Repositories
{
    internal abstract class BaseRepository : IStorage
    {
        protected abstract SqlDbContext ConnectToDatabase();


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
                return db.Users.FirstOrDefault(u => u.Login == userLogin) ?? throw new NullReferenceException();
            }
        }
        public bool IsUserExists(string userLogin)
        {
            using (var db = ConnectToDatabase())
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
                var roleIds = db.UserItroles
                                .Where(u => u.UserId == userLogin)
                                .Select(u => u.RoleId)
                                .ToList();
                foreach (var roleId in roleIds)
                {
                    var role = db.ItRoles
                                      .Where(r => r.Id == roleId)
                                      .SingleOrDefault() ?? throw new ArgumentNullException();
                }
            }
            return userRoles;
        }

        public List<RequestRight> GetItRequestRightsFromUser(string userLogin)
        {
            var userRequestRight = new List<RequestRight>();
            using (var db = ConnectToDatabase())
            {
                var rightIds = db.UserRequestRights
                                .Where(u => u.UserId == userLogin)
                                .Select(u => u.RightId)
                                .ToList();
                foreach (var rightId in rightIds)
                {
                    var requestRight = db.RequestRights
                                      .Where(r => r.Id == rightId)
                                      .SingleOrDefault() ?? throw new ArgumentNullException();

                    userRequestRight.Add(requestRight);
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
