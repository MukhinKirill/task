using Dapper;
using System.Data;
using AvanpostGelik.Connector.Interfaces;
using Task.Integration.Data.Models.Models;
using Task.Connector.Interfaces;
namespace AvanpostGelik.Connector.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly IDatabaseService _connection;

    public PermissionRepository(IDatabaseService connection)
    {
        _connection = connection;
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        using var connection = _connection.GetOpenConnection();

        var requestRights = connection.Query(
            "SELECT id AS Id, name AS Name, 'Request' AS Description FROM [TestTaskSchema].[RequestRight]")
            .Select(row => new Permission(row.Id.ToString(), row.Name, row.Description));

        var itRoles = connection.Query(
            "SELECT id AS Id, name AS Name, 'Role' AS Description FROM [TestTaskSchema].[ItRole]")
            .Select(row => new Permission(row.Id.ToString(), row.Name, row.Description));

        // Объединяем результаты и возвращаем их в виде списка
        return requestRights.Concat(itRoles).ToList();
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        _connection.ExecuteInTransaction((conn, transaction) =>
        {
            var (requestRightIds, itRoleIds) = ParseRightIds(rightIds);

            foreach (var rightId in requestRightIds)
            {
                conn.Execute(
                    "INSERT INTO [TestTaskSchema].[UserRequestRight] (userId, RightId) VALUES (@UserId, @RightId)",
                    new { UserId = userLogin, RightId = rightId }, transaction);
            }

            foreach (var roleId in itRoleIds)
            {
                conn.Execute(
                    "INSERT INTO [TestTaskSchema].[UserItRole] (userId, RoleId) VALUES (@UserId, @RoleId)",
                    new { UserId = userLogin, RoleId = roleId }, transaction);
            }
        });
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        _connection.ExecuteInTransaction((conn, transaction) =>
        {
            var (requestRightIds, itRoleIds) = ParseRightIds(rightIds);

            foreach (var rightId in requestRightIds)
            {
                conn.Execute(
                    "DELETE FROM [TestTaskSchema].[UserRequestRight] WHERE userId = @UserId AND RightId = @RightId",
                    new { UserId = userLogin, RightId = rightId }, transaction);
            }

            foreach (var roleId in itRoleIds)
            {
                conn.Execute(
                    "DELETE FROM [TestTaskSchema].[UserItRole] WHERE userId = @UserId AND roleId = @RoleId",
                    new { UserId = userLogin, RoleId = roleId }, transaction);
            }
        });
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        using var connection = _connection.GetOpenConnection();
        var userRequestRights = connection.Query<string>(
            "SELECT 'Request:' + CAST(RightId AS NVARCHAR) " +
            "FROM [TestTaskSchema].[UserRequestRight] " +
            "WHERE userId = @UserId",
            new { UserId = userLogin });

        var userItRoles = connection.Query<string>(
            "SELECT 'Role:' + CAST(roleId AS NVARCHAR) " +
            "FROM [TestTaskSchema].[UserItRole] " +
            "WHERE userId = @UserId",
            new { UserId = userLogin });

        return userRequestRights.Concat(userItRoles);
    }

    private static (List<int> requestRightIds, List<int> itRoleIds) ParseRightIds(IEnumerable<string> rightIds)
    {
        var parsedRightIds = rightIds.Select(id =>
        {
            var parts = id.Split(':');
            return new
            {
                Type = parts.Length > 1 ? parts[0] : "",
                Id = int.TryParse(parts.Last(), out var parsedId) ? parsedId : (int?)null
            };
        }).Where(x => x.Id.HasValue).ToList();

        var requestRightIds = parsedRightIds
            .Where(x => x.Type.Equals("Request", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Id.Value).ToList();

        var itRoleIds = parsedRightIds
            .Where(x => x.Type.Equals("Role", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Id.Value).ToList();

        return (requestRightIds, itRoleIds);
    }
}
