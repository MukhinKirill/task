using Dapper;
using Npgsql;
using System.Data.Common;
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

        private DbConnection _dbConnection;

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void CreateUser(UserToCreate user)
        {
            var query = "INSERT INTO \"TestTaskSchema\".\"User\"( " +
                "login, \"lastName\", \"firstName\", \"middleName\", \"telephoneNumber\", \"isLead\") " +
                "VALUES (@Login, @LastName, @FirstName, @MiddleName, @TelephoneNumber, @IsLead)";

            var parameters = new UserObjectCreateParamaters(user.Login, user.Properties);

            var completed = _dbConnection.Execute(query, parameters) != 0;

            if (!completed)
                return;

            query = "INSERT INTO \"TestTaskSchema\".\"Passwords\"( " +
                "\"userId\", \"password\") " +
                "VALUES (@Login, @Password);";

            _dbConnection.Execute(query, new { parameters.Login, Password = user.HashPassword });
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var query = "SELECT " +
                "\"id\" as Id, " +
                "\"name\" as Name " +
                "FROM \"TestTaskSchema\".\"RequestRight\"";

            var permissions = _dbConnection.Query<PermissionModel>(query)
                .ToList();

            query = "SELECT \"id\" as Id, \"name\" as Name FROM \"TestTaskSchema\".\"ItRole\"";
            var roles = _dbConnection.Query<PermissionModel>(query);

            permissions.AddRange(roles);

            return permissions.Select(x=> new Permission(x.Id, x.Name, string.Empty));
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return typeof(UserObjectPropertyModel).GetProperties()
                .Where(x => x.Name != nameof(UserObjectPropertyModel.Login))
                .Select(x => new Property(x.Name, $"User property - {x.Name}"));
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
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
            throw new NotImplementedException();
        }

        public void StartUp(string connectionString)
        {
            var connection = new ConnectionConfiguration(connectionString);

            _dbConnection = new NpgsqlConnection(connection.ConnectionString);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var user = GetUserObjectProperty(userLogin);

            user.UpdateObject(properties);

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
