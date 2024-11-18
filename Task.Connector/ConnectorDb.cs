using Npgsql;
using System.Linq;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private NpgsqlDataSource? _dataSource = null;

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
            try
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
            catch (Exception ex)
            {
                Logger.Error($"Ошибка создания пользователя: {ex.Message}");
            }
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
                new Property(_telephoneNumber, "Номер телефона"),
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

            try
            {
                using var cmd = _dataSource.CreateCommand(
                $"""
                SELECT "lastName", "firstName", "middleName", "telephoneNumber", "isLead"
                FROM "TestTaskSchema"."User"
                WHERE "login" = '{userLogin}';
                """);
                using var reader = cmd.ExecuteReader();

                reader.Read();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    result.Add(new UserProperty(reader.GetName(i), reader.GetValue(i).ToString()));
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения всех значений свойств пользователей: {ex.Message}");
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
            try
            {
                using var cmd = _dataSource.CreateCommand($"SELECT \"login\" FROM \"TestTaskSchema\".\"User\" WHERE \"login\" = '{userLogin}'");
                using var reader = cmd.ExecuteReader();
                return reader.Read();
            }
            catch (Exception ex) 
            {
                Logger.Error($"Ошибка проверки существования пользователя: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Устанавливать значения свойств пользователя
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="userLogin"></param>
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
            try
            {
                using var firstCmd = _dataSource.CreateCommand(
                $"""
                SELECT id, name
                FROM "TestTaskSchema"."ItRole";
                """);
                using var firstReader = firstCmd.ExecuteReader();

                while (firstReader.Read())
                    result.Add(new Permission(firstReader["id"].ToString(), firstReader["name"].ToString(), "Роль"));

                using var secondCmd = _dataSource.CreateCommand(
                    $"""
                SELECT id, name
                FROM "TestTaskSchema"."RequestRight";
                """);
                using var secondReader = secondCmd.ExecuteReader();

                while (secondReader.Read())
                    result.Add(new Permission(secondReader["id"].ToString(), secondReader["name"].ToString(), "Права"));
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения всех прав в системе: {ex.Message}");
            }
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
            try
            {
                using var connection = _dataSource.OpenConnection();
                using var transaction = connection.BeginTransaction();
                foreach (var id in rightIds)
                {
                    var cmd = new NpgsqlCommand();
                    if (id.Contains("Role"))
                    {
                        cmd = new NpgsqlCommand(
                            $"""
                        INSERT INTO "TestTaskSchema"."UserITRole"(
                        "userId", "roleId")
                        VALUES ('{userLogin}', '{id.Replace("Role", "").Replace(":", "")}');
                        """, connection, transaction);
                        cmd.ExecuteNonQuery();
                    }
                    else if (id.Contains("Request"))
                    {
                        cmd = new NpgsqlCommand(
                            $"""
                        INSERT INTO "TestTaskSchema"."UserRequestRight"(
                        "userId", "rightId")
                        VALUES ('{userLogin}', '{id.Replace("Role", "").Replace(":", "")}');
                        """, connection, transaction);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        Logger.Warn("Неправильно заданы параметры");
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка добавления прав пользователю: {ex.Message}");
            }
        }

        /// <summary>
        /// Удалить права пользователю в системе
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="rightIds">Идентификатор прав/роли</param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                using var connection = _dataSource.OpenConnection();
                using var transaction = connection.BeginTransaction();
                foreach (var id in rightIds)
                {
                    var cmd = new NpgsqlCommand();
                    if (id.Contains("Role"))
                    {
                        cmd = new NpgsqlCommand(
                            $"""
                        DELETE FROM "TestTaskSchema"."UserITRole"
                        WHERE "userId" = '{userLogin}' AND "roleId" = '{id.Replace("Role", "").Replace(":", "")}';
                        """, connection, transaction);
                        cmd.ExecuteNonQuery();
                    }
                    else if (id.Contains("Request"))
                    {
                        cmd = new NpgsqlCommand(
                            $"""
                        DELETE FROM "TestTaskSchema"."UserRequestRight"
                        WHERE "userId" = '{userLogin}' AND "rightId" = '{id.Replace("Request", "").Replace(":", "")}';
                        """, connection, transaction);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        Logger.Warn("Неправильно заданы параметры");
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка удаления прав пользователю: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить права пользователя в системе
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var result = new List<string>();
            try
            {
                using var firstCmd = _dataSource.CreateCommand(
                    $"""
                SELECT "roleId"
                FROM "TestTaskSchema"."UserITRole"
                WHERE "userId" = '{userLogin}';
                """);
                using var firstReader = firstCmd.ExecuteReader();

                while (firstReader.Read())
                    result.Add(firstReader["roleId"].ToString());

                using var secondCmd = _dataSource.CreateCommand(
                    $"""
                SELECT "rightId"
                FROM "TestTaskSchema"."UserRequestRight"
                WHERE "userId" = '{userLogin}';
                """);
                using var secondReader = secondCmd.ExecuteReader();

                while (secondReader.Read())
                    result.Add(secondReader["rightId"].ToString());
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения прав пользователя: {ex.Message}");
            }

            return result;
        }

        public ILogger Logger { get; set; }
    }
}