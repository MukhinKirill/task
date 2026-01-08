namespace TR.Connector.Configuration;

public class ConnectorSettings
{
    public ApiEndpointsSettings Endpoints { get; set; } = new();
    public UserStatusSettings UserStatus { get; set; } = new();
    public PermissionTypesSettings PermissionTypes { get; set; } = new();
}

public class ApiEndpointsSettings
{
    public string Login { get; set; } = "api/v1/login";
    public string RolesAll { get; set; } = "api/v1/roles/all";
    public string RightsAll { get; set; } = "api/v1/rights/all";
    public string UsersAll { get; set; } = "api/v1/users/all";
    public string UserByLogin { get; set; } = "api/v1/users/{0}";
    public string UserRoles { get; set; } = "api/v1/users/{0}/roles";
    public string UserRights { get; set; } = "api/v1/users/{0}/rights";
    public string AddRole { get; set; } = "api/v1/users/{0}/add/role/{1}";
    public string AddRight { get; set; } = "api/v1/users/{0}/add/right/{1}";
    public string DropRole { get; set; } = "api/v1/users/{0}/drop/role/{1}";
    public string DropRight { get; set; } = "api/v1/users/{0}/drop/right/{1}";
    public string CreateUser { get; set; } = "api/v1/users/create";
    public string EditUser { get; set; } = "api/v1/users/edit";
}

public class UserStatusSettings
{
    public string Locked { get; set; } = "Lock";
    public string Unlocked { get; set; } = "Unlock";
}

public class PermissionTypesSettings
{
    public string ItRole { get; set; } = "ItRole";
    public string RequestRight { get; set; } = "RequestRight";
}
