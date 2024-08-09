using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.Diagnostics.CodeAnalysis;
using System.Data;
using Npgsql;
using Microsoft.Data.SqlClient;
using Dapper;

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


    public void CreateUser(UserToCreate user)
    {
      Logger?.Debug($"Вызов метода CreateUser с параметрами: {user.Login}, {user.HashPassword}");
      using var connection = _createConnection();
      connection.Open();

      var userPropsInfo = GetCurrentUserPropsInfo();

      CheckUserPropsValid(user.Properties, userPropsInfo);

      using var transaction = connection.BeginTransaction();
      try
      {
        Logger?.Debug($"Блокировка таблиц для PostgreSQL");
        var lockSql = $@"
                LOCK TABLE ""{_dbParams.SchemaName}"".""{_dbParams.UsersTableName}"" IN EXCLUSIVE MODE;
                LOCK TABLE ""{_dbParams.SchemaName}"".""{_dbParams.PasswordsTableName}"" IN EXCLUSIVE MODE;";
        connection.Execute(lockSql, transaction: transaction);

        var userSql = $"INSERT INTO \"{_dbParams.SchemaName}\".\"{_dbParams.UsersTableName}\" (\"{_dbParams.UsersPkPropName}\"";

        var properties = user.Properties.ToDictionary(p => p.Name, p => p.Value);
        var parameters = new DynamicParameters();
        parameters.Add("Login", user.Login);

        foreach (var propInfo in userPropsInfo)
        {
          userSql += $", \"{propInfo.Name}\"";
          if (properties.TryGetValue(propInfo.Name, out string propValue))
          {
            object parsedValue = ParseValue(propValue, propInfo.Type);
            parameters.Add(propInfo.Name, parsedValue);
          }
          else if (propInfo.IsNotNull && propInfo.DefaultValue == null)
          {
            object defaultValue = GetDefaultValueForType(propInfo.Type);
            parameters.Add(propInfo.Name, defaultValue);
          }
          else
          {
            parameters.Add(propInfo.Name, null);
          }
        }

        userSql += $") VALUES (@Login, {string.Join(", ", userPropsInfo.Select(p => $"@{p.Name}"))})";


        Logger?.Debug($"Выполнение запроса: {userSql}");
        connection.Execute(userSql, parameters, transaction);

        var passwordSql = $"INSERT INTO \"{_dbParams.SchemaName}\".\"{_dbParams.PasswordsTableName}\" (\"{_dbParams.PasswordsFkUser}\", \"{_dbParams.PasswordPropName}\") VALUES (@UserId, @Password)";
        parameters = new DynamicParameters();
        parameters.Add("UserId", user.Login);
        parameters.Add("Password", user.HashPassword);

        Logger?.Debug($"Выполнение запроса: {passwordSql}");
        connection.Execute(passwordSql, parameters, transaction);

        transaction.Commit();
        Logger?.Debug($"Пользователь {user.Login} успешно создан.");
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        Logger?.Error($"Ошибка при создании пользователя с данными {nameof(user.Login)}: {user.Login}, {nameof(user.HashPassword)}: {user.HashPassword}, {nameof(user.Properties)}: {user.Properties} : {ex.Message}");
        throw;
      }
    }

    private object GetDefaultValueForType(string dbType)
    {
      switch (dbType.ToLower())
      {
        case "integer":
        case "int":
          return 0;
        case "bigint":
          return 0L;
        case "numeric":
        case "decimal":
          return 0m;
        case "double precision":
        case "float":
          return 0.0;
        case "boolean":
        case "bool":
          return false;
        case "date":
        case "timestamp":
        case "datetime":
          return DateTime.MinValue;
        case "character varying":
        case "varchar":
        case "text":
          return string.Empty;
        default:
          return DBNull.Value;
      }
    }

    private object ParseValue(string value, string dbType)
    {
      if (string.IsNullOrEmpty(value))
      {
        return GetDefaultValueForType(dbType);
      }

      switch (dbType.ToLower())
      {
        case "integer":
        case "int":
          return int.TryParse(value, out int intResult) ? intResult : GetDefaultValueForType(dbType);
        case "bigint":
          return long.TryParse(value, out long longResult) ? longResult : GetDefaultValueForType(dbType);
        case "numeric":
        case "decimal":
          return decimal.TryParse(value, out decimal decimalResult) ? decimalResult : GetDefaultValueForType(dbType);
        case "double precision":
        case "float":
          return double.TryParse(value, out double doubleResult) ? doubleResult : GetDefaultValueForType(dbType);
        case "boolean":
        case "bool":
          return bool.TryParse(value, out bool boolResult) ? boolResult : GetDefaultValueForType(dbType);
        case "date":
        case "timestamp":
        case "datetime":
          return DateTime.TryParse(value, out DateTime dateResult) ? dateResult : GetDefaultValueForType(dbType);
        default:
          return value;
      }
    }

    private void CheckUserPropsValid(IEnumerable<UserProperty> properties, IEnumerable<UserPropInfo> userPropsInfo)
    {
      Logger?.Debug("Проверка свойств пользователя на соответствие таблице");

      foreach (var property in properties)
      {
        if (!userPropsInfo.Any(p => p.Name == property.Name))
        {
          throw new Exception($"Свойство {property.Name} не найдено в таблице {_dbParams.UsersTableName}");
        }
      }
    }

    private IEnumerable<UserPropInfo> GetCurrentUserPropsInfo()
    {
      Logger?.Debug("Получение информации о свойствах пользователя.");
      using var connection = _createConnection();
      connection.Open();

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
        DefaultValue = row.column_default
      }).ToList();

      Logger?.Debug($"Получены свойства: {string.Join(", ", properties.Select(p => $"{p.Name} (Тип: {p.Type}, NOT_NULL: {p.IsNotNull}, Значение по умолчанию: {p.DefaultValue})"))}");
      return properties;
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
      Logger?.Debug($"Получение свойств пользователя {userLogin}");
      using var connection = _createConnection();
      connection.Open();

      var userProperties = new List<UserProperty>();

      try
      {
        var userPropsInfo = GetCurrentUserPropsInfo();

        var selectSql = $@"
            SELECT {string.Join(", ", userPropsInfo.Select(p => $"\"{p.Name}\""))}
            FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersTableName}""
            WHERE ""{_dbParams.UsersPkPropName}"" = @Login";

        var user = connection.QuerySingleOrDefault(selectSql, new { Login = userLogin });

        if (user == null)
        {
          Logger?.Warn($"Пользователь с логином {userLogin} не найден");
          return Enumerable.Empty<UserProperty>();
        }

        foreach (var propInfo in userPropsInfo)
        {
          var value = ((IDictionary<string, object>)user)[propInfo.Name];
          if (value != null)
          {
            string stringValue = ConvertToString(value, propInfo.Type);
            userProperties.Add(new UserProperty(propInfo.Name, stringValue));
          }
        }

        Logger?.Debug($"Получено {userProperties.Count} свойств для пользователя {userLogin}");
      }
      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при получении свойств пользователя {userLogin}: {ex.Message}");
        throw;
      }

      return userProperties;
    }

    private string ConvertToString(object value, string dbType)
    {
      if (value == null || value == DBNull.Value)
      {
        return null;
      }

      switch (dbType.ToLower())
      {
        case "date":
        case "timestamp":
        case "datetime":
          return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
        case "boolean":
        case "bool":
          return ((bool)value).ToString().ToLower();
        default:
          return value.ToString();
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

      var userPropsInfo = GetCurrentUserPropsInfo();
      CheckUserPropsValid(properties, userPropsInfo);

      using var transaction = connection.BeginTransaction();
      try
      {
        var updateSql = $"UPDATE \"{_dbParams.SchemaName}\".\"{_dbParams.UsersTableName}\" SET ";
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
          else
          {
            Logger?.Warn($"Свойство {property.Name} не найдено в информации о свойствах пользователя и будет пропущено");
          }
        }

        if (setClauses.Count == 0)
        {
          Logger?.Warn("Нет свойств для обновления");
          return;
        }

        updateSql += string.Join(", ", setClauses);
        updateSql += $" WHERE \"{_dbParams.UsersPkPropName}\" = @Login";

        Logger?.Debug($"Выполнение запроса: {updateSql}");
        var affectedRows = connection.Execute(updateSql, parameters, transaction);

        if (affectedRows == 0)
        {
          throw new Exception($"Пользователь с логином {userLogin} не найден");
        }

        transaction.Commit();
        Logger?.Debug($"Свойства пользователя {userLogin} успешно обновлены. Затронуто строк: {affectedRows}");
      }
      catch (Exception ex)
      {
        transaction.Rollback();
        Logger?.Error($"Ошибка при обновлении свойств пользователя {userLogin}: {ex.Message}");
        throw;
      }
    }

    public IEnumerable<Property> GetAllProperties()
    {
      using var connection = _createConnection();
      connection.Open();

      Logger?.Debug($"Открыто соединение с базой данных для получения свойств пользователя.");

      var properties = connection.Query<string>($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{_dbParams.SchemaName}' AND TABLE_NAME = '{_dbParams.UsersTableName}'").ToList();

      Logger?.Debug($"Получены свойства из таблицы {_dbParams.UsersTableName}: {string.Join(", ", properties)}");

      properties.Add(_dbParams.PasswordPropName!);
      properties.Remove(_dbParams.UsersPkPropName!);

      Logger?.Debug($"Свойства после добавления и удаления: {string.Join(", ", properties)}");

      return properties.Select(name => new Property(name, "string"));
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
      Logger?.Debug("Получение всех прав и ролей");
      using var connection = _createConnection();
      connection.Open();

      var permissions = new List<Permission>();

      try
      {
        // Запрос для получения прав с их идентификаторами
        var queryRequestRights = $"SELECT id, name FROM \"{_dbParams.SchemaName}\".\"{_dbParams.RequestRightsTableName}\"";
        var requestRights = connection.Query<(int Id, string Name)>(queryRequestRights);

        // Запрос для получения ролей с их идентификаторами
        var queryRoles = $"SELECT id, name FROM \"{_dbParams.SchemaName}\".\"{_dbParams.RolesTableName}\"";
        var roles = connection.Query<(int Id, string Name)>(queryRoles);

        // Добавление прав в список разрешений
        foreach (var (Id, Name) in requestRights)
        {
          permissions.Add(new Permission($"Request:{Id}", Name, string.Empty));
        }

        // Добавление ролей в список разрешений
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

        // Вставка ролей
        if (roleIds.Any())
        {
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

        // Вставка прав
        if (rightRequestIds.Any())
        {
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
        var roleIds = new List<string>();
        var rightIdsToRemove = new List<string>();

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
              rightIdsToRemove.Add(parts[1]);
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

        // Удаление ролей
        if (roleIds.Any())
        {
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

        // Удаление прав
        if (rightIdsToRemove.Any())
        {
          var rightSql = $@"
                DELETE FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersRequestRightsTableName}""
                WHERE ""{_dbParams.UsersRequestRightsFkUser}"" = @UserId AND ""rightId"" = @RightId";

          foreach (var rightId in rightIdsToRemove)
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

      var permissions = new List<string>();

      try
      {
        // Получение ролей пользователя
        var rolesSql = $@"
            SELECT r.id, r.name
            FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersRolesTableName}"" ur
            JOIN ""{_dbParams.SchemaName}"".""{_dbParams.RolesTableName}"" r ON ur.""roleId"" = r.id
            WHERE ur.""{_dbParams.UsersRolesFkUser}"" = @UserId";

        var roles = connection.Query<(int Id, string Name)>(rolesSql, new { UserId = userLogin });

        foreach (var (Id, Name) in roles)
        {
          permissions.Add($"Role:{Id}");
        }

        // Получение прав пользователя
        var rightsSql = $@"
            SELECT rr.id, rr.name
            FROM ""{_dbParams.SchemaName}"".""{_dbParams.UsersRequestRightsTableName}"" urr
            JOIN ""{_dbParams.SchemaName}"".""{_dbParams.RequestRightsTableName}"" rr ON urr.""rightId"" = rr.id
            WHERE urr.""{_dbParams.UsersRequestRightsFkUser}"" = @UserId";

        var rights = connection.Query<(int Id, string Name)>(rightsSql, new { UserId = userLogin });

        foreach (var (Id, Name) in rights)
        {
          permissions.Add($"Request:{Id}");
        }

        Logger?.Debug($"Получено {permissions.Count} прав и ролей для пользователя {userLogin}");
      }
      catch (Exception ex)
      {
        Logger?.Error($"Ошибка при получении прав для пользователя {userLogin}: {ex.Message}");
        throw;
      }

      return permissions;
    }


  }

}

