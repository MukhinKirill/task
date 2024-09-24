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

        // Конфигурация коннектора через строку подключения (настройки для подключения к ресурсу(строка подключения к бд, 
        // путь к хосту с логином и паролем, дополнительные параметры конфигурации бизнес-логики и тд, формат любой, например: "key1=value1;key2=value2..."
        public void StartUp(string connectionString)
        {
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
            CheckConnection();
            if (IsUserExists(user.Login))
            {
                Logger?.Warn($"Пользователь с логином {user.Login} уже существует");
            }
            else
            {
                StringBuilder query_sb = new();
                query_sb.Append($"INSERT INTO [{connect!.Database}].[{schemaName}].[User] VALUES ('{user.Login}',");

                for (int i = 0; i < GetAllProperties().Count() - 1; i++)
                {
                    if (!user.Properties.Select(prop => prop.Name).Contains(GetAllProperties().ElementAt(i).Name))
                    {
                        throw new Exception($"Отсутствует свойство {GetAllProperties().ElementAt(i).Name}");
                    }

                    var value = user.Properties.Where(prop => prop.Name == GetAllProperties().ElementAt(i).Name).Select(prop => prop.Value).First();

                    if (GetAllProperties().ElementAt(i).Name == "isLead")
                    {
                        if (value.ToLower() != "false" && value.ToLower() != "true" && value != "0" && value != "1")
                        {
                            throw new Exception("Значение свойства isLead должно быть либо false, либо true");
                        }
                    }

                    query_sb.Append($"'{value}',");
                }
                query_sb.Replace(',', ')', query_sb.Length - 1, 1);

                ChangeDataInDB(query_sb.ToString());
                ChangeDataInDB($"INSERT INTO [{connect.Database}].[{schemaName}].[Passwords] VALUES('{user.Login}', '{user.HashPassword}')");
            }
        }

        // Метод позволяет получить все свойства пользователя(смотри Описание системы), пароль тоже считать свойством
        public IEnumerable<Property> GetAllProperties()
        {
            CheckConnection();

            List<Property> allProperties = new();

            var namesOfProperties = ReadDataFromDB($"SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('[{connect!.Database}].[{schemaName}].[User]')").AsEnumerable();
            for (int i = 1; i < namesOfProperties.Count(); i++)
            {
                allProperties.Add( new(namesOfProperties.ElementAt(i).ItemArray[0].ToString(), "") );
            }

            namesOfProperties = ReadDataFromDB($"SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('[{connect.Database}].[{schemaName}].[Passwords]')").AsEnumerable();
            for (int i = 2; i < namesOfProperties.Count(); i++)
            {
                allProperties.Add( new(namesOfProperties.ElementAt(i).ItemArray[0].ToString(), "") );
            }

            return allProperties;
        }

        // Получить все значения свойств пользователя
        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            CheckConnection();

            List<UserProperty> userProps = new();

            if (IsUserExists(userLogin))
            {
                StringBuilder query_sb = new();
                query_sb.Append("SELECT ");
                foreach (var property in GetAllProperties())
                {
                    query_sb.Append($"{property.Name},");
                }
                query_sb.Replace(',', ' ', query_sb.Length - 1, 1);

                query_sb.Append($"FROM [{connect!.Database}].[{schemaName}].[User] ");
                query_sb.Append($"INNER JOIN [{connect.Database}].[{schemaName}].[Passwords] ");
                query_sb.Append("ON login = userid ");
                query_sb.Append($"WHERE login = '{userLogin}'");

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
            CheckConnection();

            SqlCommand cmd = new($"SELECT COUNT(*) FROM [{connect!.Database}].[{schemaName}].[User] WHERE login = '{userLogin}'", connect);
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
            CheckConnection();

            if (IsUserExists(userLogin))
            {
                if (properties.Any())
                {
                    bool isChanged = false;
                    StringBuilder query_sb = new();
                    query_sb.Append($"UPDATE [{connect!.Database}].[{schemaName}].[User] SET ");

                    foreach (var property in GetAllProperties())
                    {
                        if (properties.Select(prop => prop.Name).Contains(property.Name))
                        {
                            query_sb.Append($"{property.Name} = '{properties.Where(prop => prop.Name == property.Name).Select(prop => prop.Value).First()}',");
                            if (!isChanged)
                            {
                                isChanged = true;
                            }
                        }
                    }
                    if (isChanged)
                    {
                        query_sb.Remove(query_sb.Length - 1, 1);
                        query_sb.Append($" WHERE login = '{userLogin}'");

                        ChangeDataInDB(query_sb.ToString());
                    }
                    else
                    {
                        Logger?.Warn($"Метод {MethodBase.GetCurrentMethod().Name} завершился без обращения к базе данных");
                    }
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
            CheckConnection();

            List<Permission> permissions = new();

            foreach (var typeOfPermission in new string[] { "RequestRight", "ItRole" })
            {
                var perms = ReadDataFromDB($"SELECT * FROM [{connect!.Database}].[{schemaName}].[{typeOfPermission}]");
                for (int i = 0; i < perms.AsEnumerable().Count(); i++)
                {
                    permissions.Add( new(perms.AsEnumerable().ElementAt(i).ItemArray[0].ToString(), perms.AsEnumerable().ElementAt(i).ItemArray[1].ToString(), typeOfPermission) );
                }
            }

            return permissions;
        }

        // Добавить права пользователю в системе
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (rightIds.Any())
            {
                var allRights = GetAllPermissions().Where(perm => perm.Description == "RequestRight").Select(perm => perm.Id);
                var existsRights = GetUserPermissions(userLogin);
                bool isChanged = false;

                StringBuilder query_sb = new();
                query_sb.Append($"INSERT INTO [{connect!.Database}].[{schemaName}].[UserRequestRight] VALUES");

                foreach (var newRight in rightIds)
                {
                    if (allRights.Contains(newRight))
                    {
                        if (!existsRights.Contains(newRight))
                        {
                            query_sb.Append($"('{userLogin}', {newRight}),");
                            existsRights = existsRights.Append(newRight);
                            if (!isChanged)
                            {
                                isChanged = true;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Не удалось обнаружить право с id {newRight}");
                    }
                }

                if (isChanged) 
                {
                    query_sb.Remove(query_sb.Length - 1, 1);

                    ChangeDataInDB(query_sb.ToString());
                }
                else
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
            }
        }

        // Удалить права пользователю в системе
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (rightIds.Any())
            {
                var existsRights = GetUserPermissions(userLogin);
                bool isChanged = false;

                StringBuilder query_sb = new();
                query_sb.Append($"DELETE FROM [{connect!.Database}].[{schemaName}].[UserRequestRight] WHERE userId = '{userLogin}' AND rightId IN (");
                
                foreach (var delRight in rightIds)
                {
                    if (existsRights.Contains(delRight))
                    {
                        query_sb.Append($"{delRight},");
                        existsRights = existsRights.Where(right => right != delRight);
                        if (!isChanged)
                        {
                            isChanged = true;
                        }
                    }
                }

                if (isChanged)
                {
                    query_sb.Replace(',', ')', query_sb.Length - 1, 1);

                    ChangeDataInDB(query_sb.ToString());
                }
                else
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
            }
        }

        // Получить права пользователя в системе
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            CheckConnection();

            List<string> permissionIDs = new();

            if (IsUserExists(userLogin))
            {
                var perms = ReadDataFromDB($"SELECT rightId FROM [{connect!.Database}].[{schemaName}].[UserRequestRight] WHERE userId = '{userLogin}'").AsEnumerable();
                foreach (var row in perms)
                {
                    permissionIDs.Add(row.ItemArray[0].ToString());
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