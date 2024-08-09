using System.Text.RegularExpressions;
using static System.String;

namespace Task.Connector;

public class DbParams {
    public string ConnectionString { get; set; }
    public string Provider { get; set; }
    public string SchemaName { get; set; }

    //названия таблиц
    public string? UsersTableName { get; set; }
    public string? PasswordsTableName { get; set; }
    public string? RolesTableName { get; set; }
    public string? RequestRightsTableName { get; set; }
    public string? UsersRolesTableName { get; set; }
    public string? UsersRequestRightsTableName { get; set; }

    //названия внешних ключей
    public string? PasswordsFkUser { get; set; }
    public string? UsersRolesFkUser { get; set; }
    public string? UsersRolesFkRole { get; set; }
    public string? UsersRequestRightsFkUser { get; set; }
    public string? UsersRequestRightsFkRight { get; set; }

    public string? UsersPkPropName { get; set; }
    public string? PasswordPropName { get; set; }

    public string? PermissionDelimiter { get; set; }

    public DbParams(string connectionString, string provider, string schemaName) {
        ConnectionString = connectionString;
        Provider = provider;
        SchemaName = schemaName;
    }

    public string ToConnectionString() {
        var connectionString = $"{nameof(ConnectionString)}='{ConnectionString}';{nameof(Provider)}='{Provider}';{nameof(SchemaName)}='{SchemaName}'";

        if (UsersTableName != null)
            connectionString += $";{nameof(UsersTableName)}='{UsersTableName}'";
        if (PasswordsTableName != null)
            connectionString += $";{nameof(PasswordsTableName)}='{PasswordsTableName}'";
        if (RolesTableName != null)
            connectionString += $";{nameof(RolesTableName)}='{RolesTableName}'";
        if (RequestRightsTableName != null)
            connectionString += $";{nameof(RequestRightsTableName)}='{RequestRightsTableName}'";
        if (UsersRolesTableName != null)
            connectionString += $";{nameof(UsersRolesTableName)}='{UsersRolesTableName}'";
        if (UsersRequestRightsTableName != null)
            connectionString += $";{nameof(UsersRequestRightsTableName)}='{UsersRequestRightsTableName}'";
        if (PasswordsFkUser != null)
            connectionString += $";{nameof(PasswordsFkUser)}='{PasswordsFkUser}'";
        if (UsersRolesFkUser != null)
            connectionString += $";{nameof(UsersRolesFkUser)}='{UsersRolesFkUser}'";
        if (UsersRolesFkRole != null)
            connectionString += $";{nameof(UsersRolesFkRole)}='{UsersRolesFkRole}'";
        if (UsersRequestRightsFkUser != null)
            connectionString += $";{nameof(UsersRequestRightsFkUser)}='{UsersRequestRightsFkUser}'";
        if (UsersRequestRightsFkRight != null)
            connectionString += $";{nameof(UsersRequestRightsFkRight)}='{UsersRequestRightsFkRight}'";
        if (UsersPkPropName != null)
            connectionString += $";{nameof(UsersPkPropName)}='{UsersPkPropName}'";
        if (PasswordPropName != null)
            connectionString += $";{nameof(PasswordPropName)}='{PasswordPropName}'";
        if (PermissionDelimiter != null)
            connectionString += $";{nameof(PermissionDelimiter)}='{PermissionDelimiter}'";

        return connectionString;
    }

    public static DbParams FromConnectionString(string inputString) {
        var match = CreateConnectionStringRegex().Match(inputString);
        if (!match.Success) {
            throw new ArgumentException("Неверный формат строки подключения");
        }

        string connectionString = match.Groups[nameof(ConnectionString)].Value;
        string provider = match.Groups[nameof(Provider)].Value;
        string schemaName = match.Groups[nameof(SchemaName)].Value;

        var connectionStringParams = new DbParams(connectionString, provider, schemaName);

        string usersTableName = match.Groups[nameof(UsersTableName)].Value;
        string passwordsTableName = match.Groups[nameof(PasswordsTableName)].Value;
        string rolesTableName = match.Groups[nameof(RolesTableName)].Value;
        string requestRightsTableName = match.Groups[nameof(RequestRightsTableName)].Value;
        string usersRolesTableName = match.Groups[nameof(UsersRolesTableName)].Value;
        string usersRequestRightsTableName = match.Groups[nameof(UsersRequestRightsTableName)].Value;
        string passwordsFkUser = match.Groups[nameof(PasswordsFkUser)].Value;
        string usersRolesFkUser = match.Groups[nameof(UsersRolesFkUser)].Value;
        string usersRolesFkRole = match.Groups[nameof(UsersRolesFkRole)].Value;
        string usersRequestRightsFkUser = match.Groups[nameof(UsersRequestRightsFkUser)].Value;
        string usersRequestRightsFkRight = match.Groups[nameof(UsersRequestRightsFkRight)].Value;
        string usersPk = match.Groups[nameof(UsersPkPropName)].Value;
        string passwordPropName = match.Groups[nameof(PasswordPropName)].Value;
        string permissionDelimiter = match.Groups[nameof(PermissionDelimiter)].Value;

        connectionStringParams.UsersTableName = IsNullOrEmpty(usersTableName)
          ? "Users"
          : usersTableName;

        connectionStringParams.PasswordsTableName = IsNullOrEmpty(passwordsTableName)
          ? "Passwords"
          : passwordsTableName;

        connectionStringParams.RolesTableName = IsNullOrEmpty(rolesTableName)
          ? "Roles"
          : rolesTableName;

        connectionStringParams.RequestRightsTableName = IsNullOrEmpty(requestRightsTableName)
          ? "RequestRights"
          : requestRightsTableName;

        connectionStringParams.UsersRolesTableName = IsNullOrEmpty(usersRolesTableName)
          ? "UsersRoles"
          : usersRolesTableName;

        connectionStringParams.UsersRequestRightsTableName = IsNullOrEmpty(usersRequestRightsTableName)
          ? "UsersRequestRights"
          : usersRequestRightsTableName;

        connectionStringParams.PasswordsFkUser = IsNullOrEmpty(passwordsFkUser)
          ? "userId"
          : passwordsFkUser;

        connectionStringParams.UsersRolesFkUser = IsNullOrEmpty(usersRolesFkUser)
          ? "userId"
          : usersRolesFkUser;

        connectionStringParams.UsersRolesFkRole = IsNullOrEmpty(usersRolesFkRole)
          ? "roleId"
          : usersRolesFkRole;

        connectionStringParams.UsersRequestRightsFkUser = IsNullOrEmpty(usersRequestRightsFkUser)
          ? "userId"
          : usersRequestRightsFkUser;

        connectionStringParams.UsersRequestRightsFkRight = IsNullOrEmpty(usersRequestRightsFkRight)
          ? "rightId"
          : usersRequestRightsFkRight;

        connectionStringParams.UsersPkPropName = IsNullOrEmpty(usersPk)
          ? "login"
          : usersPk;

        connectionStringParams.PasswordPropName = IsNullOrEmpty(passwordPropName)
          ? "password"
          : passwordPropName;

        connectionStringParams.PermissionDelimiter = IsNullOrEmpty(permissionDelimiter)
          ? ":"
          : permissionDelimiter;

        return connectionStringParams;
    }

    private static Regex CreateConnectionStringRegex() {
        return new Regex($"ConnectionString='(?<{nameof(ConnectionString)}>[^']+)';Provider='(?<{nameof(Provider)}>[^']+)';SchemaName='(?<{nameof(SchemaName)}>[^']+)';(?:{nameof(UsersTableName)}='(?<{nameof(UsersTableName)}>[^']+)';)?(?:{nameof(PasswordsTableName)}='(?<{nameof(PasswordsTableName)}>[^']+)';)?(?:{nameof(RolesTableName)}='(?<{nameof(RolesTableName)}>[^']+)';)?(?:{nameof(RequestRightsTableName)}='(?<{nameof(RequestRightsTableName)}>[^']+)';)?(?:{nameof(UsersRolesTableName)}='(?<{nameof(UsersRolesTableName)}>[^']+)';)?(?:{nameof(UsersRequestRightsTableName)}='(?<{nameof(UsersRequestRightsTableName)}>[^']+)';)?(?:{nameof(PasswordsFkUser)}='(?<{nameof(PasswordsFkUser)}>[^']+)';)?(?:{nameof(UsersRolesFkUser)}='(?<{nameof(UsersRolesFkUser)}>[^']+)';)?(?:{nameof(UsersRolesFkRole)}='(?<{nameof(UsersRolesFkRole)}>[^']+)';)?(?:{nameof(UsersRequestRightsFkUser)}='(?<{nameof(UsersRequestRightsFkUser)}>[^']+)';)?(?:{nameof(UsersRequestRightsFkRight)}='(?<{nameof(UsersRequestRightsFkRight)}>[^']+)';)?(?:{nameof(UsersPkPropName)}='(?<{nameof(UsersPkPropName)}>[^']+)';)?(?:{nameof(PasswordPropName)}='(?<{nameof(PasswordPropName)}>[^']+)';)?(?:{nameof(PermissionDelimiter)}='(?<{nameof(PermissionDelimiter)}>[^']+)';)?");
    }

}