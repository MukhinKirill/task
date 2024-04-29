using Microsoft.Data.SqlClient;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private string _connectionString;
        public ILogger Logger { get; set; }

        public ConnectorDb() { }


        public void StartUp(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateUser(UserToCreate user)
        {
            // Создание нового пользователя в базе данных с набором свойств по умолчанию
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = $"INSERT INTO [TestTaskSchema].[User] (Login, {string.Join(", ", user.Properties.Select(p => p.Name))}) " +
                $"VALUES (@Login, {string.Join(", ", user.Properties.Select(p => '@' + p.Name))})";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", user.Login);
                    foreach (var property in user.Properties)
                    {
                        command.Parameters.AddWithValue($"@{property.Name}", property.Value);
                    }

                    int result = command.ExecuteNonQuery();
                }

                Logger.Debug($"User {user.Login} created");
            }
        }

        public bool IsUserExists(string userLogin)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [TestTaskSchema].[User] WHERE Login = @Login";

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
            throw new NotImplementedException();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }

    }
}