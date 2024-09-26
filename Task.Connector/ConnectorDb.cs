using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Reflection;
using System.Configuration;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ConnectorDb() 
        {

        }

        public ConnectorDb(ILogger logger)
        {
            Logger = logger;
        }

        private string schemaName = "dbo";

        private static readonly string[] tableNames = new[] { "RequestRight", "ItRole" }; // названия таблиц в бд (также нужны при обращении к таблицам User...)
        private static readonly string[] groupNames = new[] { "Request", "Role" }; // названия прав для коннектора
        private static readonly string delimeter = ":";

        private SqlConnection? connect;
        private void CheckConnection()
        {
            if (connect is null)
            {
                Logger?.Error("Не установлено соединение с базой данных!");
                throw new Exception("Не установлено соединение с базой данных!");
            }
        }

        private DataTable ReadDataFromDB(string command)
        {
            DataTable dt = new();

            SqlCommand cmd = new(command, connect);
            connect!.Open();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message);
                Logger?.Debug($"Ошибка вызвана при выполнении запроса {command}");
                throw new Exception(ex.Message);
            }
            finally
            {
                connect.Close();
            }
            
            return dt;
        }

        private void ChangeDataInDB(string command)
        {
            SqlCommand cmd = new(command, connect);
            connect!.Open();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message);
                Logger?.Debug($"Ошибка вызвана при выполнении запроса {command}");
                throw new Exception(ex.Message);
            }
            finally
            {
                connect.Close();
            }
        }

        private void LogStartMethod(string methodName)
        {
            Logger?.Debug($"Запущен метод {methodName}");
        }

        // Конфигурация коннектора через строку подключения (настройки для подключения к ресурсу(строка подключения к бд, 
        // путь к хосту с логином и паролем, дополнительные параметры конфигурации бизнес-логики и тд, формат любой, например: "key1=value1;key2=value2..."
        public void StartUp(string connectionString)
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            if (!connectionString.Contains('\''))
            {
                Logger?.Error("Строка подключения коннектора доджна иметь вид \"key1='value1';key2='value2';...\"");
                throw new Exception("Строка подключения коннектора доджна иметь вид \"key1='value1';key2='value2';...\"");
            }

            ConnectionStringSettings settings = new();

            if (connectionString[^1] == ';')
            {
                connectionString = connectionString.Remove(connectionString.Length - 1);
            }
            connectionString = connectionString.Remove(connectionString.Length - 1); // удаляем символ '

            var configComponents = connectionString.Split("';");

            var namesOfComponents = new string[configComponents.Length];
            for (int i = 0; i < configComponents.Length; i++)
            {
                namesOfComponents[i] = configComponents[i].Split("='")[0];
                configComponents[i] = configComponents[i].Split("='")[1];
            }

            for (int i = 0; i < namesOfComponents.Length; i++)
            {
                if (namesOfComponents[i] == "ConnectionString")
                {
                    settings.ConnectionString = configComponents[i];
                    connect = new(settings.ConnectionString);
                }
                else if (namesOfComponents[i] == "Provider")
                {
                    settings.ProviderName = configComponents[i];
                }
                else if (namesOfComponents[i] == "SchemaName")
                {
                    schemaName = configComponents[i];
                }
            }
        }

        // Создать пользователя с набором свойств по умолчанию
        public void CreateUser(UserToCreate user)
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            CheckConnection();

            if (IsUserExists(user.Login))
            {
                Logger?.Warn($"Пользователь с логином {user.Login} уже существует");
            }
            else
            {
                StringBuilder query_sb = new();
                query_sb.Append($"INSERT INTO [{connect!.Database}].[{schemaName}].[User] VALUES (N'{user.Login}',");

                string value;
                var allProperties = GetAllProperties();
                for (int i = 0; i < allProperties.Count() - 1; i++)
                {
                    if (!user.Properties.Select(prop => prop.Name).Contains(allProperties.ElementAt(i).Name))
                    {
                        if (allProperties.ElementAt(i).Name != "isLead")
                        {
                            value = string.Empty;
                        }
                        else
                        {
                            value = "false";
                        }
                    }
                    else
                    {
                        value = user.Properties.Where(prop => prop.Name == allProperties.ElementAt(i).Name).Select(prop => prop.Value).First();

                        if (allProperties.ElementAt(i).Name == "isLead")
                        {
                            if (value.ToLower() != "false" && value.ToLower() != "true" && value != "0" && value != "1")
                            {
                                Logger?.Error("Значение свойства isLead должно быть либо false, либо true");
                                throw new Exception("Значение свойства isLead должно быть либо false, либо true");
                            }
                        }
                    }

                    query_sb.Append($"N'{value}',");
                }
                query_sb.Replace(',', ')', query_sb.Length - 1, 1);

                ChangeDataInDB(query_sb.ToString());
                ChangeDataInDB($"INSERT INTO [{connect.Database}].[{schemaName}].[Passwords] VALUES(N'{user.Login}', N'{user.HashPassword}')");
            }
        }

        // Метод позволяет получить все свойства пользователя(смотри Описание системы), пароль тоже считать свойством
        public IEnumerable<Property> GetAllProperties()
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            CheckConnection();

            List<Property> allProperties = new();

            var namesOfProperties = ReadDataFromDB($"SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('[{connect!.Database}].[{schemaName}].[User]')").AsEnumerable();
            for (int i = 1; i < namesOfProperties.Count(); i++)
            {
                allProperties.Add( new(namesOfProperties.ElementAt(i).ItemArray[0].ToString(), string.Empty) );
            }

            namesOfProperties = ReadDataFromDB($"SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('[{connect.Database}].[{schemaName}].[Passwords]')").AsEnumerable();
            for (int i = 2; i < namesOfProperties.Count(); i++)
            {
                allProperties.Add( new(namesOfProperties.ElementAt(i).ItemArray[0].ToString(), string.Empty) );
            }

            return allProperties;
        }

        // Получить все значения свойств пользователя
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            CheckConnection();

            List<UserProperty> userProps = new();

            if (IsUserExists(userLogin))
            {
                StringBuilder query_sb = new();
                query_sb.Append("SELECT ");

                var allProperties = GetAllProperties();
                foreach (var property in allProperties.Where(prop => prop.Name != "password"))
                {
                    query_sb.Append($"{property.Name},");
                }
                query_sb.Replace(',', ' ', query_sb.Length - 1, 1);

                query_sb.Append($"FROM [{connect!.Database}].[{schemaName}].[User] WHERE login = N'{userLogin}'");

                var props = ReadDataFromDB(query_sb.ToString());

                for (int i = 0; i < props.AsEnumerable().First().ItemArray.Length; i++)
                {
                    userProps.Add(new(props.Columns[i].ColumnName, props.AsEnumerable().First().ItemArray[i].ToString()));
                }
            }
            else
            {
                Logger?.Warn($"Пользователя с логином {userLogin} не существует");
            }

            return userProps;
        }

        // Проверка существования пользователя
        public bool IsUserExists(string userLogin)
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            CheckConnection();

            SqlCommand cmd = new($"SELECT COUNT(*) FROM [{connect!.Database}].[{schemaName}].[User] WHERE login = N'{userLogin}'", connect);
            connect!.Open();

            try
            {
                int countUsers = Convert.ToInt32(cmd.ExecuteScalar());

                return countUsers > 0;
            }
            catch
            {
                Logger?.Error($"Не удалось получить информацию о пользователе {userLogin}");
                throw new Exception($"Не удалось получить информацию о пользователе {userLogin}");
            }
            finally
            {
                connect.Close();
            }
        }

        // Метод позволяет устанавливать значения свойств пользователя
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            CheckConnection();

            if (IsUserExists(userLogin))
            {
                if (properties.Any())
                {
                    bool isChanged = false;
                    StringBuilder query_sb = new();
                    query_sb.Append($"UPDATE [{connect!.Database}].[{schemaName}].[User] SET ");

                    var allProperties = GetAllProperties();
                    foreach (var property in allProperties.Where(prop => prop.Name != "password"))
                    {
                        if (properties.Select(prop => prop.Name).Contains(property.Name))
                        {
                            query_sb.Append($"{property.Name} = N'{properties.Where(prop => prop.Name == property.Name).Select(prop => prop.Value).First()}',");
                            if (!isChanged)
                            {
                                isChanged = true;
                            }
                        }
                    }
                    if (isChanged)
                    {
                        query_sb.Remove(query_sb.Length - 1, 1);
                        query_sb.Append($" WHERE login = N'{userLogin}'");

                        ChangeDataInDB(query_sb.ToString());
                    }
                    else
                    {
                        Logger?.Warn($"Метод {MethodBase.GetCurrentMethod().Name} завершился без обращения к базе данных");
                    }
                }
                else
                {
                    Logger?.Warn($"Введён пустой перечень для изменения свойств");
                }
            }
            else
            {
                Logger?.Warn($"Пользователя с логином {userLogin} не существует");
            }
        }

        // Получить все права в системе (смотри Описание системы клиента)
        public IEnumerable<Permission> GetAllPermissions()
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            CheckConnection();

            List<Permission> permissions = new();
            
            for (int i = 0; i < tableNames.Length; i++)
            {
                var perms = ReadDataFromDB($"SELECT * FROM [{connect!.Database}].[{schemaName}].[{tableNames[i]}]");
                for (int j = 0; j < perms.AsEnumerable().Count(); j++)
                {
                    permissions.Add(new($"{groupNames[i]}{delimeter}{perms.AsEnumerable().ElementAt(j).ItemArray[0]}", perms.AsEnumerable().ElementAt(j).ItemArray[1].ToString(), string.Empty));
                }
            }

            return permissions;
        }

        // Добавить права пользователю в системе
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            if (rightIds.Any() && IsUserExists(userLogin))
            {
                var allRights = GetAllPermissions().Select(perm => perm.Id);
                var existsRights = GetUserPermissions(userLogin);
                bool isChangedRequest = false, isChangedRole = false;

                StringBuilder queryToRequest_sb = new();
                StringBuilder queryToRole_sb = new();
                queryToRequest_sb.Append($"INSERT INTO [{connect!.Database}].[{schemaName}].[User{tableNames[0]}] VALUES");
                queryToRole_sb.Append($"INSERT INTO [{connect!.Database}].[{schemaName}].[User{tableNames[1]}] VALUES");

                foreach (var newRight in rightIds)
                {
                    if (allRights.Contains(newRight))
                    {
                        if (!existsRights.Contains(newRight))
                        {
                            if (newRight.Contains(groupNames[0]))
                            {
                                queryToRequest_sb.Append($"(N'{userLogin}', {newRight.Replace($"{groupNames[0]}{delimeter}","")}),");

                                if (!isChangedRequest)
                                {
                                    isChangedRequest = true;
                                }
                            }
                            else
                            {
                                queryToRole_sb.Append($"(N'{userLogin}', {newRight.Replace($"{groupNames[1]}{delimeter}", "")}),");

                                if (!isChangedRole)
                                {
                                    isChangedRole = true;
                                }
                            }

                            existsRights = existsRights.Append(newRight);
                        }
                    }
                    else
                    {
                        Logger?.Warn($"Не удалось идентифицировать право с id {newRight}");
                    }
                }

                if (isChangedRequest) 
                {
                    queryToRequest_sb.Remove(queryToRequest_sb.Length - 1, 1);

                    ChangeDataInDB(queryToRequest_sb.ToString());
                }
                if (isChangedRole)
                {
                    queryToRole_sb.Remove(queryToRole_sb.Length - 1, 1);

                    ChangeDataInDB(queryToRole_sb.ToString());
                }
                if (!isChangedRequest && !isChangedRole)
                {
                    Logger?.Warn($"Метод {MethodBase.GetCurrentMethod().Name} завершился без обращения к базе данных");
                }
            }
            else
            {
                CheckConnection();

                if (!IsUserExists(userLogin))
                {
                    Logger?.Warn($"Пользователя с логином {userLogin} не существует");
                }
                else
                {
                    Logger?.Warn($"Введён пустой перечень для добавления прав");
                }
            }
        }

        // Удалить права пользователю в системе
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            if (rightIds.Any() && IsUserExists(userLogin))
            {
                var existsRights = GetUserPermissions(userLogin);
                bool isChangedRequest = false, isChangedRole = false;

                StringBuilder queryToRequest_sb = new();
                StringBuilder queryToRole_sb = new();
                queryToRequest_sb.Append($"DELETE FROM [{connect!.Database}].[{schemaName}].[User{tableNames[0]}] WHERE userId = N'{userLogin}' AND rightId IN (");
                queryToRole_sb.Append($"DELETE FROM [{connect!.Database}].[{schemaName}].[User{tableNames[1]}] WHERE userId = N'{userLogin}' AND roleId IN (");

                foreach (var delRight in rightIds)
                {
                    if (existsRights.Contains(delRight))
                    {
                        if (delRight.Contains(groupNames[0]))
                        {
                            queryToRequest_sb.Append($"{delRight.Replace($"{groupNames[0]}{delimeter}", "")},");

                            if (!isChangedRequest)
                            {
                                isChangedRequest = true;
                            }
                        }
                        else
                        {
                            queryToRole_sb.Append($"{delRight.Replace($"{groupNames[1]}{delimeter}", "")},");

                            if (!isChangedRole)
                            {
                                isChangedRole = true;
                            }
                        }

                        existsRights = existsRights.Where(right => right != delRight);
                    }
                    else
                    {
                        Logger?.Warn($"Не удалось найти право с id {delRight}");
                    }
                }

                if (isChangedRequest)
                {
                    queryToRequest_sb.Replace(',', ')', queryToRequest_sb.Length - 1, 1);

                    ChangeDataInDB(queryToRequest_sb.ToString());
                }
                if (isChangedRole)
                {
                    queryToRole_sb.Replace(',', ')', queryToRole_sb.Length - 1, 1);

                    ChangeDataInDB(queryToRole_sb.ToString());
                }
                if (!isChangedRequest && !isChangedRole)
                {
                    Logger?.Warn($"Метод {MethodBase.GetCurrentMethod().Name} завершился без обращения к базе данных");
                }
            }
            else
            {
                CheckConnection();

                if (!IsUserExists(userLogin))
                {
                    Logger?.Warn($"Пользователя с логином {userLogin} не существует");
                }
                else
                {
                    Logger?.Warn($"Введён пустой перечень для удаления прав");
                }
            }
        }

        // Получить права пользователя в системе
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            LogStartMethod(MethodBase.GetCurrentMethod().Name);

            CheckConnection();

            List<string> permissionIDs = new();

            if (IsUserExists(userLogin))
            {
                string[] columnNames = new[] { "rightId", "roleId" };
                for (int i = 0; i < tableNames.Length; i++)
                {
                    var perms = ReadDataFromDB($"SELECT {columnNames[i]} FROM [{connect!.Database}].[{schemaName}].[User{tableNames[i]}] WHERE userId = N'{userLogin}'").AsEnumerable();
                    foreach (var row in perms)
                    {
                        permissionIDs.Add($"{groupNames[i]}{delimeter}{row.ItemArray[0]}");
                    }
                }
            }
            else
            {
                Logger?.Warn($"Пользователя с логином {userLogin} не существует");
            }

            return permissionIDs;
        }

        public ILogger Logger { get; set; }
    }
}