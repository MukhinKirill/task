using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.Diagnostics.CodeAnalysis;
using System.Data;
using Npgsql;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Text;

namespace Task.Connector
{
  public partial class ConnectorDb : IConnector
  {
    [AllowNull]
    public ILogger Logger { get; set; } = null!;

    [NotNull]
    private DbParams _dbParams = null!;

    [NotNull]
    private Func<IDbConnection> _createConnection = null!;

    public void StartUp(string connectionString)
    {
      Logger?.Debug("Инициализация коннектора");
      try
      {
        _dbParams = DbParams.FromConnectionString(connectionString);
        SetCreateConnection();
        CheckTablesExists();

        Logger?.Debug($"Коннектор инициализирован с параметрами: {_dbParams.ToConnectionString()}");
      }

      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при инициализации коннектора: {ex.Message}");
        throw;
      }
    }

    public void CreateUser(UserToCreate user)
    {
      Logger?.Debug($"Вызов метода CreateUser с параметрами: {user.Login}, {user.HashPassword}");
      using var connection = _createConnection();
      connection.Open();

      var userPropsInfo = GetUserTablePropsInfo();
      CheckUserPropsValid(user.Properties, userPropsInfo);

      using var transaction = connection.BeginTransaction();
      try
      {
        var tablesToLock = new List<string> { _dbParams.UsersTableName!, _dbParams.PasswordsTableName! };
        LockTables(connection, tablesToLock, transaction);
        InsertUser(connection, transaction, user, userPropsInfo);
        InsertPassword(connection, transaction, user);

        transaction.Commit();
        Logger?.Debug($"Пользователь {user.Login} успешно создан.");
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        Logger?.Error($"Ошибка при создании пользователя {user.Login}: {ex.Message}");
        throw;
      }
    }

    public IEnumerable<Property> GetAllProperties()
    {
      Logger?.Debug("Получение всех доступных свойств пользователя.");
      using var connection = _createConnection();
      connection.Open();

      try
      {
        var sql = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{_dbParams.SchemaName}' AND TABLE_NAME = '{_dbParams.UsersTableName}'";

        Logger?.Debug($"Выполнение запроса: {sql}");
        var properties = connection.Query<string>(sql).ToList();

        Logger?.Debug($"Получены свойства из таблицы {_dbParams.UsersTableName}: {string.Join(", ", properties)}");

        properties.Add(_dbParams.PasswordPropName!);
        properties.Remove(_dbParams.UsersPkPropName!);

        Logger?.Debug($"Итоговые свойства пользователя: {string.Join(", ", properties)}");

        return properties.Select(name => new Property(name, "string"));
      }
      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при получении свойств пользователя: {ex.Message}");
        throw new Exception("Не удалось получить свойства пользователя", ex);
      }
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
      Logger?.Debug($"Получение свойств пользователя {userLogin}");
      using var connection = _createConnection();
      connection.Open();

      try
      {
        var user = FetchUserData(connection, userLogin);
        if (user == null)
        {
          throw new Exception($"Пользователь с логином {userLogin} не найден");
        }

        return ExtractUserProperties(user);
      }
      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при получении свойств пользователя {userLogin}: {ex.Message}");
        throw;
      }
    }

    public bool IsUserExists(string userLogin)
    {
      Logger?.Debug($"Проверка существования пользователя с логином: {userLogin}");
      using var connection = _createConnection();
      connection.Open();

      try
      {
        var userExists = connection.QuerySingleOrDefault<int>($"SELECT COUNT(*) FROM \"{_dbParams.SchemaName}\".\"{_dbParams.UsersTableName}\" WHERE \"{_dbParams.UsersPkPropName}\" = @Login", new { Login = userLogin });

        Logger?.Debug($"Пользователь с логином {userLogin} существует: {userExists > 0}");
        return userExists > 0;
      }
      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при проверке существования пользователя с логином {userLogin}: {ex.Message}");
        throw;
      }
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
      Logger?.Debug($"Обновление свойств пользователя {userLogin}");
      using var connection = _createConnection();
      connection.Open();

      var userPropsInfo = GetUserTablePropsInfo();
      CheckUserPropsValid(properties, userPropsInfo);

      using var transaction = connection.BeginTransaction();
      try
      {
        var tablesToLock = new List<string> { _dbParams.UsersTableName!, _dbParams.PasswordsTableName! };
        LockTables(connection, tablesToLock, transaction);

        UpdateUserTableProperties(connection, transaction, properties, userPropsInfo, userLogin);
        UpdateUserPassword(connection, transaction, properties, userLogin);

        transaction.Commit();
        Logger?.Debug($"Свойства пользователя {userLogin} успешно обновлены.");
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        Logger?.Error($"Ошибка при обновлении свойств пользователя {userLogin}: {ex.Message}");
        throw;
      }
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
      Logger?.Debug("Получение всех прав и ролей");
      using var connection = _createConnection();
      connection.Open();

      var permissions = new List<Permission>();

      try
      {
        var queryRequestRights = $"SELECT id, name FROM \"{_dbParams.SchemaName}\".\"{_dbParams.RequestRightsTableName}\"";

        Logger?.Debug($"Выполнение запроса: {queryRequestRights}");
        var requestRights = connection.Query<(int Id, string Name)>(queryRequestRights);

        var queryRoles = $"SELECT id, name FROM \"{_dbParams.SchemaName}\".\"{_dbParams.RolesTableName}\"";

        Logger?.Debug($"Выполнение запроса: {queryRoles}");
        var roles = connection.Query<(int Id, string Name)>(queryRoles);

        foreach (var (Id, Name) in requestRights)
        {
          permissions.Add(new Permission($"Request:{Id}", Name, string.Empty));
        }

        foreach (var (Id, Name) in roles)
        {
          permissions.Add(new Permission($"Role:{Id}", Name, string.Empty));
        }

        Logger?.Debug($"Получено {permissions.Count} прав и ролей");
      }
      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при получении прав и ролей: {ex.Message}");
        throw;
      }

      return permissions;
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
      Logger?.Debug($"Добавление прав для пользователя {userLogin} с правами: {string.Join(", ", rightIds)}");
      using var connection = _createConnection();
      connection.Open();

      using var transaction = connection.BeginTransaction();
      try
      {
        var (roleIds, rightRequestIds) = SeparatePermissions(rightIds);

        var tablesToLock = new List<string>
        {
            _dbParams.UsersRolesTableName!,
            _dbParams.UsersRequestRightsTableName!
        };
        LockTables(connection, tablesToLock, transaction);
        InsertUserRoles(connection, transaction, userLogin, roleIds);
        InsertUserRights(connection, transaction, userLogin, rightRequestIds);

        transaction.Commit();
        Logger?.Debug($"Права для пользователя {userLogin} успешно добавлены.");
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        Logger?.Error($"Ошибка при добавлении прав для пользователя {userLogin}: {ex.Message}");
        throw;
      }
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
      Logger?.Debug($"Удаление прав для пользователя {userLogin} с правами: {string.Join(", ", rightIds)}");
      using var connection = _createConnection();
      connection.Open();

      using var transaction = connection.BeginTransaction();
      try
      {
        var tablesToLock = new List<string>
        {
            _dbParams.UsersRolesTableName!,
            _dbParams.UsersRequestRightsTableName!
        };
        LockTables(connection, tablesToLock, transaction);

        var (roleIds, rightRequestIds) = SeparatePermissions(rightIds);

        RemoveUserRoles(connection, transaction, userLogin, roleIds);
        RemoveUserRights(connection, transaction, userLogin, rightRequestIds);

        transaction.Commit();
        Logger?.Debug($"Права для пользователя {userLogin} успешно удалены.");
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        Logger?.Error($"Ошибка при удалении прав для пользователя {userLogin}: {ex.Message}");
        throw;
      }
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
      Logger?.Debug($"Получение прав для пользователя {userLogin}");
      using var connection = _createConnection();
      connection.Open();

      try
      {
        var roles = GetUserRoles(connection, userLogin);
        var rights = GetUserRights(connection, userLogin);

        var permissions = new List<string>();
        permissions.AddRange(roles.Select(r => $"Role:{r.Id}"));
        permissions.AddRange(rights.Select(r => $"Request:{r.Id}"));

        Logger?.Debug($"Получено {permissions.Count} прав и ролей для пользователя {userLogin}");
        return permissions;
      }
      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при получении прав для пользователя {userLogin}: {ex.Message}");
        throw;
      }
    }

    private void SetCreateConnection()
    {
      var providerToUpper = _dbParams.Provider.ToUpper();

      if (providerToUpper.Contains("POSTGRES"))
      {
        _createConnection = () => new NpgsqlConnection(_dbParams.ConnectionString);
        return;
      }

      if (providerToUpper.Contains("SQLSERVER"))
      {
        _createConnection = () => new SqlConnection(_dbParams.ConnectionString);
        return;
      }

      throw new NotSupportedException($"Провайдер {_dbParams.Provider} не поддерживается");
    }

    private void CheckTablesExists()
    {
      Logger?.Debug("Проверка существования таблиц");

      using var connection = _createConnection();
      connection.Open();
      var tableNames = new List<string>
      {
        _dbParams.UsersTableName!,
        _dbParams.PasswordsTableName!,
        _dbParams.RolesTableName!,
        _dbParams.RequestRightsTableName!,
        _dbParams.UsersRolesTableName!,
        _dbParams.UsersRequestRightsTableName!,
      };

      var nonExistentTables = new List<string>();
      foreach (var tableName in tableNames)
      {
        var tableExists = connection.QuerySingleOrDefault<int>($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{_dbParams.SchemaName}' AND table_name = '{tableName}'") > 0;

        if (!tableExists)
        {
          nonExistentTables.Add(tableName);
        }
      }

      if (nonExistentTables.Count > 0)
      {
        throw new Exception($"Таблицы {string.Join(", ", nonExistentTables)} не существуют.");
      }
    }

    private void LockTables(IDbConnection connection, IEnumerable<string> tableNames, IDbTransaction? transaction = null)
    {
      if (tableNames == null || !tableNames.Any())
      {
        Logger?.Warn("Не указаны таблицы для блокировки");
        return;
      }

      var lockSql = new StringBuilder();

      if (_dbParams.Provider.ToUpper().Contains("POSTGRES"))
      {
        Logger?.Debug("Блокировка таблиц для PostgreSQL");
        foreach (var tableName in tableNames)
        {
          lockSql.AppendLine($"LOCK TABLE \"{_dbParams.SchemaName}\".\"{tableName}\" IN EXCLUSIVE MODE;");
        }
      }
      else if (_dbParams.Provider.ToUpper().Contains("SQLSERVER"))
      {
        Logger?.Debug("Блокировка таблиц для SQL Server");
        foreach (var tableName in tableNames)
        {
          lockSql.AppendLine($"SELECT TOP 1 * FROM [{_dbParams.SchemaName}].[{tableName}] WITH (TABLOCKX);");
        }
      }
      else
      {
        throw new NotSupportedException($"Провайдер {_dbParams.Provider} не поддерживается");
      }

      if (lockSql.Length > 0)
      {
        Logger?.Debug($"Выполнение запроса блокировки: {lockSql}");
        connection.Execute(lockSql.ToString(), transaction: transaction);
      }
    }

    private void InsertUser(IDbConnection connection, IDbTransaction transaction, UserToCreate user, IEnumerable<UserPropInfo> userPropsInfo)
    {
      var (sql, parameters) = BuildInsertUserSql(user, userPropsInfo);
      Logger?.Debug($"Выполнение запроса: {sql}");
      connection.Execute(sql, parameters, transaction);
    }

    private (string sql, DynamicParameters parameters) BuildInsertUserSql(UserToCreate user, IEnumerable<UserPropInfo> userPropsInfo)
    {
      var sql = new StringBuilder($"INSERT INTO \"{_dbParams.SchemaName}\".\"{_dbParams.UsersTableName}\" (\"{_dbParams.UsersPkPropName}\"");
      var valuesSql = new StringBuilder("(@Login");
      var parameters = new DynamicParameters();
      parameters.Add("Login", user.Login);

      var properties = user.Properties.ToDictionary(p => p.Name, p => p.Value);

      foreach (var propInfo in userPropsInfo)
      {
        sql.Append($", \"{propInfo.Name}\"");
        valuesSql.Append($", @{propInfo.Name}");

        object paramValue;
        if (properties.TryGetValue(propInfo.Name, out string? propValue))
        {
          paramValue = ParseValue(propValue, propInfo.Type);
        }
        else if (propInfo.IsNotNull && !propInfo.HaveDefaultValue)
        {
          paramValue = GetDefaultValueForType(propInfo.Type);
        }
        else
        {
          paramValue = DBNull.Value;
        }
        parameters.Add(propInfo.Name, paramValue);
      }

      sql.Append(") VALUES ");
      sql.Append(valuesSql);
      sql.Append(")");

      return (sql.ToString(), parameters);
    }

    private void InsertPassword(IDbConnection connection, IDbTransaction transaction, UserToCreate user)
    {
      var sql = $"INSERT INTO \"{_dbParams.SchemaName}\".\"{_dbParams.PasswordsTableName}\" (\"{_dbParams.PasswordsFkUser}\", \"{_dbParams.PasswordPropName}\") VALUES (@UserId, @Password)";
      var parameters = new DynamicParameters();
      parameters.Add("UserId", user.Login);
      parameters.Add("Password", user.HashPassword);

      Logger?.Debug($"Выполнение запроса: {sql}");
      connection.Execute(sql, parameters, transaction);
    }

    private object GetDefaultValueForType(string dbType)
    {
      return dbType.ToLower() switch
      {
        "integer" or "int" or "bigint" or "numeric" or "decimal" or "double precision" or "float" => 0,
        "boolean" or "bool" => false,
        "date" or "timestamp" or "datetime" => DateTime.MinValue,
        _ => string.Empty
      };
    }

    private object ParseValue(string value, string dbType)
    {
      if (string.IsNullOrEmpty(value))
      {
        return GetDefaultValueForType(dbType);
      }

      return dbType.ToLower() switch
      {
        "integer" or "int" => int.TryParse(value, out int intResult) ? intResult : 0,
        "bigint" => long.TryParse(value, out long longResult) ? longResult : 0L,
        "numeric" or "decimal" => decimal.TryParse(value, out decimal decimalResult) ? decimalResult : 0m,
        "double precision" or "float" => double.TryParse(value, out double doubleResult) ? doubleResult : 0.0,
        "boolean" or "bool" => bool.TryParse(value, out bool boolResult) ? boolResult : false,
        "date" or "timestamp" or "datetime" => DateTime.TryParse(value, out DateTime dateResult) ? dateResult : DateTime.MinValue,
        _ => value
      };
    }

    private void CheckUserPropsValid(IEnumerable<UserProperty> properties, IEnumerable<UserPropInfo> userPropsInfo)
    {
      Logger?.Debug("Проверка свойств пользователя на соответствие таблице");

      foreach (var property in properties)
      {
        if (property.Name != _dbParams.PasswordPropName && !userPropsInfo.Any(p => p.Name == property.Name))
        {
          throw new Exception($"Свойство {property.Name} не найдено в таблице {_dbParams.UsersTableName}");
        }
      }
    }

    private IEnumerable<UserPropInfo> GetUserTablePropsInfo()
    {
      Logger?.Debug("Получение информации о свойствах пользователя.");
      using var connection = _createConnection();
      connection.Open();

      try
      {
        var query = $@"
          SELECT column_name, data_type, is_nullable, column_default
          FROM information_schema.columns
          WHERE table_schema = '{_dbParams.SchemaName}'
          AND table_name = '{_dbParams.UsersTableName}'
          AND column_name <> '{_dbParams.UsersPkPropName}'";

        var properties = connection.Query<dynamic>(query).Select(row => new UserPropInfo
        {
          Name = row.column_name,
          Type = row.data_type,
          IsNotNull = row.is_nullable == "NO",
          HaveDefaultValue = row.column_default != null
        }).ToList();

        Logger?.Debug($"Получены свойства: {string.Join(", ", properties.Select(p => $"{p.Name} (Тип: {p.Type}, NOT_NULL: {p.IsNotNull}, Значение по умолчанию: {p.HaveDefaultValue})"))}");
        return properties;
      }
      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при получении информации о свойствах пользователя: {ex.Message}");
        throw;
      }
    }

    private dynamic FetchUserData(IDbConnection connection, string userLogin)
    {
      var selectSql = BuildSelectUserSql();
      var sqlParam = new { Login = userLogin };

      Logger?.Debug($"Выполнение запроса: {selectSql} \n с параметром: {sqlParam}");
      return connection.QuerySingleOrDefault(selectSql, sqlParam);
    }

    private string BuildSelectUserSql()
    {
      return $@"
        SELECT u.*,
               p.""{_dbParams.PasswordPropName}"" as Password
        FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersTableName}"" u
        JOIN ""{_dbParams.SchemaName}"".""{_dbParams.PasswordsTableName}"" p
            ON u.""{_dbParams.UsersPkPropName}"" = p.""{_dbParams.PasswordsFkUser}""
        WHERE u.""{_dbParams.UsersPkPropName}"" = @Login";
    }

    private IEnumerable<UserProperty> ExtractUserProperties(dynamic user)
    {
      var userProperties = new List<UserProperty>();
      foreach (var prop in (IDictionary<string, object>)user)
      {
        if (prop.Key != _dbParams.UsersPkPropName && prop.Value != null && prop.Value != DBNull.Value)
        {
          string stringValue = ConvertToString(prop.Value, prop.Value.GetType().Name);
          userProperties.Add(new UserProperty(prop.Key, stringValue));
        }
      }

      Logger?.Debug($"Получено {userProperties.Count} свойств для пользователя");
      return userProperties;
    }

    private string ConvertToString(object value, string typeName)
    {
      if (value == null || value == DBNull.Value)
      {
        return string.Empty;
      }

      switch (typeName.ToLower())
      {
        case "datetime":
          return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
        case "boolean":
          return ((bool)value).ToString().ToLower();
        default:
          return value.ToString();
      }
    }

    private void UpdateUserTableProperties(IDbConnection connection, IDbTransaction transaction,
    IEnumerable<UserProperty> properties, IEnumerable<UserPropInfo> userPropsInfo, string userLogin)
    {
      var (updateSql, parameters) = BuildUpdateUserSql(properties, userPropsInfo, userLogin);
      if (!string.IsNullOrEmpty(updateSql))
      {
        Logger?.Debug($"Выполнение запроса: {updateSql}\n с параметрами: {string.Join(", ", parameters.ParameterNames.Select(name => $"{name} = {parameters.Get<object>(name)}"))}");
        var affectedRows = connection.Execute(updateSql, parameters, transaction);

        if (affectedRows == 0)
        {
          throw new Exception($"Пользователь с логином {userLogin} не найден");
        }
      }
    }

    private (string sql, DynamicParameters parameters) BuildUpdateUserSql(IEnumerable<UserProperty> properties,
    IEnumerable<UserPropInfo> userPropsInfo, string userLogin)
    {
      var setClauses = new List<string>();
      var parameters = new DynamicParameters();
      parameters.Add("Login", userLogin);

      foreach (var property in properties)
      {
        var propInfo = userPropsInfo.FirstOrDefault(p => p.Name == property.Name);
        if (propInfo != null)
        {
          setClauses.Add($"\"{property.Name}\" = @{property.Name}");
          object parsedValue = ParseValue(property.Value, propInfo.Type);
          parameters.Add(property.Name, parsedValue);
        }
      }

      if (setClauses.Count == 0)
      {
        return (string.Empty, parameters);
      }

      var updateSql = $"UPDATE \"{_dbParams.SchemaName}\".\"{_dbParams.UsersTableName}\" SET {string.Join(", ", setClauses)} WHERE \"{_dbParams.UsersPkPropName}\" = @Login";
      return (updateSql, parameters);
    }

    private void UpdateUserPassword(IDbConnection connection, IDbTransaction transaction,
    IEnumerable<UserProperty> properties, string userLogin)
    {
      var passwordProperty = properties.FirstOrDefault(p => p.Name == _dbParams.PasswordPropName);
      if (passwordProperty != null)
      {
        var passwordUpdateSql = $@"
            UPDATE ""{_dbParams.SchemaName}"".""{_dbParams.PasswordsTableName}""
            SET ""{_dbParams.PasswordPropName}"" = @Password
            WHERE ""{_dbParams.PasswordsFkUser}"" = @Login";
        var passwordParams = new { Password = passwordProperty.Value, Login = userLogin };

        Logger?.Debug($"Выполнение запроса обновления пароля: {passwordUpdateSql}\n с параметрами: {passwordParams}");
        connection.Execute(passwordUpdateSql, passwordParams, transaction);
      }
    }

    private (List<string> roleIds, List<string> rightRequestIds) SeparatePermissions(IEnumerable<string> rightIds)
    {
      var roleIds = new List<string>();
      var rightRequestIds = new List<string>();

      foreach (var item in rightIds ?? Enumerable.Empty<string>())
      {
        var parts = item.Split(':');
        if (parts.Length == 2)
        {
          if (parts[0] == "Role")
          {
            roleIds.Add(parts[1]);
          }
          else if (parts[0] == "Request")
          {
            rightRequestIds.Add(parts[1]);
          }
          else
          {
            Logger?.Warn($"Неизвестный префикс: {parts[0]} для элемента {item}");
          }
        }
        else
        {
          Logger?.Warn($"Некорректный формат элемента: {item}");
        }
      }

      return (roleIds, rightRequestIds);
    }

    private void InsertUserRoles(IDbConnection connection, IDbTransaction transaction, string userLogin, List<string> roleIds)
    {
      if (!roleIds.Any()) return;

      var roleSql = $@"
        INSERT INTO ""{_dbParams.SchemaName}"".""{_dbParams.UsersRolesTableName}""
        (""{_dbParams.UsersRolesFkUser}"", ""roleId"")
        VALUES (@UserId, @RoleId)";

      foreach (var roleId in roleIds)
      {
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userLogin);
        parameters.Add("RoleId", int.Parse(roleId));

        Logger?.Debug($"Выполнение запроса для добавления роли с ID {roleId}: {roleSql}");
        connection.Execute(roleSql, parameters, transaction);
      }
    }

    private void InsertUserRights(IDbConnection connection, IDbTransaction transaction, string userLogin, List<string> rightRequestIds)
    {
      if (!rightRequestIds.Any()) return;

      var rightSql = $@"
        INSERT INTO ""{_dbParams.SchemaName}"".""{_dbParams.UsersRequestRightsTableName}""
        (""{_dbParams.UsersRequestRightsFkUser}"", ""rightId"")
        VALUES (@UserId, @RightId)";

      foreach (var rightId in rightRequestIds)
      {
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userLogin);
        parameters.Add("RightId", int.Parse(rightId));

        Logger?.Debug($"Выполнение запроса для добавления права с ID {rightId}: {rightSql}");
        connection.Execute(rightSql, parameters, transaction);
      }
    }

    private void RemoveUserRoles(IDbConnection connection, IDbTransaction transaction, string userLogin, List<string> roleIds)
    {
      if (!roleIds.Any()) return;

      var roleSql = $@"
        DELETE FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersRolesTableName}""
        WHERE ""{_dbParams.UsersRolesFkUser}"" = @UserId AND ""roleId"" = @RoleId";

      foreach (var roleId in roleIds)
      {
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userLogin);
        parameters.Add("RoleId", int.Parse(roleId));

        Logger?.Debug($"Выполнение запроса для удаления роли с ID {roleId}: {roleSql}");
        var affectedRows = connection.Execute(roleSql, parameters, transaction);
        if (affectedRows == 0)
        {
          Logger?.Warn($"Роль с ID {roleId} не была найдена для пользователя {userLogin}");
        }
      }
    }

    private void RemoveUserRights(IDbConnection connection, IDbTransaction transaction, string userLogin, List<string> rightRequestIds)
    {
      if (!rightRequestIds.Any()) return;

      var rightSql = $@"
        DELETE FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersRequestRightsTableName}""
        WHERE ""{_dbParams.UsersRequestRightsFkUser}"" = @UserId AND ""rightId"" = @RightId";

      foreach (var rightId in rightRequestIds)
      {
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userLogin);
        parameters.Add("RightId", int.Parse(rightId));

        Logger?.Debug($"Выполнение запроса для удаления права с ID {rightId}: {rightSql}");
        var affectedRows = connection.Execute(rightSql, parameters, transaction);
        if (affectedRows == 0)
        {
          Logger?.Warn($"Право с ID {rightId} не было найдено для пользователя {userLogin}");
        }
      }
    }

    private IEnumerable<(int Id, string Name)> GetUserRoles(IDbConnection connection, string userLogin)
    {
      var rolesSql = $@"
        SELECT r.id, r.name
        FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersRolesTableName}"" ur
        JOIN ""{_dbParams.SchemaName}"".""{_dbParams.RolesTableName}"" r ON ur.""roleId"" = r.id
        WHERE ur.""{_dbParams.UsersRolesFkUser}"" = @UserId";

      Logger?.Debug($"Выполнение запроса для получения ролей: {rolesSql}");
      return connection.Query<(int Id, string Name)>(rolesSql, new { UserId = userLogin });
    }

    private IEnumerable<(int Id, string Name)> GetUserRights(IDbConnection connection, string userLogin)
    {
      var rightsSql = $@"
        SELECT rr.id, rr.name
        FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersRequestRightsTableName}"" urr
        JOIN ""{_dbParams.SchemaName}"".""{_dbParams.RequestRightsTableName}"" rr ON urr.""rightId"" = rr.id
        WHERE urr.""{_dbParams.UsersRequestRightsFkUser}"" = @UserId";

      Logger?.Debug($"Выполнение запроса для получения прав: {rightsSql}");
      return connection.Query<(int Id, string Name)>(rightsSql, new { UserId = userLogin });
    }
  }

}

