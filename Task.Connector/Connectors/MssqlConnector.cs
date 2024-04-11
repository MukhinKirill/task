using Microsoft.EntityFrameworkCore;
using Task.Connector.DbModels;
using Task.Connector.Exceptions;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Connectors
{
    internal class MssqlConnector : IConnector
    {
        private MssqlAvanpostContext _context;
        public ILogger Logger { get; set; }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException($"User with login:{nameof(userLogin)} is not exist.");

            List<int> requests, roles;

            SeparateRightsAndRoles(rightIds, out requests, out roles);

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
                throw new UserAlreadyExistsException($"User with login:{nameof(user.Login)} already exist.");

            var userToCreate = new User() { Login = user.Login };

            FillUserProperties(user.Properties, userToCreate);


            _context.Users.Add(userToCreate);
            _context.Passwords.Add(new Passwords() { Password = user.HashPassword, UserId = userToCreate.Login });

            _context.SaveChanges();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return _context.RequestRights
                .Select(rr => new Permission(rr.Id.ToString(), rr.Name, string.Empty)).AsEnumerable()
                .Concat(_context.ItRoles
                .Select(ir => new Permission(ir.Id.ToString(), ir.Name, string.Empty)).AsEnumerable());
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var user = new User();
            var resultList = GetPropertyListFromUser(user);

            resultList.Add(new UserProperty("password", string.Empty));

            return resultList.Select(p => new Property(p.Name, ""));
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException($"User with login:{nameof(userLogin)} is not exist.");

            return _context.UserRequestRights
                .Where(u => u.UserId == userLogin)
                .Join(_context.RequestRights,
                    urr => urr.RightId,
                    rr => rr.Id,
                    (urr, rr) => rr.Id.ToString())
                .ToList();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException($"User with login:{nameof(userLogin)} is not exist.");

            return GetPropertyListFromUser(_context.Users.Where(u => u.Login == userLogin).FirstOrDefault());
        }

        public bool IsUserExists(string userLogin) => _context.Users.Any(u => u.Login == userLogin);

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException($"User with login:{nameof(userLogin)} is not exist.");

            List<int> requests, roles;
            SeparateRightsAndRoles(rightIds, out requests, out roles);

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
            _context = new MssqlAvanpostContext();
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException($"User with login:{nameof(userLogin)} is not exist.");

            var user = _context.Users.Where(u => u.Login == userLogin).FirstOrDefault();

            FillUserProperties(properties, user);

            _context.SaveChanges();
        }

        private static List<UserProperty> GetPropertyListFromUser(User user)
        {
            return new List<UserProperty>
            {
                new UserProperty("lastName", user.LastName ?? string.Empty),
                new UserProperty("firstName", user.FirstName ?? string.Empty),
                new UserProperty("middleName", user.MiddleName ?? string.Empty),
                new UserProperty("telephoneNumber", user.TelephoneNumber ?? string.Empty),
                new UserProperty("isLead", user.IsLead.ToString() ?? string.Empty)
            };
        }

        private static void FillUserProperties(IEnumerable<UserProperty> properties, User? user)
        {
            user.LastName = properties.FirstOrDefault(x => x.Name == "lastName")?.Value ?? string.Empty;
            user.FirstName = properties.FirstOrDefault(x => x.Name == "firstName")?.Value ?? string.Empty;
            user.MiddleName = properties.FirstOrDefault(x => x.Name == "middleName")?.Value ?? string.Empty;
            user.TelephoneNumber = properties.FirstOrDefault(x => x.Name == "telephoneNumber")?.Value ?? string.Empty;

            var isLeadValue = properties.FirstOrDefault(x => x.Name == "isLead")?.Value;
            var isLead = false;

            if (!string.IsNullOrEmpty(isLeadValue))
                isLead = bool.Parse(isLeadValue);

            user.IsLead = isLead;
        }

        private static void SeparateRightsAndRoles(IEnumerable<string> rightIds, out List<int> rights, out List<int> roles)
        {
            rights = new List<int>();
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
                    rights.Add(rightId);
                }
            }
        }
    }
}
