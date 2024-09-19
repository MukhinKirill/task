using Task.Integration.Data.DbCommon;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector.Services.UserPermissionService;

internal class UserPermissionService : IUserPermissionService
{
    private DbContextFactory _contextFactory;
    private string _provider;

    internal UserPermissionService(DbContextFactory contextFactory, string provider)
    {
        _contextFactory = contextFactory;
        _provider = provider;
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        using var context = _contextFactory.GetContext(_provider);

        var userPermissions = context.Users
            .Where(u => u.Login == userLogin)
            .Select(u => new
            {
                RequestRights = context.UserRequestRights
                    .Where(urr => urr.UserId == userLogin)
                    .Select(urr => urr.RightId)
                    .ToArray(),
                ItRoles = context.UserITRoles
                    .Where(uir => uir.UserId == userLogin)
                    .Select(uir => uir.RoleId)
                    .ToArray()
            }).FirstOrDefault();

        if (userPermissions is null)
        {
            throw new Exception($"Пользователь с логином '{userLogin}' не найден.");
        }

        var permissions = new List<string>();

        foreach (var rightId in userPermissions.RequestRights)
        {
            permissions.Add($"{StringConstants.RequestRightGroupName}{StringConstants.Separator}{rightId}");
        }

        foreach (var roleId in userPermissions.ItRoles)
        {
            permissions.Add($"{StringConstants.ItRoleRightGroupName}{StringConstants.Separator}{roleId}");
        }

        return permissions;
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        using var context = _contextFactory.GetContext(_provider);

        var userPermissions = context.Users
            .Where(u => u.Login == userLogin)
            .Select(u => new
            {
                RequestRights = context.UserRequestRights
                    .Where(urr => urr.UserId == userLogin)
                    .Select(urr => urr.RightId)
                    .ToArray(),
                ITRoles = context.UserITRoles
                    .Where(uir => uir.UserId == userLogin)
                    .Select(uir => uir.RoleId)
                    .ToArray()
            }).FirstOrDefault();

        if (userPermissions is null)
        {
            throw new Exception($"Пользователь с логином '{userLogin}' не найден.");
        }

        foreach (var permission in rightIds)
        {
            var permissionInfo = permission.Split(StringConstants.Separator);
            var permissionId = int.Parse(permissionInfo[1]);

            switch (permissionInfo[0])
            {
                case StringConstants.RequestRightGroupName:
                    if (!userPermissions.RequestRights.Contains(permissionId))
                    {
                        context.UserRequestRights.Add(new()
                        {
                            UserId = userLogin,
                            RightId = permissionId,
                        });
                    }
                    break;
                case StringConstants.ItRoleRightGroupName:
                    if (!userPermissions.ITRoles.Contains(permissionId))
                    {
                        context.UserITRoles.Add(new()
                        {
                            UserId = userLogin,
                            RoleId = permissionId,
                        });
                    }
                    break;
            }
        }

        context.SaveChanges();
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        using var context = _contextFactory.GetContext(_provider);

        var userPermissions = context.Users
            .Where(u => u.Login == userLogin)
            .Select(u => new
            {
                RequestRights = context.UserRequestRights
                    .Where(urr => urr.UserId == userLogin)
                    .Select(urr => urr.RightId)
                    .ToArray(),
                ITRoles = context.UserITRoles
                    .Where(uir => uir.UserId == userLogin)
                    .Select(uir => uir.RoleId)
                    .ToArray()
            }).FirstOrDefault();

        if (userPermissions is null)
        {
            throw new Exception($"Пользователь с логином '{userLogin}' не найден.");
        }

        var rightsToRemove = new List<int>();
        var itRolesToRemove = new List<int>();

        foreach (var permission in rightIds)
        {
            var permissionInfo = permission.Split(StringConstants.Separator);
            var permissionId = int.Parse(permissionInfo[1]);

            switch (permissionInfo[0])
            {
                case StringConstants.RequestRightGroupName:
                    rightsToRemove.Add(permissionId);
                    break;
                case StringConstants.ItRoleRightGroupName:
                    rightsToRemove.Add(permissionId);
                    break;
            }
        }

        if (rightsToRemove.Count > 0)
        {
            context.UserRequestRights.Where(urr => urr.UserId == userLogin
                && rightsToRemove.Contains(urr.RightId)).ExecuteDelete();
        }

        if (itRolesToRemove.Count > 0)
        {
            context.UserITRoles.Where(uir => uir.UserId == userLogin
                && itRolesToRemove.Contains(uir.RoleId)).ExecuteDelete();
        }
    }
}
