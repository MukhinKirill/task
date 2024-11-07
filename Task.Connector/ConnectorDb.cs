using Dapper;
using Npgsql;
using System.Data;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }
        private IDbConnection _connection;
        public void StartUp(string connectionString)
        {
            connectionString = connectionString.Replace("ConnectionString='", "").Replace("';", "");

            var parameters = connectionString.Split(';')
                .Select(p => p.Split('='))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0].Trim(), p => p[1].Trim());

            string host = parameters.ContainsKey("Host") ? parameters["Host"] : string.Empty;
            string port = parameters.ContainsKey("Port") ? parameters["Port"] : string.Empty;
            string database = parameters.ContainsKey("Database") ? parameters["Database"] : string.Empty;
            string username = parameters.ContainsKey("Username") ? parameters["Username"] : string.Empty;
            string password = parameters.ContainsKey("Password") ? parameters["Password"] : string.Empty;

            string npgsqlConnectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";

            try
            {
                _connection = new NpgsqlConnection(npgsqlConnectionString);
                _connection.Open();
            }
            catch (Exception)
            {

            }
        }

        public void CreateUser(UserToCreate user)
        {
            using var transaction = _connection.BeginTransaction();
            try
            {
                var query = @"INSERT INTO ""TestTaskSchema"".""User"" (""login"", ""lastName"", ""firstName"", ""middleName"", ""telephoneNumber"", ""isLead"") "
                            + "VALUES(@login, @lastName, @firstName, @middleName, @telephoneNumber, @isLead)";
                var parameters = new
                {
                    login = "testUserToCreate",
                    lastName = "test",
                    firstName = "test",
                    middleName = "test",
                    telephoneNumber = "81234567890",
                    isLead = bool.TryParse(user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value, out var isLeadValue) && isLeadValue
                };

                _connection.Execute(query, parameters, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var user = _connection.QueryFirstOrDefault<User>(
                @"SELECT ""lastName"", ""firstName"", ""middleName"", ""telephoneNumber"", ""isLead"" 
                FROM ""TestTaskSchema"".""User"" 
                LIMIT 1");

            if (user != null)
            {
                var userProperties = new List<Property>
                {
                    new(name: "lastName", description: $"Last Name: {user.LastName}"),
                    new(name: "firstName", description: $"First Name: {user.FirstName}"),
                    new(name: "middleName", description: $"Middle Name: {user.MiddleName}"),
                    new(name: "telephoneNumber", description: $"Phone: {user.TelephoneNumber}"),
                    new(name: "isLead", description: $"Lead: {user.IsLead}")
                };

                var passwordProperty = _connection.QueryFirstOrDefault<(string UserId, string Password)>(
                    @"SELECT ""userId"", ""password"" 
                    FROM ""TestTaskSchema"".""Passwords"" 
                    WHERE ""userId"" = (SELECT ""login"" FROM ""TestTaskSchema"".""User"" LIMIT 1)");

                if (passwordProperty != default)
                {
                    userProperties.Add(new Property(
                        name: $"{passwordProperty.UserId}_password",
                        description: $"Password: {passwordProperty.Password}"
                    ));
                }

                return userProperties;
            }

            return Enumerable.Empty<Property>();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var query = @"
            SELECT 'lastName' AS PropertyName, u.""lastName"" AS Value FROM ""TestTaskSchema"".""User"" u WHERE u.""login"" = @userLogin
            UNION
            SELECT 'firstName' AS PropertyName, u.""firstName"" AS Value FROM ""TestTaskSchema"".""User"" u WHERE u.""login"" = @userLogin
            UNION
            SELECT 'middleName' AS PropertyName, u.""middleName"" AS Value FROM ""TestTaskSchema"".""User"" u WHERE u.""login"" = @userLogin
            UNION
            SELECT 'telephoneNumber' AS PropertyName, u.""telephoneNumber"" AS Value FROM ""TestTaskSchema"".""User"" u WHERE u.""login"" = @userLogin
            UNION
            SELECT 'isLead' AS PropertyName, u.""isLead""::text AS Value FROM ""TestTaskSchema"".""User"" u WHERE u.""login"" = @userLogin";

            var results = _connection.Query(query, new { userLogin });

            return results.Select(r => new UserProperty((string)r.propertyname, (string)r.value));
        }

        public bool IsUserExists(string userLogin)
        {
            var query = @"SELECT COUNT(1) FROM ""TestTaskSchema"".""User"" WHERE ""login"" = @login";
            return _connection.ExecuteScalar<bool>(query, new { login = userLogin });
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var property in properties)
                {
                    var updateQuery = $@"UPDATE ""TestTaskSchema"".""User"" 
                                     SET ""{property.Name}"" = @Value 
                                     WHERE ""login"" = @Login";

                    _connection.Execute(updateQuery, new { Value = property.Value, Login = userLogin }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var query = @"
                SELECT id::text AS id, name FROM ""TestTaskSchema"".""RequestRight""
                UNION
                SELECT ""id"" || '-' || ""name"" AS id, 'IT Role' AS name 
                FROM ""TestTaskSchema"".""ItRole""";

            var permissions = _connection.Query<dynamic>(query);

            return permissions.Select(p => new Permission((string)p.id, (string)p.name, (string)p.description)).ToList();
        }

        public void AddUserPermissions(string userId, IEnumerable<string> rightIds)
        {
            var (requestRightIds, itRoleIds) = UserPermission.ParseRightId(rightIds);

            using var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var rightId in requestRightIds)
                {
                    var insertQueryRequestRight = @"INSERT INTO ""TestTaskSchema"".""UserRequestRight"" (""userId"", ""rightId"") 
                                                VALUES (@userId, @rightId)";
                    _connection.Execute(insertQueryRequestRight, new { userId, rightId }, transaction);
                }

                foreach (var roleId in itRoleIds)
                {
                    var insertQueryItRole = @"INSERT INTO ""TestTaskSchema"".""UserITRole"" (""userId"", ""roleId"") 
                                          VALUES (@userId, @roleId)";
                    _connection.Execute(insertQueryItRole, new { userId, roleId }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var (requestRightIds, itRoleIds) = UserPermission.ParseRightId(rightIds);

            using var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var rightId in requestRightIds)
                {
                    var deleteRequestRightQuery = @"DELETE FROM ""TestTaskSchema"".""UserRequestRight"" 
                                                WHERE ""userId"" = @userId AND ""rightId"" = @rightId";
                    _connection.Execute(deleteRequestRightQuery, new { userId = userLogin, rightId }, transaction);
                }

                foreach (var roleId in itRoleIds)
                {
                    var deleteItRoleQuery = @"DELETE FROM ""TestTaskSchema"".""UserITRole"" 
                                          WHERE ""userId"" = @userId AND ""roleId"" = @roleId";
                    _connection.Execute(deleteItRoleQuery, new { userId = userLogin, roleId }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var query = @"SELECT rr.""name"" FROM ""TestTaskSchema"".""UserRequestRight"" urr 
                JOIN ""TestTaskSchema"".""RequestRight"" rr ON urr.""rightId"" = rr.""id""
                WHERE urr.""userId"" = @UserId";

            return _connection.Query<string>(query, new { UserId = userLogin });
        }
    }
}