using Microsoft.EntityFrameworkCore;
using Task.Connector.Context;
using Task.Connector.Entities;
using Task.Connector.Errors;
using Task.Connector.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services;

public class PermissionService : IPermissionService
{
    private readonly DatabaseContext _context;

    private readonly ILogger _logger;
    
    public PermissionService(DatabaseContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        var roles = _context!.ItRoles
            .Select(role =>
                new Permission(
                    $"{role.name}-{role.corporatePhoneNumber}",
                    role.name, 
                    $"Корпоративный номер телефона - {role.corporatePhoneNumber}"))
            .AsEnumerable();
        var rights = _context!.RequestRights
            .Select(right => new Permission(right.id.ToString(), right.name, string.Empty))
            .AsEnumerable();
        return roles.Concat(rights);
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        var user =  _context!.Users.FirstOrDefault(user => user.login == userLogin);
        if (user is null) Error.Throw(_logger, new ArgumentException($"GetUserProperties(userLogin) : Пользователя с логином {userLogin} не существует"));
            
        foreach (var rightId in rightIds)
        {
            var rightInfo = rightId.Split(':');
            var type = rightInfo[0];
            var id = int.Parse(rightInfo[1]);
            if (type == "Role")
            {
                var role = rightId
                    .Select(_ => new UserITRole() { roleId = id, userId = user!.login })
                    .FirstOrDefault();
                if (role is not null) _context!.UserITRoles.Add(role);
            }
            else if (type == "Request"){
                var right = rightId
                    .Select(_ => new UserRequestRight() {rightId = id, userId = user!.login})
                    .FirstOrDefault();
                if (right is not null) _context!.UserRequestRights.Add(right);
                break;
            }
            else throw new ArgumentException("Неверный формат");
        }
        _context.SaveChanges();
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        var user = _context!.Users.FirstOrDefault(user => user.login == userLogin);
        if (user is null) Error.Throw(_logger, new ArgumentException($"GetUserProperties(userLogin) : Пользователя с логином {userLogin} не существует"));
            
        foreach (var rightId in rightIds)
        {
            var rightInfo = rightId.Split(':');
            var type = rightInfo[0];
            var id = int.Parse(rightInfo[1]);
            if (type == "Role")
            {
                var role = rightId
                    .Select(_ => new UserITRole() {roleId = id, userId = user!.login})
                    .FirstOrDefault();
                if(role is not null) _context!.UserITRoles.Remove(role);
            }
            else if (type == "Request")
            {
                var right = rightId
                    .Select(_ => new UserRequestRight() {rightId = id, userId = user!.login})
                    .FirstOrDefault();
                if(right is not null) _context!.UserRequestRights.Remove(right);
            }
            else throw new ArgumentException("Неверный формат");
        }

        _context.SaveChanges();
    }

    public IEnumerable<string> GetPermissionByUserLogin(string userLogin)
    {
        var roles = _context!.UserITRoles
            .Where(uir => uir.userId == userLogin)
            .Select(uir => _context.ItRoles.First(role => role.id == uir.roleId).name)
            .AsEnumerable();
        var rights = _context!.UserRequestRights
            .Where(urr => urr.userId == userLogin)
            .Select(urr => _context.RequestRights.First(right => right.id == urr.rightId).name)
            .AsEnumerable();
        return roles.Concat(rights);
    }
}