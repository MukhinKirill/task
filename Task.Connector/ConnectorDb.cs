using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private SqlConnection connection;
        private DataContext dataContext;

        public ConnectorDb()
        {

        }

        public void StartUp(string connectionString)
        {
            // Создание подключения
            connection = new SqlConnection(connectionString);
            try
            {
                // Открываем подключение
                connection.Open();
                Logger?.Debug("[StartUp()] Успешное подлключение к базе данных...");
            }
            catch (SqlException ex)
            {
                Logger?.Debug("[StartUp()] Ошибка при открытии подключения к БД: " + ex.Message);
            }
        }

        public void CreateUser(UserToCreate user)
        {
            bool userExists = IsUserExists(user.Login);
            if (userExists)
            {
                Logger?.Error("[CreateUser()] Пользователь с таким логином уже существует!");
                return;
            }

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SqlCommand comm = connection.CreateCommand())
                    {
                        comm.Transaction = transaction;

                        // Добавление пароля пользователя
                        comm.CommandText = "INSERT INTO [TestTaskSchema].[Passwords] (userId, password) VALUES (@userId, @password)";
                        comm.Parameters.AddWithValue("@userId", user.Login);
                        comm.Parameters.AddWithValue("@password", user.HashPassword);
                        comm.ExecuteNonQuery();

                        // Добавление пользователя с его свойствами 
                        comm.CommandText = "INSERT INTO [TestTaskSchema].[User] (login, lastName, firstName, middleName, telephoneNumber, isLead) VALUES (@login, @lastName, @firstName, @middleName, @telephoneNumber, @isLead)";
                        comm.Parameters.AddWithValue("@login", user.Login);
                        comm.Parameters.AddWithValue("@lastName", user.Properties.FirstOrDefault(x => x.Name == "lastName", new UserProperty("lastName", "")).Value);
                        comm.Parameters.AddWithValue("@firstName", user.Properties.FirstOrDefault(x => x.Name == "firstName", new UserProperty("firstName", "")).Value);
                        comm.Parameters.AddWithValue("@middleName", user.Properties.FirstOrDefault(x => x.Name == "middleName", new UserProperty("middleName", "")).Value);
                        comm.Parameters.AddWithValue("@telephoneNumber", user.Properties.FirstOrDefault(x => x.Name == "telephoneNumber", new UserProperty("telephoneNumber", "")).Value);
                        comm.Parameters.AddWithValue("@isLead", user.Properties.FirstOrDefault(x => x.Name == "isLead", new UserProperty("isLead", "")).Value);
                        comm.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    Logger?.Debug("[CreateUser()] Пользователь успешно добавлен!");
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Logger?.Error("[CreateUser()] Ошибка при добавлении пользователя: " + ex.Message);
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            //Если берется и свойство пароля в виде исключения из другой таблицы (непонятно зачем), то делать запрос в бд для получения столбцов тоже подумал неверно.
            //Если в будущем добавятся другие столбцы - тест покажет, их уже не 6 и можно будет поправить. Также добавляемые в будущем столбцы могут иметь тип,
            //отличный от String (как Boolean столбец isLead). Может там будет аватар пользователя. К этому всему нужно подготовить методы, которые работают со свойствами.
            //Если таких изменений сейчас не ожидается, то у нас сейчас были бы лишние запросы к БД.


            List<Property> properties = new List<Property>();
            properties.Add(new Property("password", "Password"));
            properties.Add(new Property("lastName", "Last Name"));
            properties.Add(new Property("firstName", "First Name"));
            properties.Add(new Property("middleName", "Middle Name"));
            properties.Add(new Property("telephoneNumber", "Telephone Number"));
            properties.Add(new Property("isLead", "Is Lead"));

            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger?.Error("[GetUserProperties()] Пользователя с таким логином не существует!");
                return null;
            }

            var userProperties = new List<UserProperty>();
            try
            {
                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = connection;
                    comm.CommandText = "SELECT * FROM [TestTaskSchema].[User] WHERE login = @login";

                    comm.Parameters.AddWithValue("@login", userLogin);

                    using (var reader = comm.ExecuteReader())
                    {
                        reader.Read();

                        var allProperties = GetAllProperties();

                        userProperties.Add(new UserProperty("lastName", reader.GetString(reader.GetOrdinal("lastName"))));
                        userProperties.Add(new UserProperty("firstName", reader.GetString(reader.GetOrdinal("firstName"))));
                        userProperties.Add(new UserProperty("middleName", reader.GetString(reader.GetOrdinal("middleName"))));
                        userProperties.Add(new UserProperty("telephoneNumber", reader.GetString(reader.GetOrdinal("telephoneNumber"))));
                        userProperties.Add(new UserProperty("isLead", reader.GetBoolean(reader.GetOrdinal("isLead")).ToString()));

                        Logger.Debug("[GetUserProperties()] Свойства пользователя успешно получены");
                    }
                }
            }
            catch (SqlException ex)
            {
                Logger.Error("[GetUserProperties()] Ошибка при получении свойств пользователя: " + ex.Message);
            }

            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            string cmdString = "SELECT COUNT(*) from [TestTaskSchema].[User] WHERE login = @login";
            try
            {
                using (SqlCommand comm = new SqlCommand(cmdString, connection))
                {
                    comm.Parameters.AddWithValue("@login", userLogin);

                    int userCount = (int)comm.ExecuteScalar();
                    return userCount > 0;
                }
            }
            catch (SqlException ex)
            {
                Logger.Error("Ошибка при проверке на наличие пользователя в БД: " + ex.Message);
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            if (!IsUserExists(userLogin))
            {
                Logger?.Error("[UpdateUserProperties()] Пользователя с таким логином не существует!");
                return;
            }

            using (SqlCommand comm = new SqlCommand())
            {
                comm.Connection = connection;

                SqlTransaction transaction = connection.BeginTransaction();
                comm.Transaction = transaction;

                try
                {
                    foreach (var userProperty in properties)
                    {
                        comm.CommandText = $"UPDATE [TestTaskSchema].[User] SET {userProperty.Name} = @value WHERE login = @login";
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@value", userProperty.Value);
                        comm.Parameters.AddWithValue("@login", userLogin);
                        comm.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    Logger?.Debug("[UpdateUserProperties()] Свойства пользователя успешно обновлены!");
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Logger?.Error($"[UpdateUserProperties()] Ошибка при обновлении свойств пользователя: {ex.Message}");
                }
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var permissions = new List<Permission>();

            // Запросы к таблицам ItRole и RequestRight
            var queryItRole = "SELECT id, name FROM [TestTaskSchema].[ItRole]";
            var queryRequestRight = "SELECT id, name FROM [TestTaskSchema].[RequestRight]";
            try
            {
                using (var comm = new SqlCommand(queryItRole))
                {
                    comm.Connection = connection;
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add(new Permission(reader.GetInt32(reader.GetOrdinal("id")).ToString(), reader.GetString(reader.GetOrdinal("name")), "ItRole"));
                        }
                    }
                }

                using (var comm = new SqlCommand(queryRequestRight))
                {
                    comm.Connection = connection;
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add(new Permission(reader.GetInt32(reader.GetOrdinal("id")).ToString(), reader.GetString(reader.GetOrdinal("name")), "RequestRight"));
                        }
                    }
                }

                Logger.Debug("[GetAllPermissions()] Список прав успешно получен");
            }
            catch(SqlException ex)
            {
                Logger.Error("[GetAllPermissions()] Ошибка при получении списка прав: " + ex.Message);
            }
            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                Logger?.Error("[AddUserPermissions()] Пользователя с таким логином не существует!");
                return;
            }

            using (SqlCommand comm = new SqlCommand())
            {
                comm.Connection = connection;
                SqlTransaction transaction = connection.BeginTransaction();
                comm.Transaction = transaction;

                try
                {
                    foreach (var rightId in rightIds)
                    {
                        string? table = DeterminePermissionTable(rightId);
                        string tableColumn = table == "UserITRole" ? "roleId" : "rightId";
                        if (table == null)
                        {
                            Logger?.Error($"[AddUserPermissions()] ID права {rightId} не соответствует ни одной из таблиц.");
                            continue;
                        }

                        comm.CommandText = $@"
                            IF NOT EXISTS (SELECT * FROM [TestTaskSchema].[{table}] WHERE userId = @login AND {tableColumn} = @rightId)
                            BEGIN
                                INSERT INTO [TestTaskSchema].[{table}] (userId, {tableColumn}) VALUES (@login, @rightId)
                            END";
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@login", userLogin);
                        comm.Parameters.AddWithValue("@rightId", rightId.Split(':')[1]);
                        comm.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    Logger?.Debug("[AddUserPermissions()] Права пользователя успешно добавлены!");
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Logger?.Error($"[AddUserPermissions()] Ошибка при добавлении прав пользователя: {ex.Message}");
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!IsUserExists(userLogin))
            {
                Logger?.Error("[RemoveUserPermissions()] Пользователя с таким логином не существует!");
                return;
            }

            using (SqlCommand comm = new SqlCommand())
            {
                comm.Connection = connection;
                SqlTransaction transaction = connection.BeginTransaction();
                comm.Transaction = transaction;

                try
                {
                    foreach (var rightId in rightIds)
                    {
                        string? table = DeterminePermissionTable(rightId);
                        string tableColumn = table == "UserITRole" ? "roleId" : "rightId";
                        if (table == null)
                        {
                            Logger?.Error($"[RemoveUserPermissions()] ID права {rightId} не соответствует ни одной из таблиц.");
                            continue;
                        }

                        comm.CommandText = $"DELETE FROM [TestTaskSchema].[{table}] WHERE userId = @login AND rightId = @rightId";
                        comm.Parameters.Clear();
                        comm.Parameters.AddWithValue("@login", userLogin);
                        comm.Parameters.AddWithValue("@rightId", rightId.Split(':')[1]);
                        comm.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    Logger?.Debug("[RemoveUserPermissions()] Права пользователя успешно удалены!");
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    Logger?.Error($"[RemoveUserPermissions()] Ошибка при удалении прав пользователя: {ex.Message}");
                }
            }
        }

        private string DeterminePermissionTable(string rightId)
        {
            if (rightId.StartsWith("Role:"))
            {
                return "UserITRole";
            }
            else if (rightId.StartsWith("Request:"))
            {
                return "UserRequestRight";
            }
            else
            {
                Logger?.Error("[DeterminePermissionTable()]: Передали неверный формат ID.");
                return null;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var permissions = new List<string>();

            if (!IsUserExists(userLogin))
            {
                Logger?.Error("[GetUserPermissions()] Пользователя с таким логином не существует!");
                return permissions; // Возвращаем пустой список, если пользователь не найден
            }


            string queryItRole = "SELECT roleId FROM [TestTaskSchema].[UserITRole] WHERE userId = @userLogin";
            string queryRequestRight = "SELECT rightId FROM [TestTaskSchema].[UserRequestRight] WHERE userId = @userLogin";
            try
            {
                using (var comm = new SqlCommand(queryItRole, connection))
                {
                    comm.Parameters.AddWithValue("@userLogin", userLogin);
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add("Role:" + reader.GetInt32(reader.GetOrdinal("roleId")).ToString());
                        }
                    }
                }

                using (var comm = new SqlCommand(queryRequestRight, connection))
                {
                    comm.Parameters.AddWithValue("@userLogin", userLogin);
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add("Request:" + reader.GetInt32(reader.GetOrdinal("rightId")).ToString());
                        }
                    }
                }

                Logger?.Debug("[GetUserPermissions()] Права пользователя успешно получены.");
            }
            catch (SqlException ex)
            {
                Logger?.Error("[GetUserPermissions()] Ошибка при получении прав пользователя: " + ex.Message);
            }
            return permissions;
        }

        public ILogger Logger { get; set; }
    }
}