using System.Data.Common;
using FluentResults;
using Npgsql;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Storage;

/// <summary>
/// <c>PermissionRepository</c> that utilizes <c>Npgsql</c> data provider
/// </summary>
internal sealed class NpgsqlPermissionRepository : NpgsqlBaseRepository, IPermissionRepository
{
    public NpgsqlPermissionRepository(string connString) : base(connString)
    {
        
    }
    
    public async Task<Result<IEnumerable<Permission>>> GetAllPermissionsAsync(CancellationToken token)
    {
        await using var cmd = DataSource.CreateCommand(@"
                SELECT ""id"", ""name"", '' description FROM ""TestTaskSchema"".""ItRole"" 
                UNION ALL
                SELECT ""id"", ""name"", '' description FROM ""TestTaskSchema"".""RequestRight""
            ");
        NpgsqlDataReader reader;
        try
        {
            reader = await cmd.ExecuteReaderAsync(token);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }

        List<Permission> perms;
        try
        {
            perms = new List<Permission>(4);
            while (reader.Read())
            {
                var perm = new Permission(
                    id: reader.GetInt64(0).ToString(),
                    name: reader.GetString(1),
                    description: reader.GetString(2)
                );
                perms.Add(perm);
            }
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }
        finally
        {
            reader.Dispose();
        }

        return perms;
    }

    public async Task<Result<IEnumerable<string>>> GetUserPermissionsAsync(IGetUserPermissions req, CancellationToken token)
    {
        await using var cmd = DataSource.CreateCommand(@"
            SELECT rr.""name"" FROM ""TestTaskSchema"".""UserRequestRight"" urr
            LEFT JOIN ""TestTaskSchema"".""RequestRight"" rr ON urr.""rightId"" = rr.id
            WHERE urr.""userId""=$1"
        );
        cmd.Parameters.AddWithValue(req.UserLogin);

        NpgsqlDataReader reader;
        try
        {
            reader = await cmd.ExecuteReaderAsync(token);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }

        List<string> perms;
        try
        {
            perms = new List<string>(4);
            while (reader.Read())
            {
                perms.Add(reader.GetString(0));
            }
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }
        finally
        {
            reader.Dispose();
        }

        return perms;
    }

    public async Task<Result> AddUserPermissionAsync(IUserAddPermission req, CancellationToken token)
    {
        await using var conn = DataSource.CreateConnection();
        NpgsqlTransaction tx;

        try
        {
            await conn.OpenAsync(token);
            tx = await conn.BeginTransactionAsync(token);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }

        bool commited = false;
        try
        {
            foreach (var roleId in req.PermissionId)
            {
                await using var cmd = new NpgsqlCommand(@"
                  INSERT INTO ""TestTaskSchema"".""UserRequestRight"" (""userId"", ""rightId"") VALUES ($1, $2)", conn
                );
                cmd.Parameters.AddWithValue(req.UserLogin);
                cmd.Parameters.AddWithValue(roleId);
                await cmd.ExecuteNonQueryAsync(token);
            }

            tx.Commit();
            commited = true;
        }
        catch (OperationCanceledException e)
        {
            return new ExceptionalError(e);
        }
        catch (PostgresException e)
        {
            switch (e.SqlState)
            {
                case PostgresErrorCodes.UniqueViolation:
                    return new ExceptionalError(new RoleAlreadyExistException());
                case PostgresErrorCodes.ForeignKeyViolation:
                    if (e.ColumnName?.Equals("userId") ?? false)
                    {
                        return new ExceptionalError(new UserDoesNotExistException());
                    }
                    else
                    {
                        return new ExceptionalError(new RoleDoesNotExistException());
                    }
                default:
                    return new ExceptionalError(e);
            }
        }
        finally
        {
            if (!commited) tx.Rollback();
        }
        
        return Result.Ok();
    }

    public async Task<Result> RemoveUserPermissionAsync(IUserAddPermission req, CancellationToken token)
    {
        await using var conn = DataSource.CreateConnection();
        NpgsqlTransaction tx;

        try
        {
            await conn.OpenAsync(token);
            tx = await conn.BeginTransactionAsync(token);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }

        bool commited = false;
        try
        {
            foreach (var roleId in req.PermissionId)
            {
                await using var cmd = new NpgsqlCommand(@"
                  DELETE FROM ""TestTaskSchema"".""UserRequestRight"" WHERE ""userId""=$1 AND ""rightId""=$2", conn
                );
                cmd.Parameters.AddWithValue(req.UserLogin);
                cmd.Parameters.AddWithValue(roleId);
                await cmd.ExecuteNonQueryAsync(token);
            }

            tx.Commit();
            commited = true;
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }
        finally
        {
            if (!commited) tx.Rollback();
        }
        
        return Result.Ok();
    }

    public async Task<Result> AddUserRoleAsync(IUserAddRole req, CancellationToken token)
    {
        await using var conn = DataSource.CreateConnection();
        NpgsqlTransaction tx;

        try
        {
            await conn.OpenAsync(token);
            tx = await conn.BeginTransactionAsync(token);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }

        bool commited = false;
        try
        {
            foreach (var roleId in req.RoleId)
            {
                await using var cmd = new NpgsqlCommand(@"
                  INSERT INTO ""TestTaskSchema"".""UserITRole"" (""userId"", ""roleId"") VALUES ($1, $2)", conn
                );
                cmd.Parameters.AddWithValue(req.UserLogin);
                cmd.Parameters.AddWithValue(roleId);
                await cmd.ExecuteNonQueryAsync(token);
            }
            
            tx.Commit();
            commited = true;
        }
        catch (OperationCanceledException e)
        {
            return new ExceptionalError(e);
        }
        catch (PostgresException e)
        {
            switch (e.SqlState)
            {
                case PostgresErrorCodes.UniqueViolation:
                    return new ExceptionalError(new RoleAlreadyExistException());
                case PostgresErrorCodes.ForeignKeyViolation:
                    if (e.ColumnName?.Equals("userId") ?? false)
                    {
                        return new ExceptionalError(new UserDoesNotExistException());
                    }
                    else
                    {
                        return new ExceptionalError(new RoleDoesNotExistException());
                    }
                default:
                    return new ExceptionalError(e);
            }
        }
        finally
        {
            if (!commited) tx.Rollback();
        }
        
        return Result.Ok();
    }

    public async Task<Result> RemoveUserRoleAsync(IUserAddRole req, CancellationToken token)
    {
        await using var conn = DataSource.CreateConnection();
        NpgsqlTransaction tx;

        try
        {
            await conn.OpenAsync(token);
            tx = await conn.BeginTransactionAsync(token);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }

        bool commited = false;
        try
        {
            foreach (var roleId in req.RoleId)
            {
                await using var cmd = new NpgsqlCommand(@"
                 DELETE FROM ""TestTaskSchema"".""UserITRole"" WHERE ""roleId""=$1 AND ""userId""=$2", conn
                );
                cmd.Parameters.AddWithValue(roleId);
                cmd.Parameters.AddWithValue(req.UserLogin);
                await cmd.ExecuteNonQueryAsync(token);
            }

            tx.Commit();
            commited = true;
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }
        finally
        {
            if (!commited) tx.Rollback();
        }
        
        return Result.Ok();
    }
}