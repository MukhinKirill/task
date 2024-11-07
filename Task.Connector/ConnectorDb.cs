using Dapper;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using Task.Connector.Models;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        static string requestRightGroupName = "Request";
        static string itRoleRightGroupName = "Role";
        static string delimeter = ":";

        private string _connectionString;

        public void StartUp(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Создать пользователя с набором свойств по умолчанию
        /// </summary>
        public void CreateUser(UserToCreate user)
        {
            var parameters = new List<string> { "lastName", "firstName", "middleName", "telephoneNumber", "isLead" };

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var sql = @"INSERT INTO ""TestTaskSchema"".""User"" (login, ""lastName"", ""firstName"", ""middleName"", ""telephoneNumber"", ""isLead"") 
                        VALUES (@login, @lastName, @firstName, @middleName, @telephoneNumber, @isLead);";

            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@login", user.Login);
            foreach (var parameter in parameters)
            {
                var value = user.Properties.FirstOrDefault(prop => prop.Name == parameter)?.Value ?? string.Empty;
                if (parameter == "isLead")
                    command.Parameters.Add(new NpgsqlParameter
                    {
                        ParameterName = parameter,
                        NpgsqlDbType = NpgsqlDbType.Boolean,
                        Value = value != null ? bool.Parse(value) : default,
                    });
                else
                    command.Parameters.Add(new NpgsqlParameter(parameter, value));
            }

            var setPasswordCommandText =
                @"INSERT INTO ""TestTaskSchema"".""Passwords"" 
                (""userId"", ""password"")
                VALUES (@login, @password);";

            using var passwordCommand = new NpgsqlCommand(setPasswordCommandText, connection);
            passwordCommand.Parameters.AddWithValue("@login", user.Login);
            passwordCommand.Parameters.AddWithValue("@password", user.Properties.FirstOrDefault(p => p.Name == "password")?.Value ?? DefaultData.MasterUserDefaultPassword);

            using var transaction = connection.BeginTransaction();

            command.ExecuteNonQuery();
            passwordCommand.ExecuteNonQuery();

            transaction.Commit();
        }

        /// <summary>
        /// Получить все свойства
        /// </summary>
        public IEnumerable<Property> GetAllProperties()
        {
            var prepertiesInfo = new Dictionary<string, string>();

            var ignoredProps = new List<string>
            {
                "id", "userId", "login"
            };

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var getPropertyUserTableSql =
                $"SELECT column_name, data_type " +
                $"FROM information_schema.columns " +
                $"WHERE table_name = 'User';";

            GetProps(ref prepertiesInfo, ignoredProps, getPropertyUserTableSql, connection);

            var getPropertyPasswordsTableSql =
                $"SELECT column_name, data_type " +
                $"FROM information_schema.columns " +
                $"WHERE table_name = 'Passwords';";
            GetProps(ref prepertiesInfo, ignoredProps, getPropertyPasswordsTableSql, connection);

            return prepertiesInfo.Select(info => new Property(info.Key, string.Empty));
        }

        private void GetProps(ref Dictionary<string, string> prepertiesInfo, List<string> ignoredProps, string getPropertyUserTableSql, NpgsqlConnection connection)
        {
            using var reader = connection.ExecuteReader(getPropertyUserTableSql);
            while (reader.Read())
            {
                if (!ignoredProps.Contains(reader["column_name"].ToString()))
                    prepertiesInfo.Add(reader["column_name"].ToString(), reader["data_type"].ToString());
            }
        }

        /// <summary>
        /// Получить свойства пользователя
        /// </summary>
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var sql = @"SELECT login, ""lastName"", ""firstName"", ""middleName"", ""telephoneNumber"", ""isLead"",""password"" 
                FROM ""TestTaskSchema"".""User""   
                JOIN ""TestTaskSchema"".""Passwords"" ON ""TestTaskSchema"".""User"".login = ""TestTaskSchema"".""Passwords"".""userId""
                WHERE login = @login";
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var reader = connection.ExecuteReader(sql, new { login = userLogin });

            var allProperties = GetAllProperties();
            List<UserProperty> properties = new();
            while (reader.Read())
            {
                foreach (var prop in allProperties)
                    properties.Add(new UserProperty(prop.Name, reader[prop.Name].ToString()));
            }
            return properties;
        }

        /// <summary>
        /// Проверить то пользователь существует
        /// </summary>
        public bool IsUserExists(string userLogin)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT EXISTS(SELECT 1 FROM ""TestTaskSchema"".""User"" WHERE login = @login)";
            var exists = connection.ExecuteScalar<bool>(sql, new { login = userLogin });
            return exists;
        }

        /// <summary>
        /// Обновить свойства пользователя
        /// </summary>
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Dictionary<string, string> userPropertyDictionary = new();

            foreach (var userProperty in properties)
            {
                userPropertyDictionary.Add(userProperty.Name, userProperty.Value);
            }

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"UPDATE ""TestTaskSchema"".""User"" SET ");
            foreach (KeyValuePair<string, string> entry in userPropertyDictionary)
            {
                sqlBuilder.Append(@"""{entry.Key}"" = @{entry.Key},");
            }
            sqlBuilder.Length -= 1;
            sqlBuilder.Append(@" WHERE ""login"" = @login");

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using var command = new NpgsqlCommand(sqlBuilder.ToString(), connection);
                command.Parameters.AddWithValue("@login", userLogin);
                foreach (var entry in userPropertyDictionary)
                {
                    command.Parameters.AddWithValue($"@{entry.Key}", entry.Value);
                }
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Получить все доступные права
        /// </summary>
        public IEnumerable<Permission> GetAllPermissions()
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var getRightsSql = @"SELECT ""id"",""name"" FROM  ""TestTaskSchema"".""RequestRight""";
            var getItRightsSql = @"SELECT ""id"",""name"" FROM ""TestTaskSchema"".""ItRole"";";
            var rights = connection.Query<PermisionModel>(getRightsSql);
            var itRights = connection.Query<PermisionModel>(getItRightsSql);

            var commonRights = rights.Union(itRights);
            return commonRights.Select(right => new Permission(right.Id, right.Name, string.Empty));
        }

        /// <summary>
        /// Добавить права пользователю
        /// </summary>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var itRolesIdsToAdd = new List<int>();
            var requestRightsIdsToAdd = new List<int>();

            foreach (var rightId in rightIds)
            {
                if (rightId.Contains(requestRightGroupName))
                    requestRightsIdsToAdd.Add(int.Parse(rightId.Split(delimeter)[1]));

                if (rightId.Contains(itRoleRightGroupName))
                    itRolesIdsToAdd.Add(int.Parse(rightId.Split(delimeter)[1]));
            }

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            foreach (var roleId in itRolesIdsToAdd)
            {
                var sqlInsertUserItPermission =
                    @"INSERT INTO ""TestTaskSchema"".""UserITRole"" 
                    (""userId"", ""roleId"")  
                    VALUES (@login, @roleId);";
                connection.Execute(sqlInsertUserItPermission, new { login = userLogin, roleId });
            }

            foreach (var rightId in requestRightsIdsToAdd)
            {
                var sqlInserteUserRequestPermission =
                    @"INSERT INTO ""TestTaskSchema"".""UserRequestRight"" 
                    (""userId"", ""rightId"") 
                    VALUES (@login, @rightId);";
                connection.Execute(sqlInserteUserRequestPermission, new { login = userLogin, rightId });
            }
        }

        /// <summary>
        /// Удалить права пользователя
        /// </summary>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var itRolesIdsToDelete = new List<int>();
            var requestRightsIdsToDelete = new List<int>();
            foreach (var rightId in rightIds)
            {
                if (rightId.Contains(requestRightGroupName))
                    requestRightsIdsToDelete.Add(int.Parse(rightId.Split(delimeter)[1]));

                if (rightId.Contains(itRoleRightGroupName))
                    itRolesIdsToDelete.Add(int.Parse(rightId.Split(delimeter)[1]));
            }

            var sqlRemoveUserItPermission =
                @"DELETE FROM ""TestTaskSchema"".""UserITRole""
                WHERE ""userId"" =@login AND ""roleId""= ANY(@roles)";
            connection.Execute(sqlRemoveUserItPermission, new { login = userLogin, roles = itRolesIdsToDelete.ToArray() });

            var sqlRemoveUserRequestPermission =
                @"DELETE FROM ""TestTaskSchema"".""UserRequestRight""  
                WHERE ""userId"" =@login AND ""rightId""= ANY(@rightIds)";
            connection.Execute(sqlRemoveUserRequestPermission, new { login = userLogin, rightIds = requestRightsIdsToDelete.ToArray() });
        }

        /// <summary>
        /// Получить права пользователя
        /// </summary>
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql =
                @"SELECT ""TestTaskSchema"".""RequestRight"".""name"" 
                FROM ""TestTaskSchema"".""UserRequestRight"" 
                JOIN ""TestTaskSchema"".""RequestRight"" ON ""TestTaskSchema"".""UserRequestRight"".""rightId"" = ""TestTaskSchema"".""RequestRight"".""id"" 
                WHERE ""userId"" = @userId";
            var parameters = new { userId = userLogin };

            var rights = connection.Query<string>(sql, parameters);
            return rights;
        }

        public ILogger Logger { get; set; }
    }
}