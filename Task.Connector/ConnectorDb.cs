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
                            "INSERT INTO [TestTaskSchema].[User] " +
                            "(login, lastName, firstName, middleName, telephoneNumber, isLead) " +
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
                        Console.WriteLine("New user added.");
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
                                                               "WHERE TABLE_NAME = 'User' " +
                                                               "AND COLUMN_NAME NOT IN " +
                                                               "(" +
                                                               "    SELECT COLUMN_NAME  " +
                                                               "    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                                                               "    WHERE TABLE_NAME = 'User' " +
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
                    Console.WriteLine("An error occurred while selecting properties: " + ex.Message);
                }
            }

            return propertiesNames;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var userProperties = new List<UserProperty>();

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                Console.WriteLine("Connection established.");

                try
                {
                    var selectUserPropertiesQuery = new SqlCommand(
                        "SELECT lastName, firstName, middleName, telephoneNumber, isLead " +
                        "FROM [TestTaskSchema].[User] " +
                        "WHERE login = @login", sqlConnection);

                    selectUserPropertiesQuery.Parameters.AddWithValue("@login", userLogin);

                    using (var reader = selectUserPropertiesQuery.ExecuteReader())
                    {
                        var columnNames = Enumerable.Range(0, reader.FieldCount)
                            .Select(i => reader.GetName(i))
                            .ToArray();

                        if (reader.Read())
                        {
                            foreach (var columnName in columnNames)
                            {
                                userProperties.Add(new UserProperty(columnName,
                                    reader[columnName].ToString() ?? string.Empty));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while selecting user properties: " + ex.Message);
                }
            }

            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                Console.WriteLine("Connection established.");

                try
                {
                    var checkUserExistsQuery = new SqlCommand("SELECT COUNT(*)" +
                                                              "FROM [TestTaskSchema].[User]" +
                                                              "WHERE login = @login", sqlConnection);

                    checkUserExistsQuery.Parameters.AddWithValue("@login", userLogin);

                    var isUserExists = (int)checkUserExistsQuery.ExecuteScalar();

                    return isUserExists > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while checking user existance: " + ex.Message);
                    return false;
                }
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                Console.WriteLine("Connection established.");

                try
                {
                    var updateUserPropertiesQuery = new SqlCommand("UPDATE [TestTaskSchema].[User] " +
                                                                   "SET lastName = COALESCE(@lastName, lastName), " +
                                                                   "firstName = COALESCE(@firstName, firstName), " +
                                                                   "middleName = COALESCE(@middleName, middleName), " +
                                                                   "telephoneNumber = COALESCE(@telephoneNumber, telephoneNumber), " +
                                                                   "isLead = COALESCE(@isLead, isLead) " +
                                                                   "WHERE login = @login", sqlConnection);

                    var lastName = properties.FirstOrDefault(p => p.Name == "lastName")?.Value;
                    var firstName = properties.FirstOrDefault(p => p.Name == "firstName")?.Value;
                    var middleName = properties.FirstOrDefault(p => p.Name == "middleName")?.Value;
                    var telephoneNumber = properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value;
                    var isLead = properties.FirstOrDefault(p => p.Name == "isLead")?.Value;

                    updateUserPropertiesQuery.Parameters.AddWithValue("@lastName",
                        string.IsNullOrEmpty(lastName) ? DBNull.Value : lastName);
                    updateUserPropertiesQuery.Parameters.AddWithValue("@firstName",
                        string.IsNullOrEmpty(firstName) ? DBNull.Value : firstName);
                    updateUserPropertiesQuery.Parameters.AddWithValue("@middleName",
                        string.IsNullOrEmpty(middleName) ? DBNull.Value : middleName);
                    updateUserPropertiesQuery.Parameters.AddWithValue("@telephoneNumber",
                        string.IsNullOrEmpty(telephoneNumber) ? DBNull.Value : telephoneNumber);
                    updateUserPropertiesQuery.Parameters.AddWithValue("@isLead",
                        string.IsNullOrEmpty(isLead) ? 0 : isLead);
                    updateUserPropertiesQuery.Parameters.AddWithValue("@login", userLogin);

                    updateUserPropertiesQuery.ExecuteNonQuery();
                    Console.WriteLine("User properties updated.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while updating properties: " + ex.Message);
                }
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var permissionsList = new List<Permission>();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                Console.WriteLine("Connection established.");
                try
                {
                    var selectPermissionsQuery = new SqlCommand(
                        "SELECT * " +
                        "FROM [TestTaskSchema].[RequestRight]" +
                        "UNION ALL " +
                        "SELECT id, name " +
                        "FROM [TestTaskSchema].[ItRole] AS name", sqlConnection);

                    using (var reader = selectPermissionsQuery.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissionsList.Add(new Permission(reader.GetInt32(0).ToString(),
                                reader.GetString(1),
                                "empty"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while updating properties: " + ex.Message);
                }
            }

            return permissionsList;
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