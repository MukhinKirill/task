using System.Data.Common;
using System.Text;
using FluentResults;
using Npgsql;
using Task.Connector.Domain;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Storage;

/// <summary>
/// <c>UserRepository</c> that utilizes <c>Npgsql</c> data provider
/// </summary>
internal sealed class NpgsqlUserRepository : NpgsqlBaseRepository, IUserRepository
{
    public NpgsqlUserRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<Result<IEnumerable<UserProperty>>> GetUserPropertiesAsync(IGetUserProperties r, CancellationToken token)
    {
        await using var cmd = DataSource.CreateCommand(@"SELECT * FROM ""TestTaskSchema"".""User"" WHERE ""login""=$1");
        cmd.Parameters.AddWithValue(r.UserLogin);
        NpgsqlDataReader reader;
        try
        {
            reader = await cmd.ExecuteReaderAsync(token);
        }
        catch (OperationCanceledException e)
        {
            return new ExceptionalError(e);
        }
        catch (DbException e)
        {
            return new ExceptionalError(e);
        }

        List<UserProperty> props;
        try
        {
            props = new(4);
            if (await reader.ReadAsync(token))
            {
                props.AddRange(new UserProperty[]
                    {
                        new("lastName", reader.GetString(1)),
                        new("firstName", reader.GetString(2)),
                        new("middleName", reader.GetString(3)),
                        new("telephoneNumber", reader.GetString(4)),
                        new("isLead", reader.GetBoolean(5).ToString()),
                    }
                );
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

        return props;
    }

    public async Task<Result> UpdateUserProperties(IGetUserProperties req, CancellationToken token)
    {
        await using var cmd = DataSource.CreateCommand();
        
        var sb = new StringBuilder(@"UPDATE ""TestTaskSchema"".""User"" SET ");
        int i = 1;
        foreach (UserProperty prop in req.Properties)
        {
            if (string.IsNullOrEmpty(prop.Name)) continue;
            sb.Append('"').Append(prop.Name).Append('"').Append('=').Append('$').Append(i);
            cmd.Parameters.AddWithValue(prop.Value);
            i++;
        }

        // nothing updated
        if (i == 1)
        {
            return Result.Fail("no properties were set");
        }

        cmd.CommandText = sb.ToString();

        try
        {
            await cmd.ExecuteNonQueryAsync(token);
        }
        catch (DbException e)
        {
            return new ExceptionalError(e);
        }
        
        return Result.Ok();
    }

    public async Task<Result<bool>> DoesUserExist(IGetUserExist r, CancellationToken token)
    {
        await using var cmd = DataSource.CreateCommand(@"SELECT count(*) FROM ""TestTaskSchema"".""User"" WHERE ""login""=$1");
        cmd.Parameters.AddWithValue(r.UserLogin);
        object? res;
        try
        {
            res = await cmd.ExecuteScalarAsync(token);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }

        return res is not null && 
               res.GetType().IsAssignableFrom(typeof(long)) && 
               (long)res > 0;
    }

    public async Task<Result> CreateUserAsync(UserToCreate request, CancellationToken token)
    {
        var props = 
            request.Properties.ToDictionary(x => x.Name, x => x.Value);

        NpgsqlTransaction tx;
        await using var conn = DataSource.CreateConnection();
        try
        {
            await conn.OpenAsync(token);
            tx = await conn.BeginTransactionAsync(token);
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }

        NpgsqlCommand addUser, addPwd;
        try
        {
            addUser = new NpgsqlCommand(
                @"INSERT INTO ""TestTaskSchema"".""User"" 
                        (""login"", ""lastName"", ""firstName"", ""middleName"", ""telephoneNumber"", ""isLead"")
                    VALUES ($1, $2, $3, $4, $5, $6)"
                , conn);

            addPwd = new NpgsqlCommand(
                @"INSERT INTO ""TestTaskSchema"".""Passwords""
                        (""userId"", ""password"")
                    VALUES ($1, $2)"
                , conn);

            addUser.Parameters.AddWithValue(request.Login);
            if (props.TryGetValue("lastName", out var lastName))
            {
                addUser.Parameters.AddWithValue(lastName);
            }
            else
            {
                addUser.Parameters.AddWithValue("");
            }

            if (props.TryGetValue("firstName", out var firstName))
            {
                addUser.Parameters.AddWithValue(firstName);
            }
            else
            {
                addUser.Parameters.AddWithValue("");
            }
            
            if (props.TryGetValue("middleName", out var middleName))
            {
                addUser.Parameters.AddWithValue(middleName);
            }
            else
            {
                addUser.Parameters.AddWithValue("");
            }
            
            if (props.TryGetValue("telephoneNumber", out var telNumber))
            {
                addUser.Parameters.AddWithValue(telNumber);
            }
            else
            {
                addUser.Parameters.AddWithValue("");
            }
            
            if (props.TryGetValue("isLead", out var isLead) && bool.TryParse(isLead, out var bIsLead))
            {
                addUser.Parameters.AddWithValue(bIsLead);
            }
            else
            {
                addUser.Parameters.AddWithValue("");
            }
            
            addPwd.Parameters.AddWithValue(request.Login);
            addPwd.Parameters.AddWithValue(request.HashPassword);
        }
        catch (Exception e)
        {
            tx.Rollback();
            return new ExceptionalError(e);
        }

        bool commited = false;
        try
        {
            await addUser.ExecuteNonQueryAsync(token);
            await addPwd.ExecuteNonQueryAsync(token);
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
                    if (e.ColumnName?.Equals("login") ?? false)
                    {
                        return new ExceptionalError(new UserDoesNotExistException());
                    }

                    break;
            }
            
            return new ExceptionalError(e);
        }
        finally
        {
            if (!commited) tx.Rollback();
        }
        
        return Result.Ok();
    }

    public async Task<Result<IEnumerable<Property>>> GetAllPropertiesAsync(CancellationToken token)
    {
        await using var cmd = DataSource.CreateCommand(@"SELECT * FROM ""TestTaskSchema"".""User"" WHERE false");
        try
        {
            using var reader = await cmd.ExecuteReaderAsync(token);
            var schema = await reader.GetColumnSchemaAsync(token);
            return schema
                .Skip(1)
                .Select(x => new Property(x.ColumnName, ""))
                .Append(new Property("password", ""))
                .ToList();
        }
        catch (Exception e)
        {
            return new ExceptionalError(e);
        }
    }
}