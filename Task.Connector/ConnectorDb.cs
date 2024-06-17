using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Npgsql;
using System.Data.Common;
using System.Text;
using System.Collections.Immutable;

namespace Task.Connector;

public class ConnectorDb : IConnector
{
    private NpgsqlDataSource DataSource { get; set; }

    private static IImmutableDictionary<string, string> UserPropertyNames { get; }

    static ConnectorDb()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        builder.Add("lastname", "lastName");
        builder.Add("firstname", "firstName");
        builder.Add("middlename", "middleName");
        builder.Add("telephonenumber", "telephoneNumber");
        builder.Add("islead", "isLead");
        UserPropertyNames = builder.ToImmutable();
    }

    // TODO: replace the non-parameterized constructor with a proper one that allows to pass...
    // TODO: ...the connection string and the logger through its parameters
    public void StartUp(string connectionString)
    {
        try
        {
            var actualConnectionString = connectionString;
            var split = connectionString.Split('\'');
            for (var i = 0; i < split.Length - 1; i += 2)
            {
                if (split[i].ToLower().Contains("connectionstring"))
                {
                    actualConnectionString = split[i + 1];
                    break;
                }
            }

            DataSource = NpgsqlDataSource.Create(actualConnectionString);

            Logger?.Debug("Created data source");
        }
        catch (Exception e)
        {
            Logger?.Error($"Failed to create data source. {e.Message}");

            throw;
        }
    }

    private static IDictionary<string, string> ParseUserProperties(in IEnumerable<UserProperty> userProperties)
    {
        var properties = new Dictionary<string, string>();
        foreach (var name in UserPropertyNames.Keys)
        {
            properties[name] = "";
        }

        properties["islead"] = "false";

        var validPropertyNames = new LinkedList<string>(UserPropertyNames.Keys);

        foreach (var property in userProperties)
        {
            var propertyName = property.Name.ToLower();

            if (validPropertyNames.Contains(propertyName))
            {
                properties[propertyName] = property.Value;

                validPropertyNames.Remove(propertyName);
            }
            else
            {
                throw new ArgumentException(
                    $"Property \"{property.Name}\" doesn't exist or it is stated more than once");
            }
        }

        return properties;
    }

    private static DbBatchCommand CreateInsertUserCommand(in UserToCreate user)
    {
        const string commandText =
            "INSERT INTO \"TestTaskSchema\".\"User\" " +
            "(\"login\", \"lastName\", \"firstName\", \"middleName\", \"telephoneNumber\", \"isLead\") " +
            "VALUES (@login, @lastname, @firstname, @middlename, @telephonenumber, @islead)";

        var command = new NpgsqlBatchCommand(commandText);

        command.Parameters.AddWithValue("login", user.Login);

        var properties = ParseUserProperties(user.Properties);

        foreach (var property in properties)
        {
            switch (property.Key)
            {
                case "islead":
                    command.Parameters.AddWithValue(property.Key, property.Value.ToLower().Equals("true"));
                    break;
                default:
                    command.Parameters.AddWithValue(property.Key, property.Value);
                    break;
            }
        }

        return command;
    }

    private static DbBatchCommand CreateInsertPasswordCommand(string userId, string password)
    {
        const string commandText =
            "INSERT INTO \"TestTaskSchema\".\"Passwords\" (\"userId\", \"password\") VALUES ($1, $2)";

        var command = new NpgsqlBatchCommand(commandText);

        command.Parameters.AddWithValue(userId);
        command.Parameters.AddWithValue(password);

        return command;
    }

    public void CreateUser(UserToCreate user)
    {
        try
        {
            using var connection = DataSource.CreateConnection();
            using var batch = new NpgsqlBatch(connection)
            {
                BatchCommands =
                {
                    CreateInsertUserCommand(user),
                    CreateInsertPasswordCommand(user.Login, user.HashPassword)
                }
            };

            connection.Open();
            batch.ExecuteNonQuery();
            connection.Close();

            Logger?.Debug($"Created User \"{user.Login}\"");
        }
        catch (Exception e)
        {
            Logger?.Warn($"Failed to create User \"{user.Login}\". {e.Message}");

            throw;
        }
    }

    public IEnumerable<Property> GetAllProperties()
    {
        var properties = UserPropertyNames.Values.Select(name => new Property(name, "")).ToList();

        properties.Add(new Property("password", ""));

        return properties;
    }

    private DbDataReader GetUserReader(string userLogin)
    {
        const string commandText = "SELECT * FROM \"TestTaskSchema\".\"User\" WHERE \"login\" = $1";

        using var command = DataSource.CreateCommand(commandText);

        command.Parameters.AddWithValue(userLogin);

        return command.ExecuteReader();
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        try
        {
            using var reader = GetUserReader(userLogin);
            if (!reader.HasRows)
            {
                throw new ArgumentException("User not found");
            }

            reader.Read();

            var properties = UserPropertyNames.Values
                .Select(name => new UserProperty(name, reader[name].ToString()!)).ToList();

            Logger?.Debug($"Got properties of User \"{userLogin}\"");

            return properties;
        }
        catch (Exception e)
        {
            Logger?.Warn($"Failed to get properties of User \"{userLogin}\". {e.Message}");

            throw;
        }
    }

    public bool IsUserExists(string userLogin)
    {
        try
        {
            using var reader = GetUserReader(userLogin);
            if (!reader.HasRows)
            {
                Logger?.Debug($"User \"{userLogin}\" not found");

                return false;
            }

            Logger?.Debug($"User \"{userLogin}\" found");

            return true;
        }
        catch (Exception e)
        {
            Logger?.Warn($"Failed to check whether User \"{userLogin}\" exists. {e.Message}");

            throw;
        }
    }

    private static string BuildUpdateUserCommandText(in IEnumerable<UserProperty> properties)
    {
        var commandTextBuilder = new StringBuilder("UPDATE \"TestTaskSchema\".\"User\" SET");

        var validPropertyNames = new Dictionary<string, string>(UserPropertyNames);

        foreach (var property in properties)
        {
            var originalPropertyName = property.Name;

            property.Name = property.Name.ToLower();

            if (validPropertyNames.TryGetValue(property.Name, out var dbName))
            {
                commandTextBuilder.Append($" \"{dbName}\" = @{property.Name},");

                validPropertyNames.Remove(property.Name);
            }
            else
            {
                throw new ArgumentException(
                    $"Property \"{originalPropertyName}\" doesn't exist or it is stated more than once");
            }
        }

        commandTextBuilder.Remove(commandTextBuilder.Length - 1, 1).Append(" WHERE \"login\" = @login");

        return commandTextBuilder.ToString();
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        try
        {
            var propertyList = new List<UserProperty>(properties);
            using var command = DataSource.CreateCommand(BuildUpdateUserCommandText(propertyList));

            foreach (var property in propertyList)
            {
                switch (property.Name)
                {
                    case "islead":
                        command.Parameters.AddWithValue(property.Name, property.Value.ToLower().Equals("true"));
                        break;
                    default:
                        command.Parameters.AddWithValue(property.Name, property.Value);
                        break;
                }
            }

            command.Parameters.AddWithValue("login", userLogin);
            command.ExecuteNonQuery();

            Logger?.Debug($"Updated properties of User \"{userLogin}\"");
        }
        catch (Exception e)
        {
            Logger?.Warn($"Failed to update properties of User \"{userLogin}\". {e.Message}");

            throw;
        }
    }

    private IEnumerable<Permission> SelectAllPermissions()
    {
        var permissions = new List<Permission>();

        var commandText = "SELECT * FROM \"TestTaskSchema\".\"ItRole\"";
        using (var command = DataSource.CreateCommand(commandText))
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                permissions.Add(new Permission(reader["id"].ToString()!, reader["name"].ToString()!,
                    $"IT role. Corporate phone number: {reader["corporatePhoneNumber"]}"));
            }
        }

        commandText = "SELECT * FROM \"TestTaskSchema\".\"RequestRight\"";
        using (var command = DataSource.CreateCommand(commandText))
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                permissions.Add(new Permission(reader["id"].ToString()!, reader["name"].ToString()!,
                    "Request right"));
            }
        }

        return permissions;
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        try
        {
            var permissions = SelectAllPermissions();

            Logger?.Debug("Got all Permissions");

            return permissions;
        }
        catch (Exception e)
        {
            Logger?.Warn($"Failed to get all Permissions. {e.Message}");

            throw;
        }
    }

    private static void ParsePermissionIds(in IEnumerable<string> permissionIds,
        out ICollection<int> roleIds, out ICollection<int> requestRightIds)
    {
        roleIds = new HashSet<int>();
        requestRightIds = new HashSet<int>();

        foreach (var id in permissionIds)
        {
            var split = id.Split(':');
            var permissionType = split[0].ToLower();

            if (permissionType.Contains("role"))
            {
                roleIds.Add(int.Parse(split[1].Trim()));
            }
            else if (permissionType.Contains("request"))
            {
                requestRightIds.Add(int.Parse(split[1].Trim()));
            }
            else
            {
                throw new FormatException($"Invalid Permission type format: {split[0]}");
            }
        }
    }

    // TODO: add foreign keys to the database!
    private void CheckPermissionIds(in IEnumerable<int> roleIds, in IEnumerable<int> requestRightIds)
    {
        foreach (var roleId in roleIds)
        {
            const string commandText = "SELECT * FROM \"TestTaskSchema\".\"ItRole\" WHERE \"id\" = $1";
            var command = DataSource.CreateCommand(commandText);
            command.Parameters.AddWithValue(roleId);

            using var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                throw new ArgumentException($"Role \"{roleId}\" not found");
            }
        }

        foreach (var rightId in requestRightIds)
        {
            const string commandText = "SELECT * FROM \"TestTaskSchema\".\"RequestRight\" WHERE \"id\" = $1";
            var command = DataSource.CreateCommand(commandText);
            command.Parameters.AddWithValue(rightId);

            using var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                throw new ArgumentException($"Request right \"{rightId}\" not found");
            }
        }
    }

    private void InsertUserPermissions(string userLogin,
        in IEnumerable<int> roleIds, in IEnumerable<int> requestRightIds)
    {
        using var connection = DataSource.CreateConnection();
        using var batch = new NpgsqlBatch(connection);

        foreach (var roleId in roleIds)
        {
            const string commandText =
                "INSERT INTO \"TestTaskSchema\".\"UserITRole\" (\"userId\", \"roleId\") " +
                "VALUES ($1, $2)";
            var command = new NpgsqlBatchCommand(commandText);
            command.Parameters.AddWithValue(userLogin);
            command.Parameters.AddWithValue(roleId);
            batch.BatchCommands.Add(command);
        }

        foreach (var rightId in requestRightIds)
        {
            const string commandText =
                "INSERT INTO \"TestTaskSchema\".\"UserRequestRight\" (\"userId\", \"rightId\") " +
                "VALUES ($1, $2)";
            var command = new NpgsqlBatchCommand(commandText);
            command.Parameters.AddWithValue(userLogin);
            command.Parameters.AddWithValue(rightId);
            batch.BatchCommands.Add(command);
        }

        connection.Open();
        batch.ExecuteNonQuery();
        connection.Close();
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        try
        {
            // TODO: add foreign keys to the database!
            using (var reader = GetUserReader(userLogin))
            {
                if (!reader.HasRows)
                {
                    throw new ArgumentException("User not found");
                }
            }

            ParsePermissionIds(rightIds, out var roleIds, out var requestRightIds);

            // TODO: add foreign keys to the database!
            CheckPermissionIds(roleIds, requestRightIds);

            InsertUserPermissions(userLogin, roleIds, requestRightIds);

            Logger?.Debug($"Added Permissions for User \"{userLogin}\"");
        }
        catch (Exception e)
        {
            Logger?.Warn($"Failed to add Permissions for User \"{userLogin}\". {e.Message}");

            throw;
        }
    }

    private void DeleteUserPermissions(string userLogin,
        in IEnumerable<int> roleIds, in IEnumerable<int> requestRightIds)
    {
        using var connection = DataSource.CreateConnection();
        using var batch = new NpgsqlBatch(connection);

        foreach (var roleId in roleIds)
        {
            const string commandText =
                "DELETE FROM \"TestTaskSchema\".\"UserITRole\" WHERE (\"userId\" = $1, \"roleId\" = $2)";
            var command = new NpgsqlBatchCommand(commandText);
            command.Parameters.AddWithValue(userLogin);
            command.Parameters.AddWithValue(roleId);
            batch.BatchCommands.Add(command);
        }

        foreach (var requestRightId in requestRightIds)
        {
            const string commandText =
                "DELETE FROM \"TestTaskSchema\".\"UserRequestRight\" WHERE \"userId\" = $1 AND \"rightId\" = $2";
            var command = new NpgsqlBatchCommand(commandText);
            command.Parameters.AddWithValue(userLogin);
            command.Parameters.AddWithValue(requestRightId);
            batch.BatchCommands.Add(command);
        }

        connection.Open();
        batch.ExecuteNonQuery();
        connection.Close();
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        try
        {
            ParsePermissionIds(rightIds, out var roleIds, out var requestRightIds);

            DeleteUserPermissions(userLogin, roleIds, requestRightIds);

            Logger?.Debug($"Removed Permissions for User \"{userLogin}\"");
        }
        catch (Exception e)
        {
            Logger?.Warn($"Failed to remove Permissions for User \"{userLogin}\". {e.Message}");

            throw;
        }
    }

    private IEnumerable<string> SelectUserPermissions(string userLogin)
    {
        var permissions = new List<string>();

        var commandText = "SELECT * FROM \"TestTaskSchema\".\"UserITRole\" WHERE \"userId\" = $1";
        using (var command = DataSource.CreateCommand(commandText))
        {
            command.Parameters.AddWithValue(userLogin);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                permissions.Add($"Role:{reader["roleId"]}");
            }
        }

        commandText = "SELECT * FROM \"TestTaskSchema\".\"UserRequestRight\" WHERE \"userId\" = $1";
        using (var command = DataSource.CreateCommand(commandText))
        {
            command.Parameters.AddWithValue(userLogin);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                permissions.Add($"Request:{reader["rightId"]}");
            }
        }

        return permissions;
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        try
        {
            var permissions = SelectUserPermissions(userLogin);

            Logger?.Debug($"Got Permissions for User \"{userLogin}\"");

            return permissions;
        }
        catch (Exception e)
        {
            Logger?.Warn($"Failed to get Permissions for User \"{userLogin}\". {e.Message}");

            throw;
        }
    }

    ~ConnectorDb()
    {
        try
        {
            DataSource.Dispose();
        }
        catch (Exception e)
        {
            Logger?.Error($"Failed to dispose of data source. {e.Message}");
        }
    }

    public ILogger? Logger { get; set; }
}