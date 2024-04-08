using Dapper;
using Npgsql;
using Task.Connector.Constants;
using Task.Connector.Exceptions;
using Task.Connector.Interfaces;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Connectors
{
    internal class PostgreConnector : IConnectorDb
    {
        public ILogger Logger { get; set; }

        private bool _disposed = false;

        private NpgsqlConnection _dbConnection;

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException(userLogin);

            var rightQuery = "INSERT INTO \"TestTaskSchema\".\"UserRequestRight\" (\"userId\", \"rightId\")" +
                "VALUES (@Login, @RightId)";

            var roleQuery = "INSERT INTO \"TestTaskSchema\".\"UserITRole\" " +
                "VALUES (@Login, @RoleId)";

            var permissions = new UserPremissionsModel(rightIds);

            if (permissions.Requests.Count > 0 || permissions.Roles.Count > 0)
            {
                _dbConnection.Open();

                foreach (var request in permissions.Requests)
                {
                    var command = new NpgsqlCommand(roleQuery, _dbConnection)
                    {
                        CommandText = rightQuery
                    };
                    command.Parameters.AddWithValue("@Login", userLogin);
                    command.Parameters.AddWithValue("@RightId", request);

                    command.ExecuteNonQuery();
                }

                foreach (var role in permissions.Roles)
                {
                    var command = new NpgsqlCommand(roleQuery, _dbConnection)
                    {
                        CommandText = roleQuery
                    };
                    command.Parameters.AddWithValue("@Login", userLogin);
                    command.Parameters.AddWithValue("@RoleId", role);

                    command.ExecuteNonQuery();
                }

                _dbConnection.Close();
            }
        }

        public void CreateUser(UserToCreate user)
        {
            if (IsUserExists(user.Login))
                throw new UserLoginNotUniqueException(user.Login);

            var createUserQuery = "INSERT INTO \"TestTaskSchema\".\"User\"( " +
                "login, \"lastName\", \"firstName\", \"middleName\", \"telephoneNumber\", \"isLead\") " +
                "VALUES (@Login, @LastName, @FirstName, @MiddleName, @TelephoneNumber, @IsLead)";

            var createPasswordQuery = "INSERT INTO \"TestTaskSchema\".\"Passwords\"( " +
                "\"userId\", \"password\") " +
                "VALUES (@Login, @Password);";

            var parameters = new UserModel(user.Login, user.Properties);

            var completed = _dbConnection.Execute(createUserQuery, parameters) != 0;

            if (!completed)
                throw new Exception("Error creating user");

            _dbConnection.Execute(createPasswordQuery, new { parameters.Login, Password = user.HashPassword });
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var query = "SELECT " +
                $"'{RightConstants.REQUEST_RIGHT_GROUP_NAME}' || '{RightConstants.DELIMETER}' || CAST(\"id\" as text) as Id, " +
                "\"name\" as Name " +
                "FROM \"TestTaskSchema\".\"RequestRight\" " +
                "UNION " +
                $"SELECT '{RightConstants.IT_ROLE_RIGHT_GROUP_NAME}' || '{RightConstants.DELIMETER}' || CAST(\"id\" as text) as Id, \"name\" as Name FROM \"TestTaskSchema\".\"ItRole\"";

            return _dbConnection.Query<PermissionModel>(query)
                .Select(x=> new Permission(x.Id, x.Name, string.Empty));
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return typeof(UserObjectPropertyModel).GetProperties()
                .Where(x => x.Name != nameof(UserObjectPropertyModel.Login))
                .Select(x => new Property(x.Name, $"User property - {x.Name}"));
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException(userLogin);

            var query = "SELECT rr.\"name\" FROM \"TestTaskSchema\".\"UserRequestRight\" urr " +
                "INNER JOIN \"TestTaskSchema\".\"RequestRight\" rr ON rr.\"id\" = urr.\"rightId\" " +
                "WHERE urr.\"userId\" = @Login " +
                "UNION " +
                "SELECT ir.\"name\" FROM \"TestTaskSchema\".\"UserITRole\" uir " +
                "INNER JOIN \"TestTaskSchema\".\"ItRole\" ir ON ir.\"id\" = uir.\"roleId\" " + 
                "WHERE uir.\"userId\" = @Login";

            return _dbConnection.Query<string>(query, new { Login = userLogin});
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException(userLogin);

            var user = GetUserObjectProperty(userLogin);

            return user.GetProperties();
        }

        private UserObjectPropertyModel GetUserObjectProperty(string userLogin)
        {
            var query = "SELECT \"login\" as Login, " +
                "\"lastName\" as LastName, " +
                "\"firstName\" as FirstName, " +
                "\"middleName\" as MiddleName, " +
                "\"telephoneNumber\" as TelephoneNumber, " +
                "\"isLead\" as IsLead, " +
                "\"password\" as \"Password\" " +
                "FROM \"TestTaskSchema\".\"User\" u " +
                "INNER JOIN \"TestTaskSchema\".\"Passwords\" p ON p.\"userId\" = u.\"login\" " +
                "where u.login = @Login";

            return _dbConnection.QueryFirst<UserObjectPropertyModel>(query, new { Login = userLogin });
        }

        public bool IsUserExists(string userLogin)
        {
            var query = "SELECT COUNT(login) FROM \"TestTaskSchema\".\"User\" " +
                "WHERE login = @Login";

            return _dbConnection.ExecuteScalar<int>(query, new { Login = userLogin }) > 0;
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException(userLogin);

            var rightQuery = "DELETE FROM \"TestTaskSchema\".\"UserRequestRight\" " +
                "WHERE \"userId\" = @Login and \"rightId\" = ANY(@RightIds);";

            var roleQuery = "DELETE FROM \"TestTaskSchema\".\"UserITRole\" " +
                "WHERE \"userId\" = @Login and \"roleId\" = ANY(@RoleIds);";

            var permissions = new UserPremissionsModel(rightIds);

            if (permissions.Requests.Count > 0)
                _dbConnection.Execute(rightQuery, new { Login = userLogin, RightIds = permissions.Requests });

            if(permissions.Roles.Count > 0)
                _dbConnection.Execute(roleQuery, new { Login = userLogin, RoleIds = permissions.Roles });
        }

        public void StartUp(string connectionString)
        {
            var connection = new ConnectionConfiguration(connectionString);

            _dbConnection = new NpgsqlConnection(connection.ConnectionString);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!IsUserExists(userLogin))
                throw new UserNotFoundException(userLogin);

            var user = GetUserObjectProperty(userLogin);

            user.UpdateProperties(properties);

            var query = "UPDATE \"TestTaskSchema\".\"User\" " +
                "SET " +
                "\"lastName\" = @LastName, " +
                "\"firstName\" = @FirstName, " +
                "\"middleName\" = @MiddleName, " +
                "\"telephoneNumber\" = @TelephoneNumber, " +
                "\"isLead\" = @IsLead " +
                "WHERE \"login\" = @Login";

            _dbConnection.Execute(query, user);

            query = "UPDATE \"TestTaskSchema\".\"Passwords\" " +
                "SET " +
                "\"password\" = @Password " +
                "WHERE \"userId\" = @Login";

            _dbConnection.Execute(query, user);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _dbConnection.Dispose();

            GC.SuppressFinalize(this);

            _disposed = true;
        }

        ~PostgreConnector()
        {
            Dispose();
        }
    }
}
