using FluentResults;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Storage;

public interface IGetUserPermissions
{
    string UserLogin { get; }
}

public interface IUserAddRole
{
    string UserLogin { get; }
    IEnumerable<int> RoleId { get; }
}

public interface IUserAddPermission
{
    string UserLogin { get; }
    IEnumerable<int> PermissionId { get; }
}

public class RoleAlreadyExistException : Exception{}
public class RoleDoesNotExistException : Exception{}
public class PermissionAlreadyExistException : Exception{}
public class PermissionDoesNotExistException : Exception{}

public interface IPermissionRepository : IDisposable
{
    Task<Result<IEnumerable<Permission>>> GetAllPermissionsAsync(CancellationToken token);
    Task<Result<IEnumerable<string>>> GetUserPermissionsAsync(IGetUserPermissions req, CancellationToken token);
    Task<Result> AddUserRoleAsync(IUserAddRole req, CancellationToken token);
    Task<Result> AddUserPermissionAsync(IUserAddPermission req, CancellationToken token);
    Task<Result> RemoveUserRoleAsync(IUserAddRole req, CancellationToken token);
    Task<Result> RemoveUserPermissionAsync(IUserAddPermission req, CancellationToken token);
}