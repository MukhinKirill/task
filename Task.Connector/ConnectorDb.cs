using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private SqlConnection? connect;
        private void CheckConnection()
        {
            if (connect is null)
            {
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
            finally
            {
                connect.Close();
            }
        }

        // Конфигурация коннектора через строку подключения (настройки для подключения к ресурсу(строка подключения к бд, 
        // путь к хосту с логином и паролем, дополнительные параметры конфигурации бизнес-логики и тд, формат любой, например: "key1=value1;key2=value2..."
        public void StartUp(string connectionString)
        {
            SqlConnectionStringBuilder cb = new();
            string[] connectionStringSplit = connectionString.Split('|');

            string[] componentsToConnectionDB = connectionStringSplit[0].Split(';');
            for (int i = 0; i < componentsToConnectionDB.Length; i++)
            {
                componentsToConnectionDB[i] = componentsToConnectionDB[i].Split('=')[1];
            }
            cb.DataSource = componentsToConnectionDB[0];
            cb.InitialCatalog = componentsToConnectionDB[1];
            cb.UserID = componentsToConnectionDB[2];
            cb.Password = componentsToConnectionDB[3];
            cb.TrustServerCertificate = Convert.ToBoolean(componentsToConnectionDB[4]);

            connect = new(cb.ToString());
        }

        // Создать пользователя с набором свойств по умолчанию
        public void CreateUser(UserToCreate user)
        {
            CheckConnection();

            if (IsUserExists(user.Login))
            {
                throw new Exception("Пользователь с таким логином уже существует");
            }

            StringBuilder query_sb = new();
            query_sb.Append($"INSERT INTO [AvanpostService].[TestTaskSchema].[User] VALUES ({user.Login}, ");
            
            foreach (var property in GetAllProperties())
            {
                if (!user.Properties.Select(prop => prop.Name).Contains(property.Name))
                {
                    throw new Exception($"Отсутствует свойство {property.Name}");
                }

                query_sb.Append($"{user.Properties.Where(prop => prop.Name == property.Name).Select(prop => prop.Value).First()},");
            }
            query_sb.Replace(',', ')', query_sb.Length - 1, 1);

            ChangeDataInDB(query_sb.ToString());
            ChangeDataInDB($"INSERT INTO [AvanpostService].[TestTaskSchema].[Passwords] VALUES({user.Login}, {user.HashPassword})");
        }

        // Метод позволяет получить все свойства пользователя(смотри Описание системы), пароль тоже считать свойством
        public IEnumerable<Property> GetAllProperties()
        {
            CheckConnection();

            var allProperties = new List<Property>();

            var namesOfProperties = ReadDataFromDB("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('[AvanpostService].[TestTaskSchema].[User]')").AsEnumerable();
            for (int i = 1; i < namesOfProperties.Count(); i++)
            {
                allProperties.Add( new(namesOfProperties.ElementAt(i).ItemArray[0].ToString(), "") );
            }

            namesOfProperties = ReadDataFromDB("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('[AvanpostService].[TestTaskSchema].[Passwords]')").AsEnumerable();
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

            if (!IsUserExists(userLogin))
            {
                throw new Exception("Указанного пользователя не существует");
            }

            StringBuilder query_sb = new();
            query_sb.Append("SELECT ");
            foreach (var propertyName in GetAllProperties())
            {
                query_sb.Append($"{propertyName},");
            }
            query_sb.Replace(',', ' ', query_sb.Length - 1, 1);

            query_sb.Append("FROM [AvanpostService].[TestTaskSchema].[User] ");
            query_sb.Append("JOIN [AvanpostService].[TestTaskSchema].[Passwords] ");
            query_sb.Append("ON login = userid ");
            query_sb.Append($"WHERE login = '{userLogin}'");

            var props = ReadDataFromDB(query_sb.ToString());

            var userProps = new List<UserProperty>();

            for (int i = 0; i < props.AsEnumerable().First().ItemArray.Length; i++)
            {
                userProps.Add( new(props.Columns[i].ColumnName, props.AsEnumerable().First().ItemArray[i].ToString()) );
            }

            return userProps;
        }

        // Проверка существования пользователя
        public bool IsUserExists(string userLogin)
        {
            CheckConnection();

            SqlCommand cmd = new($"SELECT COUNT(*) FROM [AvanpostService].[TestTaskSchema].[User] WHERE login = '{userLogin}'", connect);
            connect!.Open();

            try
            {
                int countUsers = Convert.ToInt32(cmd.ExecuteScalar());

                return countUsers > 0 ? true : false;
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

            if (!IsUserExists(userLogin))
            {
                throw new Exception("Указанного пользователя не существует");
            }

            IEnumerable<UserProperty> propertiesUnique = properties.Distinct();

            if (propertiesUnique.Any())
            {
                bool isChanged = false;
                StringBuilder query_sb = new();
                query_sb.Append("UPDATE [AvanpostService].[TestTaskSchema].[User] SET ");

                foreach (var property in GetAllProperties())
                {
                    if (propertiesUnique.Select(prop => prop.Name).Contains(property.Name))
                    {
                        query_sb.Append($"{propertiesUnique.Where(prop => prop.Name == property.Name).Select(prop => prop.Value).First()},");
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
            }
        }

        // Получить все права в системе (смотри Описание системы клиента)
        public IEnumerable<Permission> GetAllPermissions()
        {
            CheckConnection();

            List<Permission> permissions = new();

            foreach (var typeOfPermission in new string[] { "RequestRight", "ItRole" })
            {
                var perms = ReadDataFromDB($"SELECT * FROM [AvanpostService].[TestTaskSchema].[{typeOfPermission}]");
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
            CheckConnection();

            if (!IsUserExists(userLogin))
            {
                throw new Exception("Указанного пользователя не существует");
            }

            if (rightIds.Any())
            {
                var allRights = GetAllPermissions().Where(perm => perm.Description == "RequestRight").Select(perm => perm.Id);
                var existsRights = GetUserPermissions(userLogin);
                bool isChanged = false;

                StringBuilder query_sb = new();
                query_sb.Append("INSERT INTO [AvanpostService].[TestTaskSchema].[UserRequestRight] VALUES");

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
                    // указать, что такие права уже есть
                }
            }
        }

        // Удалить права пользователю в системе
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            CheckConnection();

            if (rightIds.Any())
            {
                var existsRights = GetUserPermissions(userLogin);
                bool isChanged = false;

                StringBuilder query_sb = new();
                query_sb.Append($"DELETE FROM [AvanpostService].[TestTaskSchema].[UserRequestRight] WHERE userId = '{userLogin}' AND rightId IN (");
                
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
            }
        }

        // Получить права пользователя в системе
        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            CheckConnection();

            if (!IsUserExists(userLogin))
            {
                throw new Exception("Указанного пользователя не существует");
            }

            List<string> permissionIDs = new();

            var perms = ReadDataFromDB($"SELECT rightId FROM [AvanpostService].[TestTaskSchema].[UserRequestRight] WHERE userId = '{userLogin}'").AsEnumerable();
            foreach (var row in perms)
            {
                permissionIDs.Add(row.ItemArray[0].ToString());
            }

            return permissionIDs;
        }

        public ILogger Logger { get; set; }
    }
}