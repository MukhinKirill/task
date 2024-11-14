using System.Data;
using System.Data.Common;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using Task.Connector.DomainModels;
using Task.Connector.Exceptions;
using Task.Connector.Resources;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private String? _connectionString;

        private const String LoginParameter = "@login";
        private const String UserIdParameter = "@userId";

        public void StartUp(string connectionString)
        {
            LogRequest(nameof(StartUp), connectionString);
            
            _connectionString = ConnectionStringParser.ParseConnectionString(connectionString);
        }

        public void CreateUser(UserToCreate user)
        {
            LogRequest(nameof(CreateUser), user);
            IsReadyCheck();

            IEnumerable<string> properties =
                GetAllProperties().Select(p => p.Name).Where(s => s != ColumnsNames.Password);

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                //Т.к пароль и данные пользователя хранятся в разных таблицах оборачиваю в транзакцию, что бы при неудачной вставке пароля юзер тоже удалился.
                using (var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var insertCommand = CreateCommand(conn, SqlCommands.InsertUserQuery);

                    foreach (var property in properties)
                    {
                        var propertyValue = user.Properties
                            .FirstOrDefault(p => p.Name == property)?.Value ?? string.Empty;

                        insertCommand.Parameters.Add(new NpgsqlParameter
                            { ParameterName = $"@{property}", Value = propertyValue });
                    }

                    insertCommand.Parameters.Add(new NpgsqlParameter { ParameterName = LoginParameter, Value = user.Login });

                    var passwordCommand = CreateCommand(conn,
                        SqlCommands.InsertPasswordQuery,
                        new NpgsqlParameter(LoginParameter, user.Login),
                        new NpgsqlParameter("@password", user.HashPassword));

                    insertCommand.ExecuteNonQuery();
                    passwordCommand.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
            
            LogResponse(nameof(CreateUser));
        }

        public IEnumerable<Property> GetAllProperties()
        {
            LogRequest(nameof(GetAllProperties));
            var result = new[]
            {
                new Property(ColumnsNames.Password, PropertiesDescriptions.Password),
                new Property(ColumnsNames.FirstName, PropertiesDescriptions.FirstName),
                new Property(ColumnsNames.LastName, PropertiesDescriptions.LastName),
                new Property(ColumnsNames.MiddleName, PropertiesDescriptions.MiddleName),
                new Property(ColumnsNames.TelephoneNumber, PropertiesDescriptions.TelephoneNumber),
                new Property(ColumnsNames.IsLead, PropertiesDescriptions.IsLead),
            };
            
            LogResponse(nameof(GetAllProperties), result);
            
            return result;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            LogRequest(nameof(GetUserProperties), userLogin);
            IsReadyCheck();

            var userProperties = new List<UserProperty>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var userPropertiesReader = CreateCommand(conn,
                        SqlCommands.GetUserPropertiesQuery, new NpgsqlParameter(LoginParameter, userLogin))
                    .ExecuteReader();

                userPropertiesReader.Read();
                userProperties.AddRange(ColumnsNames.UserProperties.Where(p => p != ColumnsNames.Password)
                    .Select(property =>
                        new UserProperty(property, userPropertiesReader[property].ToString() ?? string.Empty)));

            }
            
            LogResponse(nameof(GetUserProperties), userProperties);
            
            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            LogRequest(nameof(IsUserExists), userLogin);
            IsReadyCheck();

            Boolean response = false;
            
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var selectCommand = CreateCommand(conn, SqlCommands.IsUserExistQuery,
                    new NpgsqlParameter(LoginParameter, userLogin));
                response = selectCommand.ExecuteScalar() != null;
            }
            
            LogResponse(nameof(IsUserExists), response);
            
            return response;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            LogRequest(nameof(UpdateUserProperties), properties, userLogin);
            IsReadyCheck();

            var availableForUpdateProperties = ColumnsNames.AbleToUpdateUserProperties;
            var propertiesToUpdate = properties.Where(p => availableForUpdateProperties.Contains(p.Name));

            if (!propertiesToUpdate.Any()) throw new ConnectorException(ExceptionMessages.IncorrectProperties);

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var parametrs = new List<NpgsqlParameter>();

                var sb = new StringBuilder();

                foreach (var property in propertiesToUpdate)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(',');
                        sb.Append($"\"{property.Name}\" = @{property.Name}");
                    }
                    else
                    {
                        sb.Append($"\"{property.Name}\" = @{property.Name}");
                    }

                    parametrs.Add(new NpgsqlParameter($"@{property.Name}", property.Value));
                }

                parametrs.Add(new NpgsqlParameter(LoginParameter, userLogin));

                var updateCommand = CreateCommand(conn, string.Format(SqlCommands.UpdateUserQuery, sb));

                foreach (var parametr in parametrs)
                {
                    updateCommand.Parameters.Add(parametr);
                }

                updateCommand.ExecuteNonQuery();
            }
            
            LogResponse(nameof(UpdateUserProperties));
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            LogRequest(nameof(GetAllPermissions));
            IsReadyCheck();

            var result = new List<Permission>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var rolesReader = CreateCommand(conn, SqlCommands.GetAllRolesQuery)
                    .ExecuteReader();

                while (rolesReader.Read())
                {
                    var id = rolesReader.GetInt32(0);
                    var name = rolesReader.GetString(1);

                    result.Add(new Permission(id.ToString(), name, ColumnsNames.ItRoleRightGroupName));
                }

                rolesReader.Close();

                var requestRightReader = CreateCommand(conn, SqlCommands.GetAllRequestsRightsQuery)
                    .ExecuteReader();

                while (requestRightReader.Read())
                {
                    var id = rolesReader.GetInt32(0);
                    var name = rolesReader.GetString(1);

                    result.Add(new Permission(id.ToString(), name, ColumnsNames.RequestRightGroupName));
                }

                rolesReader.Close();
            }
            
            LogResponse(nameof(GetAllPermissions), result);
                
            return result;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            LogRequest(nameof(AddUserPermissions), userLogin, rightIds);
            IsReadyCheck();

            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();

                //Не уверен что нужно было проверять, обычно в базах проставлены FK.
                //Проверку на наличие ролей/прав на запросы в базе дополнительно реализовывать не стал, т.к. в задании про это ничего не сказано.
                if (!IsUserExists(userLogin))
                    throw new ConnectorException(string.Format(ExceptionMessages.UserNotFound, userLogin));

                var rolesKeyValues = new List<string[]>();
                var requestsKeyValues = new List<string[]>();

                foreach (var rightId in rightIds)
                {
                    var keyValue = rightId.Split(':');
                    if (keyValue[0] == ColumnsNames.ItRoleRightGroupName)
                    {
                        rolesKeyValues.Add(keyValue);
                    }

                    if (keyValue[0] == ColumnsNames.RequestRightGroupName)
                    {
                        requestsKeyValues.Add(keyValue);
                    }
                }

                if (rolesKeyValues.Any())
                    ParseUpdatePermissionInputToCommand(rolesKeyValues, con, userLogin)
                        .ExecuteNonQuery();


                if (requestsKeyValues.Any())
                    ParseUpdatePermissionInputToCommand(requestsKeyValues, con, userLogin)
                        .ExecuteNonQuery();
            }
            
            LogResponse(nameof(AddUserPermissions));
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            LogRequest(nameof(RemoveUserPermissions), userLogin, rightIds);
            IsReadyCheck();
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                var rolesIdsToDelete = new List<int>();
                var requestsIdsToDelete = new List<int>();

                foreach (var rightId in rightIds)
                {
                    var keyValue = rightId.Split(':');
                    if (keyValue[0] == ColumnsNames.ItRoleRightGroupName)
                    {
                        rolesIdsToDelete.Add(int.Parse(keyValue[1]));
                    }

                    if (keyValue[0] == ColumnsNames.RequestRightGroupName)
                    {
                        requestsIdsToDelete.Add(int.Parse(keyValue[1]));
                    }
                }

                if (rolesIdsToDelete.Any())
                {
                    var removeRoles = new NpgsqlParameter("@ids", NpgsqlDbType.Integer | NpgsqlDbType.Array)
                        { Value = rolesIdsToDelete };
                    CreateCommand(con, SqlCommands.DeleteUserRolesQuery,
                        new NpgsqlParameter(UserIdParameter, userLogin), removeRoles).ExecuteNonQuery();
                }

                if (requestsIdsToDelete.Any())
                {
                    var removeRequestsRights = new NpgsqlParameter("@ids", NpgsqlDbType.Integer | NpgsqlDbType.Array)
                        { Value = requestsIdsToDelete };
                    CreateCommand(con, SqlCommands.DeleteUserRequestRightsQuery,
                        new NpgsqlParameter(UserIdParameter, userLogin), removeRequestsRights).ExecuteNonQuery();
                }
            }
            
            LogResponse(nameof(RemoveUserPermissions));
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            LogRequest(nameof(GetUserPermissions), userLogin);
            IsReadyCheck();
            
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                var result = new List<string>();
                conn.Open();

                var requestRightsReader =
                    CreateCommand(conn, SqlCommands.GetUserRequestRightsQuery,
                            new NpgsqlParameter(UserIdParameter, userLogin))
                        .ExecuteReader();
                while (requestRightsReader.Read())
                {
                    result.Add(requestRightsReader.GetString(0));
                }

                requestRightsReader.Close();

                var rolesReader =
                    CreateCommand(conn, SqlCommands.GetUserRolesQuery, new NpgsqlParameter(UserIdParameter, userLogin))
                        .ExecuteReader();
                while (rolesReader.Read())
                {
                    result.Add(rolesReader.GetString(0));
                }
                
                LogResponse(nameof(GetUserPermissions),result);

                return result;
            }
        }

        public ILogger? Logger { get; set; }

        private IDbCommand CreateCommand(IDbConnection dbConnection, string commandText,
            params DbParameter[] parameters)
        {
            var command = dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            return command;
        }

        private void IsReadyCheck()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new ConnectorException(
                    ExceptionMessages.ConnectorIsNotReady);
        }

        private IDbCommand ParseUpdatePermissionInputToCommand(IEnumerable<string[]> keyValuePermissionInput,
            IDbConnection dbConnection, string userLogin)
        {
            var parameters = new List<NpgsqlParameter>();
            var sb = new StringBuilder();
            foreach (var keyValue in keyValuePermissionInput)
            {
                if (sb.Length > 0) sb.Append(',');
                var parameterName = $"@{string.Join('I', keyValue)}";
                parameters.Add(new NpgsqlParameter(parameterName, int.Parse(keyValue[1])));
                sb.Append($"({UserIdParameter}, {parameterName})");
            }

            parameters.Add(new NpgsqlParameter(UserIdParameter, userLogin));

            return CreateCommand(dbConnection, string.Format(SqlCommands.AddUserRoles, sb), parameters.ToArray());
        }

        private void LogResponse(string methodName, params object[] parameters)
        {
            Logger?.Debug(string.Format(LogMessages.ResponseLog, methodName, string.Join(';', parameters)));
        }
        
        private void LogRequest(string methodName, params object[] parameters)
        {
            Logger?.Debug(string.Format(LogMessages.RequestLog, methodName, parameters.Length == 0 ? "Void" : string.Join(';', parameters)));
        }
    }
}