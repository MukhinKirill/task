using Task.Connector.Interfaces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ILogger _logger;
    private readonly DataContext _context;

    public PermissionRepository(ILogger logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }
    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        var userRoles = _context.UserITRoles.Where(ur => ur.UserId == userLogin).Select(r => r.RoleId).ToArray();
        var userRights = _context.UserRequestRights.Where(ur => ur.UserId == userLogin).Select(r => r.RightId).ToArray();
        foreach (var right in rightIds)
        {

            var data = right.Split(":");
            if (data.Length != 2 || !int.TryParse(data[1], out var rightId))
            {
                _logger.Warn($"The right of this type {right} cannot be recognized ");
                continue;
            }

            if (data[0] == "Role")
            {
                if (userRoles.Any(r => r == rightId))
                {
                    _logger.Warn($"The role with id {rightId} for this user is exist!");
                    continue;
                }
                _context.UserITRoles.Add(new UserITRole() { UserId = userLogin, RoleId = rightId });
            }
            else if (data[0] == "Request")
            {
                if (userRights.Any(r => r == rightId))
                {
                    _logger.Warn($"The right with id {rightId} for this user is exist!");
                    continue;
                }
                _context.UserRequestRights.Add(new UserRequestRight() { UserId = userLogin, RightId = rightId });
            }
            else
            {
                _logger.Warn($"The right of this type {right} cannot be recognized ");
                continue;
            }
        }
        _context.SaveChanges();
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        var permissions = new List<Permission>();
        permissions.AddRange(_context.RequestRights.Select(r => new Permission(r.Id.ToString(), r.Name, "RequestRight")));
        permissions.AddRange(_context.ITRoles.Select(r => new Permission(r.Id.ToString(), r.Name, "ItRole")));
        return permissions;
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        var permissions = new List<string>();
        permissions.AddRange(_context.UserITRoles.Where(ur => ur.UserId == userLogin).Select(ur => $"Role:{ur.RoleId}"));
        permissions.AddRange(_context.UserRequestRights.Where(ur => ur.UserId == userLogin).Select(ur => $"Request:{ur.RightId}"));
        return permissions;
    }

    //Здесь лучше использовать ExecuteDelete() вместо создания массивов, но версия ef core заблокирована из-за зависимостей
    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        var rights = new List<UserRequestRight>();
        var roles = new List<UserITRole>();
        foreach (var right in rightIds)
        {

            var data = right.Split(":");
            if (data.Length != 2 || !int.TryParse(data[1], out var rightId))
            {
                _logger.Warn($"The right of this type {right} cannot be recognized ");
                continue;
            }

            if (data[0] == "Role")
            {
                var dbRole = _context.UserITRoles.FirstOrDefault(r => r.UserId == userLogin && r.RoleId == rightId);
                if (dbRole != null) roles.Add(dbRole);

            }
            else if (data[0] == "Request")
            {
                var dbRight = _context.UserRequestRights.FirstOrDefault(r => r.UserId == userLogin && r.RightId == rightId);
                if (dbRight != null) rights.Add(dbRight);
            }
            else
            {
                _logger.Warn($"The right of this type {right} cannot be recognized ");
                continue;
            }
        }

        _context.UserRequestRights.RemoveRange(rights);
        _context.UserITRoles.RemoveRange(roles);
        _context.SaveChanges();
    }
}
