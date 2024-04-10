using Npgsql;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public void StartUp(string connectionString)
        {
            npgsqlConnection = new NpgsqlConnection(connectionString);
            try
            {
                npgsqlConnection.Open();
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        public void CreateUser(UserToCreate user)
        {
            NpgsqlCommand npgsqlCommand = new(@"INSERT INTO ""TestTaskSchema"".""User"" 
                                                          ( ""login"", ""lastName"", ""firstName"", ""middleName"", ""telephoneNumber"", ""isLead"" )
                                                     VALUES ( @login, @lastName, @firstName, @middleName, @telephoneNumber, @isLead )", npgsqlConnection);
            npgsqlCommand.Parameters.AddWithValue("login", user.Login);
            npgsqlCommand.Parameters.AddWithValue("lastName", string.Empty);
            npgsqlCommand.Parameters.AddWithValue("firstName", string.Empty);
            npgsqlCommand.Parameters.AddWithValue("middleName", string.Empty);
            npgsqlCommand.Parameters.AddWithValue("telephoneNumber", string.Empty);
            npgsqlCommand.Parameters.AddWithValue("isLead", bool.Parse(user.Properties.Where(e=>e.Name == "isLead").Single().Value));
            npgsqlCommand.ExecuteNonQuery();

            npgsqlCommand.Parameters.Clear();
            npgsqlCommand.Parameters.AddWithValue("userId", user.Login);
            npgsqlCommand.Parameters.AddWithValue("password", user.HashPassword);
            npgsqlCommand.CommandText = @"INSERT INTO ""TestTaskSchema"".""Passwords""
                                                    ( ""userId"", ""password"" )
                                             VALUES ( @userId, @password )";
            npgsqlCommand.ExecuteNonQuery();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            IEnumerable<Property> result = new List<Property>();
            NpgsqlCommand npgsqlCommand = new(@"SELECT ""lastName"", 
                                                      ""firstName"", 
                                                     ""middleName"",
                                                ""telephoneNumber"", 
                                                         ""isLead"",
                                                     p.""password"" FROM ""TestTaskSchema"".""User"" u
                                                                    JOIN ""TestTaskSchema"".""Passwords"" p
                                                                      ON u.""login"" = p.""userId""", npgsqlConnection);
            NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
            while (npgsqlDataReader.Read())
            { 
                for (int i = 0; i < npgsqlDataReader.FieldCount; i++)
                {

                    result = result.Append(new Property(npgsqlDataReader.GetName(i), npgsqlDataReader.GetFieldType(i).ToString()));

                }
                break;
            }
            npgsqlDataReader.Close();
            return result;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            IEnumerable<UserProperty> result = new List<UserProperty>();
            NpgsqlCommand npgsqlCommand = new(String.Format(@"SELECT ""lastName"", 
                                                                    ""firstName"", 
                                                                    ""middleName"",
                                                               ""telephoneNumber"", 
                                                                        ""isLead"" FROM ""TestTaskSchema"".""User"" WHERE login = '{0}'",userLogin), npgsqlConnection);
            NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
            while (npgsqlDataReader.Read())
            {

                for (int i = 0; i < npgsqlDataReader.FieldCount; i++)
                {
                    if (npgsqlDataReader.GetFieldType(i).ToString() == "System.Boolean")
                    {
                        result = result.Append(new UserProperty(npgsqlDataReader.GetName(i), npgsqlDataReader.GetBoolean(i).ToString()));
                    }
                    else
                    {
                        result = result.Append(new UserProperty(npgsqlDataReader.GetName(i), npgsqlDataReader.GetString(i)));
                    }
                    
                }
                
            }
            npgsqlDataReader.Close();
            return result;

        }

        public bool IsUserExists(string userLogin)
        {
            bool result = false;
            NpgsqlCommand npgsqlCommand = new(String.Format(@"SELECT EXISTS( SELECT 1 FROM ""TestTaskSchema"".""User"" WHERE login = '{0}' LIMIT 1 ) ", userLogin), npgsqlConnection);
            NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
            while (npgsqlDataReader.Read())
            {
                for (int i = 0; i < npgsqlDataReader.FieldCount; i++)
                {
                    result = npgsqlDataReader.GetBoolean(i);
                }
            }
            npgsqlDataReader.Close();
            return result;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            string cmdString = @"UPDATE ""TestTaskSchema"".""User""
                                    SET ";
            foreach (var item in properties)
            {
                cmdString += String.Format(@"""{0}"" = '{1}',",item.Name,item.Value);
            }
            cmdString = cmdString.TrimEnd(',');
            cmdString += String.Format(@" WHERE login = '{0}'", userLogin);
            NpgsqlCommand command = new(cmdString, npgsqlConnection);
            command.ExecuteNonQuery();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            IEnumerable<Permission> result = new List<Permission>();
            NpgsqlCommand npgsqlCommand = new(String.Format(@"SELECT * FROM ""TestTaskSchema"".""RequestRight""
                                                              UNION
                                                              SELECT ""id"", ""name"" FROM ""TestTaskSchema"".""ItRole"""), npgsqlConnection);
            NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
            while (npgsqlDataReader.Read())
            {
                result = result.Append(new Permission(npgsqlDataReader.GetInt32("id").ToString(), npgsqlDataReader.GetString("name"), ""));
            }
            npgsqlDataReader.Close();
            return result;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            NpgsqlCommand command = new()
            {
                Connection = npgsqlConnection
            };
            foreach (var item in rightIds)
            {
                var r_IdArr = item.Split(':');
                if (r_IdArr[0] == "Role")
                {
                    command.CommandText = String.Format(@"INSERT INTO ""TestTaskSchema"".""UserITRole"" 
                                                                ( ""userId"", ""roleId"" ) 
                                                         VALUES ( @userId, @rId )");
                }
                else
                {
                    command.CommandText = String.Format(@"INSERT INTO ""TestTaskSchema"".""UserRequestRight"" 
                                                                ( ""userId"", ""rightId"" ) 
                                                         VALUES ( @userId, @rId )");
                }
                
                command.Parameters.AddWithValue("userId", userLogin);
                command.Parameters.AddWithValue("rId", int.Parse(r_IdArr[1]));
                command.ExecuteNonQuery();
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            NpgsqlCommand command = new()
            {
                Connection = npgsqlConnection
            };
            foreach (var item in rightIds)
            {
                var r_IdArr = item.Split(':');
                if (r_IdArr[0] == "Role")
                {
                    command.CommandText = String.Format(@"DELETE FROM ""TestTaskSchema"".""UserITRole"" 
                                                                WHERE ""userId"" = '{0}'
                                                                  AND ""roleId"" = '{1}'", userLogin, r_IdArr[1]);
                }
                else
                {
                    command.CommandText = String.Format(@"DELETE FROM ""TestTaskSchema"".""UserRequestRight"" 
                                                                WHERE ""userId"" = '{0}'
                                                                  AND ""rightId"" = '{1}'", userLogin, r_IdArr[1]);
                }
                command.ExecuteNonQuery() ; 
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            IEnumerable<string> result = new List<string>();
            NpgsqlCommand npgsqlCommand = new(String.Format(@"SELECT ""rightId"" as ""id"" 
                                                                FROM ""TestTaskSchema"".""UserRequestRight"" as urr
                                                               WHERE urr.""userId"" = '{0}'
                                                               UNION
                                                              SELECT ""roleId"" as ""id"" 
                                                                FROM ""TestTaskSchema"".""UserITRole"" as ui
                                                               WHERE ui.""userId"" = '{0}'", userLogin), npgsqlConnection);
            NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
            while (npgsqlDataReader.Read())
            {
                for (int i = 0; i < npgsqlDataReader.FieldCount; i++)
                {
                    result = result.Append(npgsqlDataReader.GetInt32(i).ToString());
                }
            }
            npgsqlDataReader.Close();
            return result;
        }

        public ILogger Logger { get; set; }

        private NpgsqlConnection npgsqlConnection { get; set; }
    }
}