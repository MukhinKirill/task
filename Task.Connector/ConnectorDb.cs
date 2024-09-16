using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private readonly IDbConnection _connection;
        private readonly ILogger _logger;
        private string _connectionString;
        public ILogger Logger { get; set; }
        public Connector(IDbConnection connection, ILogger logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public Connector()
        {

        }
        public void StartUp(string connectionString)
        {
            _connectionString = connectionString;
            Logger?.LogInformation($"Connection string set to: {connectionString}");
        }
        public void CreateUser(UserToCreate user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO User (Login, LastName, FirstName, MiddleName, TelephoneNumber, IsLead) VALUES (@Login, @LastName, @FirstName, @MiddleName, @TelephoneNumber, @IsLead)";

                command.Parameters.AddWithValue("@Login", user.Login);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@MiddleName", user.MiddleName);
                command.Parameters.AddWithValue("@TelephoneNumber", user.TelephoneNumber);
                command.Parameters.AddWithValue("@IsLead", user.IsLead);

                command.ExecuteNonQuery();
            }
            Logger?.LogInformation("User created successfully.");
        }
        public bool IsUserExists(string userLogin)
        {
            string query = "SELECT COUNT(1) FROM Users WHERE Login = @login";
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.Add(new SqlParameter("@login", userLogin));
                return (int)command.ExecuteScalar() > 0;
            }
        }
        public IEnumerable<Property> GetAllProperties()
        {
            string query = "SELECT * FROM Properties";
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new Property
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };
                    }
                }
            }
        }
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            string query = "SELECT * FROM UserProperties WHERE UserLogin = @login";
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.Add(new SqlParameter("@login", userLogin));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new UserProperty
                        {
                            UserLogin = reader.GetString(0),
                            PropertyId = reader.GetInt32(1),
                            Value = reader.GetString(2)
                        };
                    }
                }
            }
        }
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            foreach (var property in properties)
            {
                string query = "UPDATE UserProperties SET Value = @value WHERE UserLogin = @login AND PropertyId = @propertyId";
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@value", property.Value));
                    command.Parameters.Add(new SqlParameter("@login", userLogin));
                    command.Parameters.Add(new SqlParameter("@propertyId", property.PropertyId));
                    command.ExecuteNonQuery();
                }
            }

            _logger.LogInformation("User properties updated for: {UserLogin}", userLogin);
        }
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            foreach (var rightId in rightIds)
            {
                string query = "INSERT INTO UserRequestRight (UserLogin, RightId) VALUES (@login, @rightId)";
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@login", userLogin));
                    command.Parameters.Add(new SqlParameter("@rightId", rightId));
                    command.ExecuteNonQuery();
                }
            }
        }
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            foreach (var rightId in rightIds)
            {
                string query = "DELETE FROM UserRequestRight WHERE UserLogin = @login AND RightId = @rightId";
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@login", userLogin));
                    command.Parameters.Add(new SqlParameter("@rightId", rightId));
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}