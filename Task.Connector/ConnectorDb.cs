using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

public class ConnectorDb : IConnector
{
    private string? _connectionString;
    public ILogger Logger { get; set; }
    public void StartUp(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void CreateUser(UserToCreate user)
    {
        ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();
            using var transaction = sqlConnection.BeginTransaction();
            try
            {
                Logger.Debug("Connection established");

                // Проверка существования пользователя
                if (CheckUserExists(sqlConnection, transaction, user.Login))
                {
                    Logger.Warn($"User with login {user.Login} already exists.");
                    throw new InvalidOperationException("User already exists.");
                }

                // Добавление пользователя с набором свойств по умолчанию
                sqlConnection.Execute(
                    "INSERT INTO [TestTaskSchema].[User] " +
                    "(login, lastName, firstName, middleName, telephoneNumber, isLead) " +
                    "VALUES (@Login, @LastName, @FirstName, @MiddleName, @TelephoneNumber, @IsLead)",
                    new
                    {
                        user.Login,
                        LastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "DefaultLastName",
                        FirstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "DefaultFirstName",
                        MiddleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "DefaultMiddleName",
                        TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ?? "000-000-0000",
                        IsLead = user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value == null ? false : bool.Parse(user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value)
                    }, transaction);

                // Добавление пароля в отдельную таблицу
                sqlConnection.Execute(
                    "INSERT INTO [TestTaskSchema].[Passwords] (userId, password) VALUES (@UserId, @PasswordHash)",
                    new { UserId = user.Login, PasswordHash = user.HashPassword }, transaction);

                transaction.Commit();
                Logger.Debug("New user added.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.Warn("An error occurred while adding a user: " + ex.Message);
                throw;
            }
        }, nameof(CreateUser));
    }

    public IEnumerable<Property> GetAllProperties()
    {
        return ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();

            // Получение свойств пользователя, исключая login
            var userProperties = sqlConnection.Query<string>(
                "SELECT COLUMN_NAME " +
                "FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'User' AND TABLE_SCHEMA = 'TestTaskSchema' AND COLUMN_NAME != 'login'"
            ).Select(column => new Property(column, "User Property"));

            // Получение только поля password из таблицы Passwords
            var passwordProperties = sqlConnection.Query<string>(
                "SELECT COLUMN_NAME " +
                "FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'Passwords' AND TABLE_SCHEMA = 'TestTaskSchema' AND COLUMN_NAME = 'password'"
            ).Select(column => new Property(column, "Password Property"));

            // Объединение свойств
            var allProperties = userProperties.Concat(passwordProperties);

            return allProperties;
        }
        , nameof(GetAllProperties));
        
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        return ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();

            // Получение значений всех свойств пользователя из таблицы User
            var userProperties = sqlConnection.Query<UserProperty>(
                "SELECT 'lastName' AS Name, lastName AS Value FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
                "SELECT 'firstName', firstName FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
                "SELECT 'middleName', middleName FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
                "SELECT 'telephoneNumber', telephoneNumber FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
                "SELECT 'isLead', CAST(isLead AS NVARCHAR) FROM [TestTaskSchema].[User] WHERE login = @Login",
                new { Login = userLogin }
            );

            return userProperties;
        }, nameof(GetAllProperties));
    }

    public bool IsUserExists(string userLogin)
    {
        return ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();
            return CheckUserExists(sqlConnection, null, userLogin);
        }, nameof(IsUserExists));
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();
            using var transaction = sqlConnection.BeginTransaction();
            try
            {
                foreach (var property in properties)
                {
                    sqlConnection.Execute(
                        $"UPDATE [TestTaskSchema].[User] SET {property.Name} = @Value WHERE login = @Login",
                        new { Value = property.Value, Login = userLogin }, transaction);
                }
                transaction.Commit();
                Logger.Debug("User properties updated.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.Warn("An error occurred while updating user properties: " + ex.Message);
                throw;
            }
        }, nameof(UpdateUserProperties));
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        return ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();
            Logger.Debug("Fetching all permissions.");

            var requestRights = sqlConnection.Query(
                "SELECT id AS Id, name AS Name, 'Request' AS Description FROM [TestTaskSchema].[RequestRight]")
                .Select(row => new Permission(row.Id.ToString(), row.Name, row.Description));

            var itRoles = sqlConnection.Query(
                "SELECT id AS Id, name AS Name, 'Role' AS Description FROM [TestTaskSchema].[ItRole]")
                .Select(row => new Permission(row.Id.ToString(), row.Name, row.Description));

            return requestRights.Concat(itRoles).ToList();
        }, nameof(GetAllPermissions));
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();
            using var transaction = sqlConnection.BeginTransaction();
            try
            {
                var (requestRightIds, itRoleIds) = ParseRightIds(rightIds);

                // Добавление прав в соответствующие таблицы
                foreach (var rightId in requestRightIds)
                {
                    sqlConnection.Execute(
                        "INSERT INTO [TestTaskSchema].[UserRequestRight] (userId, RightId) VALUES (@UserId, @RightId)",
                        new { UserId = userLogin, RightId = rightId }, transaction);
                }

                foreach (var roleId in itRoleIds)
                {
                    sqlConnection.Execute(
                        "INSERT INTO [TestTaskSchema].[UserItRole] (userId, RoleId) VALUES (@UserId, @RoleId)",
                        new { UserId = userLogin, RoleId = roleId }, transaction);
                }

                transaction.Commit();
                Logger.Debug("User permissions added.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.Warn("An error occurred while adding user permissions: " + ex.Message);
                throw;
            }
        }, nameof(AddUserPermissions));
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();
            using var transaction = sqlConnection.BeginTransaction();
            try
            {
                var (requestRightIds, itRoleIds) = ParseRightIds(rightIds);

                // Удаление прав из соответствующих таблиц
                foreach (var rightId in requestRightIds)
                {
                    sqlConnection.Execute(
                        "DELETE FROM [TestTaskSchema].[UserRequestRight] WHERE userId = @UserId AND RightId = @RightId",
                        new { UserId = userLogin, RightId = rightId }, transaction);
                }

                foreach (var roleId in itRoleIds)
                {
                    sqlConnection.Execute(
                        "DELETE FROM [TestTaskSchema].[UserItRole] WHERE userId = @UserId AND roleId = @RoleId",
                        new { UserId = userLogin, RoleId = roleId }, transaction);
                }

                transaction.Commit();
                Logger.Debug("User permissions removed.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.Warn("An error occurred while removing user permissions: " + ex.Message);
                throw;
            }
        }, nameof(RemoveUserPermissions));
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        return ExecuteWithLogging(() =>
        {
            using var sqlConnection = GetOpenConnection();
            Logger.Debug("Fetching user permissions.");

            var userRequestRights = sqlConnection.Query<string>(
                "SELECT 'Request:' + CAST(RightId AS NVARCHAR) " +
                "FROM [TestTaskSchema].[UserRequestRight] " +
                "WHERE userId = @UserId",
                new { UserId = userLogin });

            var userItRoles = sqlConnection.Query<string>(
                "SELECT 'Role:' + CAST(roleId AS NVARCHAR) " +
                "FROM [TestTaskSchema].[UserItRole] " +
                "WHERE userId = @UserId",
                new { UserId = userLogin });

            return userRequestRights.Concat(userItRoles).ToList();
        }, nameof(GetUserPermissions));
    }

    private (List<int> requestRightIds, List<int> itRoleIds) ParseRightIds(IEnumerable<string> rightIds)
    {
        var parsedRightIds = rightIds.Select(id =>
        {
            var parts = id.Split(':'); // Разделение строки по делителю
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

    private T ExecuteWithLogging<T>(Func<T> func, string methodName)
    {
        Guard.Against.Null(Logger);
        try
        {
            Logger.Debug($"{methodName} started.");
            var result = func();
            Logger.Debug($"{methodName} completed successfully.");
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error($"An error occurred in {methodName}: {ex.Message}");
            throw;
        }
    }

    private void ExecuteWithLogging(Action action, string methodName)
    {
        Guard.Against.Null(Logger);
        try
        {
            Logger.Debug($"{methodName} started.");
            action();
            Logger.Debug($"{methodName} completed successfully.");
        }
        catch (Exception ex)
        {
            Logger.Error($"An error occurred in {methodName}: {ex.Message}");
            throw;
        }
    }

    private IDbConnection GetOpenConnection()
    {
        Guard.Against.Null(_connectionString);
        var sqlConnection = new SqlConnection(_connectionString);
        sqlConnection.Open();
        return sqlConnection;
    }

    private bool CheckUserExists(IDbConnection connection, IDbTransaction transaction, string login)
    {
        return connection.QuerySingleOrDefault<bool>(
            "SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END " +
            "FROM [TestTaskSchema].[User] WHERE login = @Login",
            new { Login = login }, transaction);
    }
}