using System.Data;
using Task.Integration.Data.Models.Models;
using Dapper;
using Task.Integration.Data.Models;

namespace Task.Connector.Repositories;

public interface IUserRepository
{
    void CreateUser(UserToCreate user);
    bool IsUserExists(string userLogin);
    IEnumerable<Property> GetAllProperties();

    IEnumerable<UserProperty> GetUserProperties(string userLogin);
    void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
}

public class UserRepository : RepositoryBase, IUserRepository
{
    private readonly ILogger? _logger;

    private readonly IDbConnection _dbConnection;

    public UserRepository(ILogger? logger, IDbConnection dbConnection, string? schemaName) : base(dbConnection,
        schemaName)
    {
        _logger = logger;
        _dbConnection = dbConnection;
    }

    public void CreateUser(UserToCreate user)
    {
        if (IsUserExists(user.Login))
        {
            _logger?.Error($"User with login {user.Login} already exists.");
            return;
        }

        EnsureConnectionOpened();

        const string sql = """
                               INSERT INTO "User" ("login", "lastName", "firstName", "middleName", "telephoneNumber", "isLead")
                               VALUES (@Login, @LastName, @FirstName, @MiddleName, @TelephoneNumber, @IsLead);
                           """;

        var parameters = new
        {
            user.Login,
            LastName = GetPropertyValue(user.Properties, "lastName"),
            FirstName = GetPropertyValue(user.Properties, "firstName"),
            MiddleName = GetPropertyValue(user.Properties, "middleName"),
            TelephoneNumber = GetPropertyValue(user.Properties, "telephoneNumber"),
            IsLead = GetPropertyValue(user.Properties, "isLead") == "true"
        };

        try
        {
            _dbConnection.Execute(sql, parameters);
        }
        catch (Exception)
        {
            _logger?.Error($"Error while creating user {user.Login}");
            throw;
        }
    }

    public bool IsUserExists(string userLogin)
    {
        EnsureConnectionOpened();

        var sql = """
                  SELECT COUNT(1) 
                  FROM "User"
                  WHERE "login" = @Login
                  """;

        var count = _dbConnection.ExecuteScalar<int>(sql, new { Login = userLogin });
        return count > 0;
    }

    public IEnumerable<Property> GetAllProperties()
    {
        EnsureConnectionOpened();

        var sql = """
                  SELECT column_name AS Name,
                         CASE 
                             WHEN column_name = 'lastName' THEN 'Last name'
                             WHEN column_name = 'firstName' THEN 'First name'
                             WHEN column_name = 'middleName' THEN 'Middle name'
                             WHEN column_name = 'telephoneNumber' THEN 'Telephone number'
                             WHEN column_name = 'isLead' THEN 'Is lead'
                             WHEN column_name = 'password' THEN 'Password'
                             ELSE column_name
                         END AS Description
                  FROM information_schema.columns
                  WHERE table_name IN ('User', 'Passwords')
                  AND column_name != 'login'
                  AND column_name != 'userId'
                  AND column_name != 'id'
                  """;

        var properties = _dbConnection.Query<Property>(sql)
            .Select(p => new Property(p.Name, p.Description));

        return properties;
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        EnsureConnectionOpened();

        var sql = """
                  SELECT 'lastName' AS Name, "lastName" AS Value FROM "User" WHERE "login" = @Login
                  UNION ALL
                  SELECT 'firstName' AS Name, "firstName" AS Value FROM "User" WHERE "login" = @Login
                  UNION ALL
                  SELECT 'middleName' AS Name, "middleName" AS Value FROM "User" WHERE "login" = @Login
                  UNION ALL
                  SELECT 'telephoneNumber' AS Name, "telephoneNumber" AS Value FROM "User" WHERE "login" = @Login
                  UNION ALL
                  SELECT 'isLead' AS Name, 
                         CASE WHEN "isLead" THEN 'true' ELSE 'false' END AS Value 
                  FROM "User" WHERE "login" = @Login;
                  """;

        var result = _dbConnection.Query<UserProperty>(sql, new { Login = userLogin });
        return result;
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        EnsureConnectionOpened();

        var setClauses = new List<string>();
        var parameters = new Dictionary<string, object>
        {
            { "@Login", userLogin }
        };

        foreach (var property in properties)
        {
            if (string.IsNullOrWhiteSpace(property.Name))
            {
                _logger?.Warn($"Skipping empty or invalid property: {property.Name} for user {userLogin}");
                continue;
            }

            setClauses.Add($"\"{property.Name}\" = @Value_{property.Name}");
            parameters.Add($"@Value_{property.Name}", property.Value);
        }

        if (!setClauses.Any())
        {
            _logger?.Warn($"No valid properties to update for user: {userLogin}");
            return;
        }

        var sql = $"UPDATE \"User\" SET {string.Join(", ", setClauses)} WHERE login = @Login";

        try
        {
            _dbConnection.Execute(sql, parameters);
        }
        catch (Exception)
        {
            _logger?.Error($"Error occurred while updating properties for user: {userLogin}");
            throw;
        }
    }

    private string GetPropertyValue(IEnumerable<UserProperty> properties, string propertyName,
        string defaultValue = "Unknown")
    {
        return properties.FirstOrDefault(p => p.Name == propertyName)?.Value ?? defaultValue;
    }
}