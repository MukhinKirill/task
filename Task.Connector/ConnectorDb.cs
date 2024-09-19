using Microsoft.Data.SqlClient;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private string _connectionString;

        public void StartUp(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateUser(UserToCreate user)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                Console.WriteLine("Connection established");
                using (var transaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        var addUserQuery = new SqlCommand(
                            "INSERT INTO [TestTaskSchema].[User] (login, lastName, firstName, middleName, telephoneNumber, isLead) " +
                            "VALUES (@login, @lastName, @firstName, @middleName, @telephoneNumber, @isLead)",
                            sqlConnection, transaction);

                        addUserQuery.Parameters.AddWithValue("@login", user.Login);
                        addUserQuery.Parameters.AddWithValue("@lastName",
                            user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "empty");
                        addUserQuery.Parameters.AddWithValue("@firstName",
                            user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "empty");
                        addUserQuery.Parameters.AddWithValue("@middleName",
                            user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "empty");
                        addUserQuery.Parameters.AddWithValue("@telephoneNumber",
                            user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ?? "empty");
                        addUserQuery.Parameters.AddWithValue("@isLead",
                            user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value);
                        addUserQuery.ExecuteNonQuery();

                        var addUserPasswordQuery = new SqlCommand(
                            "INSERT INTO [TestTaskSchema].[Passwords] (userId, password) " +
                            "VALUES (@userId, @password)", sqlConnection, transaction);

                        addUserPasswordQuery.Parameters.AddWithValue("@userId", user.Login);
                        addUserPasswordQuery.Parameters.AddWithValue("@password", user.HashPassword);
                        addUserPasswordQuery.ExecuteNonQuery();

                        transaction.Commit();
                        Console.WriteLine("New user added."); //Изменить
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("An error occurred while adding a user: " + ex.Message);
                    }
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var propertiesNames = new List<Property>();

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                Console.WriteLine("Connection established.");

                try
                {
                    var selectPropertiesQuery = new SqlCommand("SELECT COLUMN_NAME " +
                                                               "FROM INFORMATION_SCHEMA.COLUMNS " +
                                                               "WHERE TABLE_NAME = N'User' " +
                                                               "AND COLUMN_NAME NOT IN (" +
                                                               "    SELECT COLUMN_NAME  " +
                                                               "    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                                                               "    WHERE TABLE_NAME = N'User' " +
                                                               ") " +
                                                               "UNION ALL " +
                                                               "SELECT 'password' AS COLUMN_NAME;", sqlConnection);

                    using (var reader = selectPropertiesQuery.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            propertiesNames.Add(new Property(reader.GetString(0), "empty"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while selecting data: " + ex.Message);
                }
            }

            return propertiesNames;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }

        public bool IsUserExists(string userLogin)
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

        public ILogger Logger { get; set; }
    }
}