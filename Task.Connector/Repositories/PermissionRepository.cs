using System.Data;
using Task.Integration.Data.Models.Models;
using Dapper;
using Task.Connector.Models;
using ILogger = Task.Integration.Data.Models.ILogger;

namespace Task.Connector.Repositories;

public interface IPermissionRepository
{
    IEnumerable<Permission> GetAllPermissions();
    void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
    void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
    IEnumerable<string> GetUserPermissions(string userLogin);
}

public class PermissionRepository : RepositoryBase, IPermissionRepository
{
    private readonly ILogger? _logger;

    private readonly IDbConnection _dbConnection;

    public PermissionRepository(ILogger? logger, IDbConnection dbConnection, string? schemaName) : base(dbConnection,
        schemaName)
    {
        _logger = logger;
        _dbConnection = dbConnection;
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        EnsureConnectionOpened();

        var sql = """
                  SELECT "id", "name", 'RequestRight' AS "description" FROM "RequestRight"
                  UNION 
                  SELECT "id", "name", 'IT Role' AS "description" FROM "ItRole";
                  """;

        var permissionData = _dbConnection.Query<(string Id, string Name, string Description)>(sql);

        var permissions = permissionData
            .Select(data => new Permission(data.Id, data.Name, data.Description));

        return permissions;
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        EnsureConnectionOpened();

        var rightsList = rightIds.ToList();
        var roles = ExtractPermissions(rightsList, userLogin, "Role:");
        var requests = ExtractPermissions(rightsList, userLogin, "Request:");

        try
        {
            InsertPermissions("UserITRole", "roleId", roles);
            InsertPermissions("UserRequestRight", "rightId", requests);
        }
        catch (Exception)
        {
            _logger?.Error($"Failed to add permissions for user: {userLogin}");
            throw;
        }
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        EnsureConnectionOpened();

        var rightsList = rightIds.ToList();
        var roles = ExtractPermissions(rightsList, userLogin, "Role:");
        var requests = ExtractPermissions(rightsList, userLogin, "Request:");

        try
        {
            DeletePermissions("UserITRole", "roleId", userLogin, roles);
            DeletePermissions("UserRequestRight", "rightId", userLogin, requests);
        }
        catch (Exception)
        {
            _logger?.Error($"Failed to remove permissions for user: {userLogin}");
            throw;
        }
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        EnsureConnectionOpened();

        var sql = """
                  SELECT rr.name 
                  FROM "UserRequestRight" urr
                  JOIN "RequestRight" rr ON urr."rightId" = rr.id
                  WHERE urr."userId" = @UserId;
                  """;

        return _dbConnection.Query<string>(sql, new { UserId = userLogin });
    }

    private void InsertPermissions(string tableName, string idColumn, IEnumerable<UserPermission> permissions)
    {
        if (permissions.Any())
        {
            var sql = $"""
                       INSERT INTO "{tableName}" ("userId", "{idColumn}")
                       VALUES (@UserId, @Id);
                       """;
            _dbConnection.Execute(sql, permissions);
        }
    }

    private void DeletePermissions(string tableName, string idColumn, string userLogin,
        IEnumerable<UserPermission> permissions)
    {
        var permissionsList = permissions.ToList();
        if (permissionsList.Any())
        {
            var ids = permissionsList.Select(p => p.Id).ToArray();
            var sql = $"""
                       DELETE FROM "{tableName}"
                       WHERE "userId" = @UserId AND "{idColumn}" = ANY(@Ids);
                       """;
            var parameters = new { UserId = userLogin, Ids = ids };
            _dbConnection.Execute(sql, parameters);
        }
    }

    private IEnumerable<UserPermission> ExtractPermissions(IEnumerable<string> rightIds, string userLogin,
        string prefix)
    {
        return rightIds
            .Where(id => id.StartsWith(prefix))
            .Select(id => new UserPermission
            {
                UserId = userLogin,
                Id = int.Parse(id.Split(':')[1])
            });
    }
}