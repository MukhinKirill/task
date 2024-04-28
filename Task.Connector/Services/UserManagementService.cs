using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services;

public class UserManagementService
{
    private readonly DataContext _context;
    public readonly ILogger _logger;

    public UserManagementService(DataContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public void SetUserProperties(User user, IEnumerable<UserProperty> properties)
    {
        Dictionary<string, Action<User, string>> propertySetters = new Dictionary<string, Action<User, string>>
        {
            { nameof(User.LastName), (u, value) => u.LastName = value ?? "" },
            { nameof(User.FirstName), (u, value) => u.FirstName = value ?? "" },
            { nameof(User.MiddleName), (u, value) => u.MiddleName = value ?? "" },
            { nameof(User.TelephoneNumber), (u, value) => u.TelephoneNumber = value ?? "" },
            {
                nameof(User.IsLead),
                (u, value) => u.IsLead = bool.TryParse(value, out var parsedBool) ? parsedBool : false
            }
        };
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                if (propertySetters.TryGetValue(prop.Name, out var setter))
                {
                    var safeValue = prop?.Value ?? "";
                }
            }
        }
    }

    public void UpdateUserPropertiesFromList(User user, IEnumerable<UserProperty> properties)
    {
        var propertySetters = new Dictionary<string, Action<User, string>>
        {
            { nameof(User.LastName), (u, value) => u.LastName = value ?? "" },
            { nameof(User.FirstName), (u, value) => u.FirstName = value ?? "" },
            { nameof(User.MiddleName), (u, value) => u.MiddleName = value ?? "" },
            { nameof(User.TelephoneNumber), (u, value) => u.TelephoneNumber = value ?? "" },
            { nameof(User.IsLead), (u, value) => u.IsLead = bool.TryParse(value, out var isLead) ? isLead : false }
        };

        foreach (var prop in properties)
        {
            if (propertySetters.TryGetValue(prop.Name, out var setter))
            {
                setter(user, prop.Value);
            }
            else
            {
                _logger.Warn($"Unrecognized property name '{prop.Name}'");
            }
        }
    }

    public bool IsRightIdInvalid(string rightId)
    {
        var split = rightId.Split(':'); // Разбиваем строку по двоеточию
        if (split.Length != 2)
        {
            _logger.Warn($"Invalid permission format: '{rightId}'");
            return true;
        }

        return false;
    }

    public (string type, int id) ParseRightId(string rightId)
    {
        var split = rightId.Split(':');
        var type = split[0];
        var id = int.Parse(split[1]);
        return (type, id);
    }

    public void AddUserRole(string userLogin, int id)
    {
        var role = _context.ITRoles.Find(id);
        if (role != null)
        {
            var userItRole = new UserITRole
            {
                RoleId = id,
                UserId = userLogin,
            };
            _context.UserITRoles.Add(userItRole);
            _logger.Warn($"Added role '{role.Name}' to user '{userLogin}'");
        }
        else
        {
            _logger.Warn($"Role with ID '{id}' not found");
        }

        _context.SaveChanges();
    }

    public void AddUserRequestRight(string userLogin, int id)
    {
        var request = _context.RequestRights.Find(id);
        if (request != null)
        {
            var userRequestRight = new UserRequestRight
            {
                UserId = userLogin,
                RightId = id,
            };

            _context.UserRequestRights.Add(userRequestRight);
            _logger.Debug($"Added request right '{request.Name}' to user '{userLogin}'");
        }
        else
        {
            _logger.Warn($"Request right with ID '{id}' not found");
        }

        _context.SaveChanges();
    }

    public void RemoveUserItRole(string userLogin, int itRoleId)
    {
        var itRole =
            _context.UserITRoles.FirstOrDefault(role => role.UserId == userLogin && role.RoleId == itRoleId);
        if (itRole != null)
        {
            _context.UserITRoles.Remove(itRole);
            _logger.Debug($"Added role to user '{userLogin}'");
        }
        else
        {
            _logger.Warn($"Role with ID '{itRoleId}' not found");
        }

        _context.SaveChanges();
    }

    public void RemoveUserRequestRight(string userLogin, int rightId)
    {
        var requestRole =
            _context.UserRequestRights.FirstOrDefault(role => role.UserId == userLogin && role.RightId == rightId);
        if (requestRole != null)
        {
            _context.UserRequestRights.Remove(requestRole);
        }
        else
        {
            _logger.Warn($"Role with ID '{rightId}' not found");
        }

        _context.SaveChanges();
    }
}
