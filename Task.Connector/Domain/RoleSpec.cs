using Task.Connector.Storage;

namespace Task.Connector.Domain;

public class GetUserPermissionsDto : IGetUserPermissions
{
    public GetUserPermissionsDto(string userLogin)
    {
        UserLogin = userLogin;
    }
    
    public string UserLogin { get; }
}

public class AddUserPermissionsDto : IUserAddPermission
{
    public AddUserPermissionsDto(string userLogin, IEnumerable<int> perms)
    {
        UserLogin = userLogin;
        PermissionId = perms;
    }

    
    public string UserLogin { get; }
    public IEnumerable<int> PermissionId { get; }
}

public class AddUserRolesDto : IUserAddRole
{
    public AddUserRolesDto(string userLogin, IEnumerable<int> roles)
    {
        UserLogin = userLogin;
        RoleId = roles;
    }
    
    public string UserLogin { get; }
    public IEnumerable<int> RoleId { get; }
}