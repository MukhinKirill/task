using Microsoft.EntityFrameworkCore;
using System;
using Task.Connector.DbModels;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Task.Connector.Connectors
{
    internal class PsqlConnector : IConnector
    {
        private PsqlAvanpostContext _context;
        public ILogger Logger { get; set; }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                throw new ArgumentNullException($"User with login:{nameof(userLogin)} is not exist.");
            }

            List<int> requests, roles;
            GetUserPermissions(rightIds, out requests, out roles);

            if (requests.Count > 0 || roles.Count > 0)
            {

                foreach (var request in requests)
                {
                    _context.UserRequestRights.Add(new UserRequestRight() { UserId = userLogin, RightId = request });
                }

                foreach (var role in roles)
                {
                    _context.UserItroles.Add(new UserItrole() { UserId = userLogin, RoleId = role });
                }

                _context.SaveChanges();
            }
        }

        public void CreateUser(UserToCreate user)
        {
            if (IsUserExists(user.Login))
            {
                throw new ArgumentNullException($"User with login:{nameof(user.Login)} already exist.");
            }

            var userToCreate = new User();

            userToCreate.Login = user.Login;
            userToCreate.LastName = user.Properties.FirstOrDefault(x => x.Name == "lastName")?.Value ?? userToCreate.LastName;
            userToCreate.FirstName = user.Properties.FirstOrDefault(x => x.Name == "firstName")?.Value ?? userToCreate.FirstName;
            userToCreate.MiddleName = user.Properties.FirstOrDefault(x => x.Name == "middleName")?.Value ?? userToCreate.MiddleName;
            userToCreate.TelephoneNumber = user.Properties.FirstOrDefault(x => x.Name == "telephoneNumber")?.Value ?? userToCreate.TelephoneNumber;

            var isLeadValue = user.Properties.FirstOrDefault(x => x.Name == "isLead")?.Value;
            var isLead = false;

            if (!string.IsNullOrEmpty(isLeadValue))
                isLead = Convert.ToBoolean(isLeadValue);

            userToCreate.IsLead = isLead;


            _context.Users.Add(userToCreate);
            _context.Passwords.Add(new Passwords() { Password = user.HashPassword, UserId = userToCreate.Login });

            _context.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var requestRights = _context.RequestRights
                .Select(rr => new
                {
                    Id = Constants.Constants.REQUEST_RIGHT_GROUP_NAME + Constants.Constants.DELIMETER + rr.Id.ToString(),
                    Name = rr.Name
                });

            var itRoles = _context.ItRoles
                .Select(ir => new
                {
                    Id = Constants.Constants.IT_ROLE_RIGHT_GROUP_NAME + Constants.Constants.DELIMETER + ir.Id.ToString(),
                    Name = ir.Name
                });

            return requestRights.Union(itRoles).Select(r => new Permission(r.Id, r.Name, ""));
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var user = new User();

            List<UserProperty> resultList = GetPropertyListFromUser(user);

            return resultList.Select(p => new Property(p.Name, ""));
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                throw new ArgumentNullException($"User with login:{nameof(userLogin)} is not exist.");
            }

            var userRequestRightsQuery = _context.UserRequestRights
            .Where(urr => urr.UserId == userLogin)
            .Join(_context.RequestRights,
                urr => urr.RightId,
                rr => rr.Id,
                (urr, rr) => new { rr.Name })
            .Select(result => result.Name);

            var userITRolesQuery = _context.UserItroles
                .Where(uir => uir.UserId == userLogin)
                .Join(_context.ItRoles,
                    uir => uir.RoleId,
                    ir => ir.Id,
                    (uir, ir) => new { ir.Name })
                .Select(result => result.Name);

            return userRequestRightsQuery.Union(userITRolesQuery);
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                throw new ArgumentNullException($"User with login:{nameof(userLogin)} is not exist.");
            }

            var user = _context.Users.Where(u => u.Login == userLogin).FirstOrDefault();

            return GetPropertyListFromUser(user);
        }

        public bool IsUserExists(string userLogin) => _context.Users.Any(u => u.Login == userLogin);

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                throw new ArgumentNullException($"User with login:{nameof(userLogin)} is not exist.");
            }

            List<int> requests, roles;
            GetUserPermissions(rightIds, out requests, out roles);

            if (requests.Count > 0)
            {
                _context.UserRequestRights.Where(rr => rr.UserId == userLogin && requests.Contains(rr.RightId)).ExecuteDelete();
            }
            if (roles.Count > 0)
            {
                _context.UserItroles.Where(ur => ur.UserId == userLogin && roles.Contains(ur.RoleId)).ExecuteDelete();
            }
        }

        public void StartUp(string connectionString)
        {
            _context = new PsqlAvanpostContext();
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                throw new ArgumentNullException($"User with login:{nameof(userLogin)} is not exist.");
            }

            var user = _context.Users.Where(u => u.Login == userLogin).FirstOrDefault();

            user.LastName = properties.FirstOrDefault(x => x.Name == "lastName")?.Value ?? user.LastName;
            user.FirstName = properties.FirstOrDefault(x => x.Name == "firstName")?.Value ?? user.FirstName;
            user.MiddleName = properties.FirstOrDefault(x => x.Name == "middleName")?.Value ?? user.MiddleName;

            user.TelephoneNumber = properties.FirstOrDefault(x => x.Name == "telephoneNumber")?.Value ?? user.TelephoneNumber;

            var isLeadValue = properties.FirstOrDefault(x => x.Name == "isLead")?.Value;
            var isLead = false;
            if (!string.IsNullOrEmpty(isLeadValue))
                isLead = bool.Parse(isLeadValue);

            user.IsLead = isLead;

            _context.SaveChanges();
        }


        private static List<UserProperty> GetPropertyListFromUser(User user)
        {
            var resultList = new List<UserProperty>();

            if (!string.IsNullOrEmpty(user.LastName))
                resultList.Add(new UserProperty("lastName", user.LastName));

            if (!string.IsNullOrEmpty(user.FirstName))
                resultList.Add(new UserProperty("firstName", user.FirstName));

            if (!string.IsNullOrEmpty(user.MiddleName))
                resultList.Add(new UserProperty("middleName", user.MiddleName));

            if (!string.IsNullOrEmpty(user.TelephoneNumber))
                resultList.Add(new UserProperty("telephoneNumber", user.TelephoneNumber));

            resultList.Add(new UserProperty("isLead", user.IsLead.ToString()));
            return resultList;
        }

        private static void GetUserPermissions(IEnumerable<string> rightIds, out List<int> requests, out List<int> roles)
        {
            requests = new List<int>();
            roles = new List<int>();
            foreach (var id in rightIds)
            {
                var splited = id.Split(Constants.Constants.DELIMETER);

                var rightId = Convert.ToInt32(splited[1]);

                if (splited[0] == Constants.Constants.IT_ROLE_RIGHT_GROUP_NAME)
                {
                    roles.Add(rightId);
                }
                if (splited[0] == Constants.Constants.REQUEST_RIGHT_GROUP_NAME)
                {
                    requests.Add(rightId);
                }
            }
        }
    }
}
