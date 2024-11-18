using Npgsql;
using System.Linq;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private NpgsqlDataSource _dataSource;

        private const string _login = "login";
        private const string _lastName = "lastName";
        private const string _firstName = "firstName";
        private const string _middleName = "middleName";
        private const string _telephoneNumber = "telephoneNumber";
        private const string _isLead = "isLead";
        private const string _password = "password";
        /// <summary>
        /// Конфигурация коннектора
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public void StartUp(string connectionString)
        {
            _dataSource = NpgsqlDataSource.Create(connectionString);
        }

        /// <summary>
        /// Создать пользователя с набором свойств по умолчанию
        /// </summary>
        /// <param name="user">Данные пользователя</param>
        public void CreateUser(UserToCreate user)
        {
            using var connection = _dataSource.OpenConnection();
            using var transaction = connection.BeginTransaction();

            using var cmd1 = new NpgsqlCommand($"""
                INSERT INTO "TestTaskSchema"."User" 
                ("{_login}", "{_lastName}", "{_firstName}", "{_middleName}", "{_telephoneNumber}", "{_isLead}") 
                VALUES 
                (@{_login}, @{_lastName}, @{_firstName}, @{_middleName}, @{_telephoneNumber}, @{_isLead})
                """, connection, transaction);

            cmd1.Parameters.AddWithValue(_login, user.Login);
            cmd1.Parameters.AddWithValue(_lastName, user.Properties.FirstOrDefault(x => x.Name == _lastName)?.Value ?? "");
            cmd1.Parameters.AddWithValue(_firstName, user.Properties.FirstOrDefault(x => x.Name == _firstName)?.Value ?? "");
            cmd1.Parameters.AddWithValue(_middleName, user.Properties.FirstOrDefault(x => x.Name == _middleName)?.Value ?? "");
            cmd1.Parameters.AddWithValue(_telephoneNumber, user.Properties.FirstOrDefault(x => x.Name == _telephoneNumber)?.Value ?? "");
            cmd1.Parameters.AddWithValue(_isLead, (user.Properties.FirstOrDefault(x => x.Name == _isLead)?.Value ?? "false") == "true" ? true : false);

            cmd1.ExecuteNonQuery();

            using var cmd2 = new NpgsqlCommand($"""
                INSERT INTO "TestTaskSchema"."Passwords" 
                ("userId", "{_password}") 
                VALUES
                ('{user.Login}', '{user.HashPassword}')
                """, connection, transaction);

            cmd2.ExecuteNonQuery();

            transaction.Commit();
        }

        /// <summary>
        /// Получить все свойства пользователя
        /// </summary>
        /// <returns>Список свойств пользователя</returns>
        public IEnumerable<Property> GetAllProperties()
        {
            return new List<Property>()
            {
                new Property(_firstName, "Имя"),
                new Property(_middleName, "Отчество"),
                new Property(_lastName, "Фамилия"),
                new Property(_isLead, "Главный"),
                new Property(_password, "Пароль")
            };
        }

        /// <summary>
        /// Получить все значения свойств пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <returns>Значения свойств пользователя</returns>
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var result = new List<UserProperty>();
            using var cmd = _dataSource.CreateCommand(
                $"""
                SELECT "lastName", "firstName", "middleName", "telephoneNumber", "isLead", "password"
                FROM "TestTaskSchema"."User"
                JOIN "TestTaskSchema"."Passwords"
                ON "TestTaskSchema"."Passwords"."userId" = "TestTaskSchema"."User"."login"
                WHERE "login" = '{userLogin}';
                """);
            using var reader = cmd.ExecuteReader();

            reader.Read();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.Add(new UserProperty(reader.GetName(i), reader.GetValue(i).ToString()));
            }
            return result;
        }

        /// <summary>
        /// Проверка существования пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользоватлея</param>
        /// <returns>true/false</returns>
        public bool IsUserExists(string userLogin)
        {
            using var cmd = _dataSource.CreateCommand($"SELECT \"login\" FROM \"TestTaskSchema\".\"User\" WHERE \"login\" = '{userLogin}'");
            using var reader = cmd.ExecuteReader();

            return reader.Read();
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            
        }

        /// <summary>
        /// Получить все права в системе
        /// </summary>
        /// <returns>Все права в системе</returns>
        public IEnumerable<Permission> GetAllPermissions()
        {
            var result = new List<Permission>();
            using var cmd = _dataSource.CreateCommand(
                $"""
                SELECT id, name
                FROM "TestTaskSchema"."ItRole";
                """);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                result.Add(new Permission(reader["id"].ToString(), reader["name"].ToString(), "Роль"));

            using var cmd1 = _dataSource.CreateCommand(
                $"""
                SELECT id, name
                FROM "TestTaskSchema"."RequestRight";
                """);
            using var reader1 = cmd1.ExecuteReader();

            while (reader1.Read())
                result.Add(new Permission(reader1["id"].ToString(), reader1["name"].ToString(), "Права"));

            return result;
        }

        /// <summary>
        /// Добавить права пользователю в системе
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="rightIds">Идентификатор прав/роли</param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using var connection = _dataSource.OpenConnection();
            using var transaction = connection.BeginTransaction();
            foreach (var id in rightIds)
            {
                if (rightIds.Contains("Role"))
                {
                    using var command1 = new NpgsqlCommand(
                        $"""
                        INSERT INTO "TestTaskSchema"."UserITRole"(
                        "userId", "roleId")
                        VALUES ('{userLogin}', '{id.Replace("Role","").Replace(":", "")}');
                        """, connection, transaction);
                    command1.ExecuteNonQuery();
                }
                else if (rightIds.Contains("Request"))
                {
                    using var command1 = new NpgsqlCommand(
                        $"""
                        INSERT INTO "TestTaskSchema"."UserRequestRight"(
                        "userId", "rightId")
                        VALUES ('{userLogin}', '{id.Replace("Role", "").Replace(":", "")}');
                        """, connection, transaction);
                    command1.ExecuteNonQuery();
                }
                else
                {

                }
            }

            transaction.Commit();
        }

        /// <summary>
        /// Удалить права пользователю в системе (переделать)
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="rightIds">Идентификатор прав/роли</param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using var connection = _dataSource.OpenConnection();
            using var transaction = connection.BeginTransaction();
            foreach (var id in rightIds)
            {
                if (rightIds.Contains("Role"))
                {
                    using var command1 = new NpgsqlCommand(
                        $"""
                        INSERT INTO "TestTaskSchema"."UserITRole"(
                        "userId", "roleId")
                        VALUES ('{userLogin}', '{id.Replace("Role", "").Replace(":", "")}');
                        """, connection, transaction);
                    command1.ExecuteNonQuery();
                }
                else if (rightIds.Contains("Request"))
                {
                    using var command1 = new NpgsqlCommand(
                        $"""
                        INSERT INTO "TestTaskSchema"."UserRequestRight"(
                        "userId", "rightId")
                        VALUES ('{userLogin}', '{id.Replace("Role", "").Replace(":", "")}');
                        """, connection, transaction);
                    command1.ExecuteNonQuery();
                }
                else
                {

                }
            }

            transaction.Commit();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }

        public ILogger Logger { get; set; }
    }
}