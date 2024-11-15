using Npgsql;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private class DB
        {
            private class ConnectionParameters
            {
                private string server = "";
                private int port = 0;
                private string database = "";
                private string username = "";
                private string password = "";
                public ConnectionParameters(string connectionString)
                {
                    var parameters = new Dictionary<string, string>();
                    var splittedString = connectionString.Split(';');
                    foreach (var parameter in splittedString)
                    {
                        var pair = parameter.Split('=');
                        if (pair.Length == 2)
                            parameters.Add(pair[0], pair[1]);
                    }
                }
            }
            private ConnectionParameters connectionParameters;
            private string _connectionString = "";
            private string provider = "";
            private string schemaName = "";
            public DB(string connector)
            {
                string pattern = @"'([^']*)'";
                Match match = Regex.Match(connector, pattern);
                if (match.Success)
                {
                    connectionParameters = new ConnectionParameters(match.Groups[0].Value.Trim('\''));
                    _connectionString = match.Groups[0].Value.Trim('\'');
                    provider = match.Groups[1].Value.Trim('\'');
                    schemaName = match.Groups[2].Value.Trim('\'');
                }
            }

            public string GetConnectionString()
            {
                return this._connectionString;
            }

            public override string ToString()
            {
                return $"{connectionParameters.ToString()}:{provider}:{schemaName}";
            }
        }

        Dictionary<string, Type> _dictionaryColumn = new Dictionary<string, Type>()
        {
            { "lastName", typeof(string) },
            { "firstName", typeof(string) },
            { "middleName", typeof(string) },
            { "telephoneNumber", typeof(string) },
            { "isLead", typeof(bool) },
        };

        private DB db;



        public void StartUp(string connectionString)
        {

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new NotImplementedException();
            db = new DB(connectionString);
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        Logger.Debug($"Connection Successfully");
                    }
                    else
                    {
                        Logger.Warn($"No connection");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
            }
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        string propertiesColumn = "";
                        string propertiesParam = "";
                        Dictionary<string, object> propertyValues = new Dictionary<string, object>();
                        foreach (var property in user.Properties)
                        {
                            propertiesColumn = propertiesColumn + "\"" + property.Name + "\"" + ", ";
                            propertiesParam = propertiesParam + "@" + property.Name + ", ";
                        }

                        foreach (var kvp in _dictionaryColumn)
                        {
                            if (!user.Properties.Any(p => p.Name == kvp.Key))
                            {
                                propertiesColumn += "\"" + kvp.Key + "\"" + ", ";
                                propertiesParam += "@" + kvp.Key + ", ";
                                propertyValues.Add(kvp.Key, GetDefaultValue(kvp.Value));
                            }
                        }
                        propertiesColumn = propertiesColumn.Trim().TrimEnd(',');
                        propertiesParam = propertiesParam.Trim().TrimEnd(',');


                        string query = @$"
                            INSERT INTO ""TestTaskSchema"".""Passwords"" (""userId"", password)
                            VALUES (@Login, @Password);
                            INSERT INTO ""TestTaskSchema"".""User"" (login, {propertiesColumn})
                            VALUES (@Login, {propertiesParam});";


                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Connection = connection;
                            command.Parameters.AddWithValue("@Login", user.Login);
                            command.Parameters.AddWithValue("@Password", user.HashPassword);
                            foreach (var kvp in propertyValues)
                            {
                                command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                            }
                            foreach (var property in user.Properties)
                            {
                                Type propertyType = _dictionaryColumn[property.Name];
                                object convertedValue = Convert.ChangeType(property.Value, propertyType);
                                command.Parameters.AddWithValue($"@{property.Name}", convertedValue ?? DBNull.Value);
                            }
                            command.ExecuteNonQuery();
                        }
                        Logger.Debug($"CreateUser Successfully");
                    }
                    else
                    {
                         Logger.Warn($"No connection");
                    }


                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var properties = new List<Property>
            {
            new Property("lastName", "Фамилия пользователя"),
            new Property("firstName", "Имя пользователя"),
            new Property("middleName", "Отчество пользователя"),
            new Property("telephoneNumber", "Номер телефона"),
            new Property("isLead", "Является ли пользователем лидером"),
            new Property("password", "Пароль пользователя")
            };
            return properties;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            List<UserProperty> columnValues = new List<UserProperty>();
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        string query = @$"
                                    SELECT 
                                    ""lastName"", ""firstName"", ""middleName"", ""telephoneNumber"", ""isLead""
                                    FROM ""TestTaskSchema"".""User"" where login = @Login LIMIT 100";


                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Connection = connection;
                            command.Parameters.AddWithValue("@Login", userLogin);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    var schemaTable = reader.GetSchemaTable();

                                    while (reader.Read())
                                    {
                                        foreach (DataRow row in schemaTable.Rows)
                                        {
                                            var columnName = row["ColumnName"].ToString();
                                            var value = reader[columnName] == DBNull.Value ? null : reader[columnName];
                                            columnValues.Add(new UserProperty(columnName, value.ToString()));
                                        }
                                        
                                    }
                                }
                            }
                        }
                        Logger.Debug($"GetUserProperties Successfully");
                        return columnValues;
                    }
                    else
                    {
                        Logger.Warn($"No connection");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
                return null;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            bool isUserExists = false;
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        string query = @$"
                                SELECT EXISTS (
                                SELECT 1 FROM ""TestTaskSchema"".""User"" WHERE ""login"" = @Login
                                );";


                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Connection = connection;
                            command.Parameters.AddWithValue("@Login", userLogin);
                            isUserExists = (bool)command.ExecuteScalar();
                        }
                        Logger.Debug($"IsUserExists Successfully");
                    }
                    else
                    {
                        Logger.Warn($"No connection");
                    }
                }
                return isUserExists;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        string propertiesParam = "";
                        foreach (var property in properties)
                        {
                            propertiesParam = propertiesParam + "\"" + property.Name + "\"" + " = " + "@" + property.Name + ", ";
                        }
                        propertiesParam = propertiesParam.Trim().TrimEnd(',');


                        string query = @$"
                            UPDATE ""TestTaskSchema"".""User"" SET {propertiesParam} WHERE login = @Login";


                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Connection = connection;
                            foreach (var property in properties)
                            {
                                Type propertyType = _dictionaryColumn[property.Name];
                                object convertedValue = Convert.ChangeType(property.Value, propertyType);
                                command.Parameters.AddWithValue($"@{property.Name}", convertedValue ?? DBNull.Value);
                            }
                            command.Parameters.AddWithValue("@Login", userLogin);
                            command.ExecuteNonQuery();
                        }
                        Logger.Debug($"IsUserExists Successfully");
                    }
                    else
                    {
                        Logger.Warn($"No connection");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            List<Permission> columnValues = new List<Permission>();
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        string query = @$"
                                    SELECT id, name FROM ""TestTaskSchema"".""ItRole"" 
                                    UNION ALL 
                                    SELECT id, name FROM ""TestTaskSchema"".""RequestRight""";


                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Connection = connection;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        columnValues.Add(new Permission(((int)reader["id"]).ToString(), (string)reader["name"], ""));
                                    }
                                }
                            }
                        }
                        Logger.Debug($"GetAllPermissions Successfully");
                        return columnValues;
                    }
                    else
                    {
                        Logger.Warn($"No connection");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
                return null;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        foreach(var id in rightIds)
                        {
                            string query = "";
                            if (id.Split(":")[0].Equals("Role"))
                            {
                                query = @"INSERT INTO ""TestTaskSchema"".""UserITRole"" (""userId"",""roleId"") VALUES (@userId, @id);";
                            }
                            else
                            {
                                query = @"INSERT INTO ""TestTaskSchema"".""UserRequestRight"" (""userId"",""rightId"") VALUES (@userId, @id);";
                            }

                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.Connection = connection;
                                command.Parameters.AddWithValue("@userId", userLogin);
                                command.Parameters.AddWithValue("@id", int.Parse(id.Split(":")[1]));
                                command.ExecuteNonQuery();
                            }
                        }
                        Logger.Debug($"AddUserPermissions Successfully");
                    }
                    else
                    {
                        Logger.Warn($"No connection");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        foreach (var id in rightIds)
                        {
                            string query = "";
                            if (id.Split(":")[0].Equals("Role"))
                            {
                                query = @"DELETE FROM ""TestTaskSchema"".""UserITRole"" where ""userId"" = @userId and ""roleId"" = @id;";

                            }
                            else
                            {
                                query = @"DELETE FROM ""TestTaskSchema"".""UserRequestRight"" where ""userId"" = @userId and ""rightId"" = @id;";
                            }

                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.Connection = connection;
                                command.Parameters.AddWithValue("@userId", userLogin);
                                command.Parameters.AddWithValue("@id", int.Parse(id.Split(":")[1]));
                                command.ExecuteNonQuery();
                            }
                        }
                        Logger.Debug($"RemoveUserPermissions Successfully");
                    }
                    else
                    {
                        Logger.Warn($"No connection");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var columnValues = new List<string>();
            try
            {
                using (var connection = new NpgsqlConnection(db.GetConnectionString()))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        string query = @$"
                                    SELECT ""roleId"" FROM ""TestTaskSchema"".""UserITRole"" where ""userId"" = @login
                                    UNION ALL 
                                    SELECT ""rightId"" FROM ""TestTaskSchema"".""UserRequestRight"" where ""userId"" = @login";


                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Connection = connection;
                            command.Parameters.AddWithValue("@login", userLogin);
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.GetSchemaTable().Rows.Cast<DataRow>()
                                                .Any(row => row["ColumnName"].ToString() == "roleId"))
                                        {
                                            columnValues.Add(((int)reader["roleId"]).ToString());
                                        }

                                        if (reader.GetSchemaTable().Rows.Cast<DataRow>()
                                                .Any(row => row["ColumnName"].ToString() == "rightId"))
                                        {
                                            columnValues.Add(((int)reader["rightId"]).ToString());
                                        }
                                    }
                                }
                            }
                        }
                        Logger.Debug($"GetUserPermissions Successfully");
                        return columnValues;
                    }
                    else
                    {
                        Logger.Warn($"No connection");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ DateTime.Now} | {ex.Message}");
                return null;
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }
            else if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public ILogger Logger { get; set; }
    }
}