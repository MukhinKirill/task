using Dapper;
using Task.Connector.DataBase.Models;
using Task.Integration.Data.Models.Models;
using Z.Dapper.Plus;

namespace Task.Connector.DataBase;

public class PermissionsRepository
{
    private readonly DataContext _context;

    private const string GetAllPermissionsSql = $"""
                                                SELECT id, "name" FROM "TestTaskSchema"."ItRole"
                                                UNION
                                                SELECT id, "name" FROM "TestTaskSchema"."RequestRight"
                                                """;
    private const string GetUserPermissionsSql = """
                                                   SELECT concat('{0}{1}', "rightId")  FROM "TestTaskSchema"."UserRequestRight" WHERE "userId" = @userLogin
                                                   Union
                                                   SELECT concat('{2}{1}', "roleId") FROM "TestTaskSchema"."UserITRole" WHERE "userId" = @userLogin
                                                   """;
    public PermissionsRepository(DataContext context)
    {
        _context = context;
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        var permissions = new List<Permission>();
        using var connection = _context.CreateConnection();
        using var reader = connection.ExecuteReader(GetAllPermissionsSql);

        while (reader.Read())
        {
            permissions.Add(new Permission(reader.GetInt32(0).ToString(), reader.GetString(1), ""));
        }

        return permissions;
    }

    public void AddUserRoles(string userLogin, IEnumerable<int> ids)
    {
        using var connection = _context.CreateConnection();

        var roles = ids.Select(x => new UserITRole { UserId = userLogin, RoleId = x });
            
        connection.BulkInsert(roles);
    }
    
    public void AddUserRequestRights(string userLogin, IEnumerable<int> ids)
    {
        using var connection = _context.CreateConnection();

        var requestRights = ids.Select(x => new UserRequestRight { UserId = userLogin, RightId = x });
            
        connection.BulkInsert(requestRights);
    }
    
    public void RemoveUserRoles(string userLogin, IEnumerable<int> ids)
    {
        using var connection = _context.CreateConnection();

        var roles = ids.Select(x => new UserITRole { UserId = userLogin, RoleId = x });
            
        connection.BulkDelete(roles);
    }
    
    public void RemoveUserRequestRights(string userLogin, IEnumerable<int> ids)
    {
        using var connection = _context.CreateConnection();

        var requestRights = ids.Select(x => new UserRequestRight { UserId = userLogin, RightId = x });
            
        connection.BulkDelete( requestRights);
    }
    
    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        using var connection = _context.CreateConnection();

        return connection.Query<string>(GetUserPermissionsSql, new {userLogin});
    }
}