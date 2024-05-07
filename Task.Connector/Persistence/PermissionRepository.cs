using System.Data;

using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Persistence;
public sealed class PermissionRepository
{
    private readonly DataContext _context;

    public PermissionRepository(DataContext context)
    {
        _context = context;
    }

    public List<Permission> GetRolePermissions()
    {
        return _context.ITRoles
                .Select(role => new Permission(role.Id!.Value.ToString(), role.Name, string.Empty))
                .ToList();
    }

    public List<Permission> GetRequestPermissions()
    {
        return _context.RequestRights
                .Select(request => new Permission(request.Id!.Value.ToString(), request.Name, string.Empty))
                .ToList();
    }

    public void AddRolePermissions(List<UserITRole> permissions)
    {
        _context.UserITRoles.AddRange(permissions);
        _context.SaveChanges();
    }

    public void AddRequestPermissions(List<UserRequestRight> permissions)
    {
        _context.UserRequestRights.AddRange(permissions);
        _context.SaveChanges();
    }

    public void RemoveRequestPermissions(List<UserRequestRight> permissions)
    {
        _context.UserRequestRights.RemoveRange(permissions);
        _context.SaveChanges();
    }

    public void RemoveRolePermissions(List<UserITRole> permissions)
    {
        _context.UserITRoles.RemoveRange(permissions);
        _context.SaveChanges();
    }
}
