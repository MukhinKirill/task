using Task.Connector.Domain.Models;
using Task.Connector.Infrastructure.Context;
using Task.Connector.Infrastructure.DataModels;
using Task.Connector.Infrastructure.Repository.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure.Repository;

public class PermissionRepository : IPermissionRepository
{
    private readonly AvanpostContext _context;
    private readonly ILogger _logger;

    public PermissionRepository(AvanpostContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public IEnumerable<Permission> GetAllPermissions()
    {
        return _context.ItRoles
            .Select(r => new Permission(r.Id.ToString(), r.Name, string.Empty))
            .ToList()
            .Union(_context.RequestRights
                .Select(r => new Permission(r.Id.ToString(), r.Name, string.Empty)))
            .ToList();
    }

    public IEnumerable<PermissionDataModel> GetUserPermissions(string login)
    {

        return _context.UserItRoles.Where(ur => ur.UserId == login).Join(_context.ItRoles, ur => ur.RoleId,
            r => r.Id,
            (ur, r) => new PermissionDataModel()
            {
                Id = ur.RoleId.ToString(),
                Name = r.Name,
                Description = "Role Permission",
                Type = "Role"
            })
            .ToList()
            .Union(_context.UserRequestRights.Where(ur => ur.UserId == login).Join(_context.RequestRights, ur => ur.RightId,
            r => r.Id,
            (ur, r) => new PermissionDataModel()
            {
                Id = ur.RightId.ToString(),
                Name = r.Name,
                Description = "Request Permission",
                Type = "Request"
            }))
            .ToList();
    }
    
    public void AddRolePermissions(IEnumerable<UserItRole> permissions)
    {
        _context.UserItRoles.AddRange(permissions);
        _context.SaveChanges();
    }

    public void AddRequestPermissions(IEnumerable<UserRequestRight> permissions)
    {
        _context.UserRequestRights.AddRange(permissions);
        _context.SaveChanges();
    }

    public void RemoveRequestPermissions(IEnumerable<UserRequestRight> permissions)
    {
        _context.UserRequestRights.RemoveRange(permissions);
        _context.SaveChanges();
    }

    public void RemoveRolePermissions(IEnumerable<UserItRole> permissions)
    {
        _context.UserItRoles.RemoveRange(permissions);
        _context.SaveChanges();
    }
}