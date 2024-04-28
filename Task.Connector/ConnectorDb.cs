using Microsoft.Data.SqlClient;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private string _connectionString;
        public ILogger Logger { get; set; }

        public ConnectorDb () { }


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

                var query = "INSERT INTO [User] (Login, LastName, FirstName, MiddleName, TelephoneNumber, IsLead) " +
                            "VALUES (@Login, @LastName, @FirstName, @MiddleName, @TelephoneNumber, @IsLead)";

                using (var command = new SqlCommand(query, connection))
                {
                    foreach (var property in user.Properties)
                    {
                        command.Parameters.AddWithValue($"@{property.Name}", property.Value);
                    }

                    command.ExecuteNonQuery();
                }

                Logger.Debug($"User {user.Login} created");
            }
        }

        public bool IsUserExists(string userLogin)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM [User] WHERE Login = @Login";

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
            throw new NotImplementedException();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }


        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
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