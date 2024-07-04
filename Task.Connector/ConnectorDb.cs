using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using Task.Connector.Domain;
using Task.Connector.Extensions;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private IDbConnection _connection;

        private string _schema;

        private string _defaultSchema = "TestTaskSchema";

        public void StartUp(string connectionString)
        {
            var connectionArgs = connectionString.Replace("';", "|").Replace("='", "|").Split('|');

            _schema = connectionArgs[5];

            _connection = connectionArgs[3].Contains("Postgre") ? new NpgsqlConnection(connectionArgs[1]) : new SqlConnection(connectionArgs[1]);
        }

        private string ToTable(string name) => @"""" + _defaultSchema + @"""" + "." + @"""" + name + @"""";

        public void CreateUser(UserToCreate user)
        {
            var baseProps = UserProperties;
            foreach (var prop in baseProps)
            {
                if (user.Properties.FirstOrDefault(p => p.Name == prop.Name) != null)
                    continue;
                user.Properties = user.Properties.Append(new UserProperty(prop.Name, ""));
            }

            try
            {
                var parms = user.Properties.ToDictionary(kvp => kvp.Name, kvp => kvp.Value);

                _connection.ExecuteCommand(
                    new Instruction
                    {
                        Text = $"INSERT INTO {ToTable("User")}(login, {string.Join(", ", parms.Select(x => x.Key.ToFormat()))})"
                                + $" VALUES (@login, {string.Join(", ", parms.Select(x => $"@{x.Key}"))});",
                        Parameters = new Dictionary<string, object>
                        {
                            { "login", user.Login },
                            { "lastName", parms["lastName"] ?? "" },
                            { "firstName", parms["firstName"] ?? "" },
                            { "middleName", parms["middleName"] ?? "" },
                            { "telephoneNumber", parms["telephoneNumber"] ?? "" },
                            { "isLead", Convert.ToBoolean(parms["isLead"])},
                        }
                    },
                    new Instruction
                    {
                        Text = $"INSERT INTO {ToTable("Passwords")}({"userId".ToFormat()}, password)"
                                + $" VALUES (@login, @password)",
                        Parameters = new Dictionary<string, object>
                        {
                            { "login", user.Login },
                            { "password", user.HashPassword }
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }
        }

        private IEnumerable<Property> UserProperties => new List<Property>
        {
            new Property("lastName", string.Empty),
            new Property("firstName", string.Empty),
            new Property("middleName", string.Empty),
            new Property("telephoneNumber", string.Empty),
            new Property("isLead", string.Empty),
        };

        public IEnumerable<Property> GetAllProperties() => UserProperties.Append(new Property("password", string.Empty));

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                var propFields = UserProperties.Select(x => x.Name);

                var results = _connection.ExecuteQuery(
                    new Instruction
                    {
                        Text = $"SELECT {string.Join(", ", propFields.Select(x => x.ToFormat()))} FROM {ToTable("User")}"
                                + $" WHERE login = @login",
                        Parameters = new Dictionary<string, object>
                        {
                            { "login", userLogin }
                        }
                    },
                    mapper: reader =>
                    {
                        var results = new List<UserProperty>();
                        while (reader.Read())
                            for (var i = 0; i < reader.FieldCount; i++)
                                results.Add(new(propFields.ElementAt(i), reader.GetValue(i).ToString()));

                        return results;
                    }
                );

                return results;
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }

            return new List<UserProperty>();
        }

        public bool IsUserExists(string userLogin)
        {
            try
            {
                var result = _connection.ExecuteQuery(
                    new Instruction
                    {
                        Text = $"SELECT EXISTS (SELECT login FROM {ToTable("User")} WHERE login = @login)",
                        Parameters = new Dictionary<string, object>
                        {
                            { "login", userLogin }
                        }
                    },
                    mapper: reader =>
                    {
                        while (reader.Read())
                            return Convert.ToBoolean(reader[0]);

                        return false;
                    }
                );

                return result;
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }

            return false;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var parms = new Dictionary<string, object> { { "login", userLogin } };
                foreach (var prop in properties)
                    parms.Add(prop.Name, prop.Name == "isLead" ? Convert.ToBoolean(prop.Value) : prop.Value);

                _connection.ExecuteCommand(
                    new Instruction
                    {
                        Text = $"UPDATE {ToTable("User")} SET {string.Join(", ", properties.Select(x => $"{x.Name.ToFormat()} = @{x.Name}"))}"
                                + $" WHERE login = @login",
                        Parameters = parms
                    }
                );
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                var rightList = _connection.ExecuteQuery(
                    new Instruction
                    {
                        Text = $"SELECT id, name FROM {ToTable("RequestRight")}"
                    },
                    mapper: reader =>
                    {
                        var results = new List<Permission>();
                        while (reader.Read())
                            results.Add(new Permission(reader[0].ToString(), reader[1].ToString(), "RequestRight"));

                        return results;
                    }
                );

                var roleList = _connection.ExecuteQuery(
                    new Instruction
                    {
                        Text = $"SELECT id, name FROM {ToTable("ItRole")}"
                    },
                    mapper: reader =>
                    {
                        var results = new List<Permission>();
                        while (reader.Read())
                            results.Add(new Permission(reader[0].ToString(), reader[1].ToString(), "ItRole"));

                        return results;
                    }
                );

                return rightList.Concat(roleList);
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }

            return new List<Permission>();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var baseParms = new Dictionary<string, object>()
                {
                    { "login", userLogin }
                };

                var roleIds = rightIds.Where(x => x.ToLower().Contains("role"));
                var requestRightIds = rightIds.Where(x => x.ToLower().Contains("right") || x.ToLower().Contains("request"));

                var instructions = new List<Instruction>();

                if (roleIds.Any() is true)
                {
                    var parms = new Dictionary<string, object>();
                    foreach (var role in roleIds)
                    {
                        var pair = role.Split(':');
                        parms.Add(pair[0], int.Parse(pair[1]));
                    }

                    var command = $"INSERT INTO {ToTable("UserITRole")}({"userId".ToFormat()}, {"roleId".ToFormat()})"
                                + $" VALUES {string.Join(", ", parms.Select(x => $"(@login, @{x.Key})"))}";

                    var roleInstructionParms = baseParms.Concat(parms).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    instructions.Add(new Instruction
                    {
                        Text = command,
                        Parameters = roleInstructionParms
                    });
                }

                if (requestRightIds.Any() is true)
                {
                    var parms = new Dictionary<string, object>();
                    foreach (var request in requestRightIds)
                    {
                        var pair = request.Split(':');
                        parms.Add(pair[0], int.Parse(pair[1]));
                    }

                    var command = $"INSERT INTO {ToTable("UserRequestRight")}({"userId".ToFormat()}, {"rightId".ToFormat()})"
                                + $" VALUES {string.Join(", ", parms.Select(x => $"(@login, @{x.Key})"))}";

                    var requestInstructionParms = baseParms.Concat(parms).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    instructions.Add(new Instruction
                    {
                        Text = command,
                        Parameters = requestInstructionParms
                    });
                }

                _connection.ExecuteCommand(instructions.ToArray());
            }
            catch (Exception ex)
            {
                //Loger.Error(ex.Message);
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var baseParms = new Dictionary<string, object>()
                {
                    { "login", userLogin }
                };

                var roleIds = rightIds.Where(x => x.ToLower().Contains("role"));
                var requestRightIds = rightIds.Where(x => x.ToLower().Contains("right") || x.ToLower().Contains("request"));

                var instructions = new List<Instruction>();

                if (roleIds.Any() is true)
                {
                    var parms = new Dictionary<string, object>();
                    foreach (var role in roleIds)
                    {
                        var pair = role.Split(':');
                        parms.Add(pair[0], int.Parse(pair[1]));
                    }

                    var command = $"DELETE FROM {ToTable("UserITRole")} WHERE {"userId".ToFormat()} = @login AND {"roleId".ToFormat()} IN ({string.Join(", ", parms.Select(x => $"@{x.Key}"))})";
                    var roleInstructionParms = baseParms.Concat(parms).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    instructions.Add(new Instruction
                    {
                        Text = command,
                        Parameters = roleInstructionParms
                    });
                }

                if (requestRightIds.Any() is true)
                {
                    var parms = new Dictionary<string, object>();
                    foreach (var request in requestRightIds)
                    {
                        var pair = request.Split(':');
                        parms.Add(pair[0], int.Parse(pair[1]));
                    }

                    var command = $"DELETE FROM {ToTable("UserRequestRight")} WHERE {"userId".ToFormat()} = @login AND {"rightId".ToFormat()} IN ({string.Join(", ", parms.Select(x => $"@{x.Key}"))})";

                    var requestInstructionParms = baseParms.Concat(parms).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    instructions.Add(new Instruction
                    {
                        Text = command,
                        Parameters = requestInstructionParms
                    });
                }

                _connection.ExecuteCommand(instructions.ToArray());
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                var parms = new Dictionary<string, object>
                {
                    { "login", userLogin }
                };

                var roleIds = _connection.ExecuteQuery(
                    new Instruction
                    {
                        Text = $"SELECT {"roleId".ToFormat()} FROM {ToTable("UserITRole")} WHERE {"userId".ToFormat()} = @login",
                        Parameters = parms
                    },
                    mapper: reader =>
                    {
                        var results = new List<string>();
                        while (reader.Read())
                            results.Add($"Role:{reader[0]}");

                        return results;
                    }
                );

                var rightIds = _connection.ExecuteQuery(
                    new Instruction
                    {
                        Text = $"SELECT {"rightId".ToFormat()} FROM {ToTable("UserRequestRight")} WHERE {"userId".ToFormat()} = @login",
                        Parameters = parms
                    },
                    mapper: reader =>
                    {
                        var results = new List<string>();
                        while (reader.Read())
                            results.Add($"RequestRight:{reader[0]}");

                        return results;
                    }
                );

                return roleIds.Concat(rightIds);
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }

            return new List<string>();
        }

        public ILogger Logger { get; set; }
    }
}