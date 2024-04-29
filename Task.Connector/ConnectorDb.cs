using Microsoft.Data.SqlClient;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    // При запуске тестов нужно будет изменить строку подключения для MSSQL на свою
    public class ConnectorDb : IConnector
    {
        public ConnectorDb() { }

        private string _connectionString;
        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateUser(UserToCreate user)
        {
            // Создание нового пользователя 
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = $"INSERT INTO [TestTaskSchema].[User]" +
                    $"(Login, " +
                    $"LastName, " +
                    $"FirstName, " +
                    $"MiddleName, " +
                    $"TelephoneNumber, " +
                    $"IsLead) " +
                    "VALUES " +
                    "(@Login, " +
                    "@LastName, " +
                    "@FirstName, " +
                    "@MiddleName, " +
                    "@TelephoneNumber, " +
                    "@IsLead)";

                using (var command = new SqlCommand(query, connection))
                {
                    var lastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value;
                    var firstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value;
                    var middleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value;
                    var telephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value;
                    var isLead = user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value;

                    command.Parameters.AddWithValue("@Login", user.Login);
                    command.Parameters.AddWithValue("@LastName", lastName ?? "user");
                    command.Parameters.AddWithValue("@FirstName", firstName ?? "firstName");
                    command.Parameters.AddWithValue("@MiddleName", middleName ?? "middleName");
                    command.Parameters.AddWithValue("@TelephoneNumber", telephoneNumber?? "8000000000");
                    command.Parameters.AddWithValue("@IsLead", isLead ?? "0");

                    int result = command.ExecuteNonQuery();
                }

                Logger.Debug($"User {user.Login} created");
            }
        }

        public bool IsUserExists(string userLogin)
        {
            // Проверка существования пользователя
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT COUNT(*) " +
                    "FROM [TestTaskSchema].[User] " +
                    "WHERE Login = @Login";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", userLogin);

                    var count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            // Получение всех свойств
            var properties = new List<Property>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT Name " +
                    "FROM sys.columns " +
                    "WHERE object_id = OBJECT_ID('[TestTaskSchema].[User]')";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var propertyName = reader.GetString(0);
                            properties.Add(new Property(propertyName, propertyName));
                        }
                    }
                }
            }

            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            // Получение свойств пользователя в системе
            var userProperties = new List<UserProperty>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT " +
                    "LastName, " +
                    "FirstName, " +
                    "MiddleName, " +
                    "TelephoneNumber, " +
                    "IsLead " +
                    "FROM [TestTaskSchema].[User] WHERE Login = @Login";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", userLogin);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userProperties.Add(new UserProperty("LastName", reader.GetString(0)));
                            userProperties.Add(new UserProperty("FirstName", reader.GetString(1)));
                            userProperties.Add(new UserProperty("MiddleName", reader.GetString(2)));
                            userProperties.Add(new UserProperty("TelephoneNumber", reader.GetString(3)));
                            userProperties.Add(new UserProperty("IsLead", reader.GetBoolean(4).ToString()));
                        }
                    }
                }
            }

            return userProperties;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            // Обновление свойств пользователя в системе
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "UPDATE [TestTaskSchema].[User] SET " +
                            $"{string.Join(", ",
                                properties.Select(p => $"{p.Name} = @{p.Name} "))} " +
                            "WHERE Login = @Login";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", userLogin);

                    foreach (var property in properties)
                    {
                        command.Parameters.AddWithValue($"@{property.Name}", property.Value);
                    }

                    command.ExecuteNonQuery();
                }

                Logger.Debug($"User {userLogin} properties updated successfully.");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            // Получение всех прав пользователя в системе
            var permissions = new List<Permission>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT Id, Name " +
                    "FROM [TestTaskSchema].[RequestRight] " +
                    "UNION " +
                    "SELECT Id, Name " +
                    "FROM [TestTaskSchema].[ItRole]";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var permissionId = reader.GetInt32(0);
                            var permissionName = reader.GetString(1);
                            permissions.Add(new Permission(permissionId.ToString(), permissionName, permissionName));
                        }
                    }
                }
            }

            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                foreach (var rightId in rightIds)
                {
                    if (rightId.StartsWith("Request"))
                    {
                        string query = "INSERT INTO [TestTaskSchema].[UserRequestRight] " +
                            "(userId, rightId) " +
                            "VALUES (@userId, @rightId);";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@userId", userLogin);

                            var requestRightId = rightId.Replace("Request:", "");
                            command.Parameters.AddWithValue("@rightId", rightId.Replace("Request:", ""));
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (rightId.StartsWith("Role"))
                    {
                        string query = "INSERT INTO [TestTaskSchema].[UserITRole] " +
                            "(userId, roleId) " +
                            "VALUES (@userId, @roleId);";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@userId", userLogin);

                            var itRoleId = rightId.Replace("Role:", "");
                            command.Parameters.AddWithValue("@roleId", rightId.Replace("Role:", ""));
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        Logger.Error($"Invalid right ID format: {rightId}");
                    }

                }

                Logger.Debug($"Permissions added to user {userLogin} successfully.");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                foreach (var rightId in rightIds)
                {
                    if (rightId.StartsWith("Request"))
                    {
                        string query = "DELETE FROM " +
                            "[TestTaskSchema].[UserRequestRight] " +
                            "WHERE userId = @userId AND rightId = rightId;";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@userId", userLogin);

                            var requestRightId = rightId.Replace("Request:", "");
                            command.Parameters.AddWithValue("@rightId", rightId.Replace("Request:", ""));
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (rightId.StartsWith("Role"))
                    {
                        string query = "DELETE FROM " +
                            "[TestTaskSchema].[UserItRole] " +
                            "WHERE userId = @userId AND roleId = @roleId;";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@userId", userLogin);

                            var itRoleId = rightId.Replace("Role:", "");
                            command.Parameters.AddWithValue("@roleId", rightId.Replace("Role:", ""));
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        Logger.Error($"Invalid right ID format: {rightId}");
                    }

                }

                Logger.Debug($"Permissions removed from user {userLogin} successfully.");
            }

        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var permissions = new List<string>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT 'Request' + CAST(rightId AS VARCHAR(10)) " +
                    "FROM [TestTaskSchema].[UserRequestRight] " +
                    "WHERE userId = @userId " +
                    "UNION " +
                    "SELECT 'Role' + CAST(roleId AS VARCHAR(10)) " +
                    "FROM [TestTaskSchema].[UserItRole] " +
                    "WHERE userId = @userId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userLogin);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var permissionId = reader.GetString(0);
                            permissions.Add(permissionId);
                        }
                    }
                }
            }

            return permissions;
        }

    }
}