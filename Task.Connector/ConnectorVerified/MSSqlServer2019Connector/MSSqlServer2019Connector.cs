using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.ConnectorVerified.MSSqlServer2019Connector;

internal class MSSqlServer2019Connector : IConnectorVerified
{
	const int try_count_close = 5;

	class RightSql
	{
		public RightSql() => new RightSql("", "", "");
		public RightSql(string source, string user, string id)
		{
			Source = source;
			User = user;
			Id = id;
		}
		public string Source { get; set; }
		public string User { get; set; }
		public string Id { get; set; }
	}

	static readonly Dictionary<Right.Type, RightSql> RightTableSql = new Dictionary<Right.Type, RightSql>
	{
		{ Right.Type.RequestRight, new RightSql("RequestRight", "UserRequestRight", "rightId") },
		{ Right.Type.ItRole, new RightSql("ItRole", "UserITRole", "roleId") },
		{ Right.Type.None, new RightSql() }
	};

	~MSSqlServer2019Connector()
	{
		TurnOff();
	}

	public void StartUp(ref string connectionString, in ConnectorDb connector)
	{
		Connector = connector;
		Connection = new SqlConnection(connectionString);

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				Connection.Open();
				Connector.Logger.Debug(GetConnectionState());
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException, connectionString);
		}
	}

	public void TurnOff()
	{
		if (Connection.State == ConnectionState.Open)
		{
			Connection.Close();
			Connector.Logger.Debug(GetConnectionState());
		}
	}

	public void CreateUser(ref UserToCreate user)
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName() + "(" + user.Login + ")"));
		string
			spliter = ", ",
			equal = " = ",
			properties_str = "";

		foreach (var i in user.Properties)
			properties_str += "[" + i.Name + "]" + equal + "'" + i.Value + "'" + spliter;
		properties_str = properties_str.TrimEnd(spliter.ToCharArray());
		properties_str += "\r\n";

		string CommandText = "" +
			"IF\r\n" +
			UserExistsSql(user.Login) +
			"\tSELECT CAST(0 AS BIT)\r\n" +

			"ELSE BEGIN\r\n" +
			"\tDECLARE @COLUMN_NUMBER int =\r\n" +
			"\t(SELECT COUNT(*)\r\n" +
			BasePropertiesRulesSql() +
			"\t))\r\n" +

			"\tDECLARE @DEFAULT_VALUE nvarchar(max) = ', '''''\r\n" +
			"\tDECLARE @DEFAULT_VALUES nvarchar(max) = ''\r\n" +
			"\tDECLARE @i int = 0\r\n" +
			"\tWHILE @i < @COLUMN_NUMBER BEGIN\r\n" +
			"\t\tSET @i = @i + 1\r\n" +
			"\t\tSET @DEFAULT_VALUES = @DEFAULT_VALUES + @DEFAULT_VALUE\r\n" +
			"\tEND\r\n" +

			"\tDECLARE @SS nvarchar(max) =\r\n" +
			"\t'INSERT INTO [Avanpost_task].[TestTaskSchema].[User] ([login], [' + (\r\n" +
			"\tSELECT string_agg([COLUMN_NAME], '], [')\r\n" +
			BasePropertiesRulesSql() +
			"\t)) + ']\r\n" +
			"\t) VALUES (\r\n" +
			$"\t\t''{user.Login}''' + @DEFAULT_VALUES +\r\n" +
			"\t')';\r\n" +
			"\tEXECUTE(@SS)" +

			"\tUPDATE [Avanpost_task].[TestTaskSchema].[User] SET\r\n" +
			properties_str +
			$"\tWHERE [login] = '{user.Login}'\r\n" +

			"INSERT INTO [Avanpost_task].[TestTaskSchema].[Passwords] (\r\n" +
			"\t[userId], [password]\r\n" +
			"\t) VALUES (" +
			"\t'" + user.Login + "', '" + user.HashPassword + "'\r\n" +
			"\t)\r\n" +

			"\tSELECT CAST(1 AS BIT)\r\n" +
			"END;";

		bool ret = false;

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					reader.Read();
					ret = reader.GetBoolean(0);
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException, user.Login);
		}
		if (!ret) Connector.ExceptionReport(new ConnectorUserAlreadyExistException());

		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
	}

	/// <summary>
	/// Не до конца понял если честно, что от меня требовалось исходя из задания.
	/// Потому как вроде данные статичны и не изменяются, а с другой стороны зачем-то этот запрос нужен...
	/// 
	/// Сделал правила выборки колонок в BasePropertiesRulesSql()
	/// </summary>
	/// <returns>
	/// </returns>
	public IEnumerable<Property> GetAllProperties()
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName()));
		string CommandText = "" +
			"SELECT [COLUMN_NAME], [TABLE_NAME]\r\n" +
			BasePropertiesRulesSql() +
			"\tOR ([TABLE_NAME] = 'Passwords' AND ([COLUMN_NAME] != 'id' AND [COLUMN_NAME] != 'userId'))" +
			")";
		List <Property> list = new List<Property>();

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					while (reader.Read())
						list.Add(new Property(
							reader.GetString(0),
							reader.GetString(1)));
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException);
		}

		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
		return list;
	}

	/// <summary>
	/// Зная, что написано в GetAllProperties, у меня ещё больше вопросов по реализации данного чуда...
	/// 
	/// Сделал правила выборки колонок в BasePropertiesRulesSql()
	/// </summary>
	/// <param name="userLogin"></param>
	/// <returns></returns>
	public IEnumerable<UserProperty> GetUserProperties(ref string userLogin)
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName() + "(" + userLogin + ")"));
		string CommandText = "" +
			"DECLARE @SS nvarchar(max)\r\n" +
			"SET @SS = 'SELECT [' + (\r\n" +
			"\t\tSELECT string_agg([COLUMN_NAME], '], [')\r\n" +
			BasePropertiesRulesSql() +
			"\t\t)) + ']\r\n" +
			"\tFROM [Avanpost_task].[TestTaskSchema].[User]\r\n" +
			$"\tWHERE [login] = ''{userLogin}''';\r\n" +
			"EXECUTE(@SS)";
		List<UserProperty> list = new List<UserProperty>();

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					reader.Read();
					var schema = reader.GetColumnSchema();
					foreach (var indexer in schema)
						list.Add(new UserProperty(
							indexer.ColumnName,
							reader.GetValue(indexer.ColumnName).ToString()));
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException, userLogin);
		}

		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
		return list;
	}

	public bool IsUserExists(ref string userLogin)
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName() + "(" + userLogin + ")"));
		string CommandText = "" +
			"SELECT CAST(CASE WHEN\r\n" +
			UserExistsSql(userLogin) +
			"\tTHEN 1 ELSE 0 END AS BIT) AS Result";
		bool ret = false;

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					reader.Read();
					ret = reader.GetBoolean(0);
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException, userLogin);
		}

		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
		return ret;
	}

	public void UpdateUserProperties(ref IEnumerable<UserProperty> properties, ref string userLogin)
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName() + "(" + userLogin + ")"));
		string
			spliter = ", ",
			equal = " = ",
			properties_str = "";

		foreach (var i in properties)
			properties_str += "[" + i.Name + "]" + equal + "'" + i.Value + "'" + spliter;
		properties_str = properties_str.TrimEnd(spliter.ToCharArray());
		properties_str += "\r\n";

		string CommandText = "IF\r\n" +
			UserExistsSql(userLogin) +
			"\tBEGIN\r\n" +

			"\tUPDATE [Avanpost_task].[TestTaskSchema].[User] SET\r\n" +
			properties_str +
			$"\tWHERE [login] = '{userLogin}'\r\n" +

			"\tSELECT CAST(1 AS BIT)\r\n" +
			"END;\r\n" +
			"ELSE\r\n" +
			"\tSELECT CAST(0 AS BIT)";

		bool ret = false;

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					reader.Read();
					ret = reader.GetBoolean(0);
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException, userLogin);
		}
		if (!ret) Connector.ExceptionReport(new ConnectorUserNotExistException());
		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
	}

	public IEnumerable<Permission> GetAllPermissions()
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName()));
		string CommandText = "" +
			"SELECT [id], [name], 'ItRole' as [TypePermission]\r\n" +
			"\tFROM [Avanpost_task].[TestTaskSchema].[ItRole]\r\n" +
			"UNION\r\n" +
			"SELECT [id], [name]\r\n, 'RequestRight' as [TypePermission]" +
			"\tFROM [Avanpost_task].[TestTaskSchema].[RequestRight]";
		List<Permission> list = new List<Permission>();

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					while (reader.Read())
						list.Add(new Permission(
							reader.GetValue(0).ToString(),
							reader.GetString(1),
							reader.GetString(2)));
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException);
		}

		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
		return list;
	}

	public void AddUserPermissions(ref string userLogin, ref List<Right> rights)
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName() + "(" + userLogin + ")"));
		string rights_sql = "";
		foreach (var i in rights)
		{
			rights_sql += "" +
				"\tIF NOT EXISTS (\r\n" +
				$"\tSELECT * FROM [Avanpost_task].[TestTaskSchema].[{RightTableSql[i.TypeRight].User}]\r\n" +
				$"\tWHERE [userId] = '{userLogin}' AND [{RightTableSql[i.TypeRight].Id}] = {i.Id}) AND\r\n" +
				"\tEXISTS (\r\n" +
				$"\tSELECT * FROM [Avanpost_task].[TestTaskSchema].[{RightTableSql[i.TypeRight].Source}]\r\n" +
				$"\tWHERE [id] = {i.Id})\r\n" +

				$"\t\tINSERT INTO [Avanpost_task].[TestTaskSchema].[{RightTableSql[i.TypeRight].User}] (\r\n" +
				$"\t\t[userId], [{RightTableSql[i.TypeRight].Id}]\r\n" +
				"\t\t) VALUES (\r\n" +
				$"\t\t'{userLogin}', {i.Id}\r\n" +
				"\t\t)\r\n";
		}

		string CommandText = "" +
			"IF\r\n" +
			UserExistsSql(userLogin) +
			"\tBEGIN\r\n" +
			rights_sql +
			"\tSELECT CAST(1 AS BIT)\r\n" +
			"END;\r\n" +
			"ELSE \r\n" +
			"\tSELECT CAST(0 AS BIT)";

		bool ret = false;

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					reader.Read();
					ret = reader.GetBoolean(0);
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException, userLogin);
		}
		if (!ret) Connector.ExceptionReport(new ConnectorUserNotExistException());
		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
	}

	public void RemoveUserPermissions(ref string userLogin, ref List<Right> rights)
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName() + "(" + userLogin + ")"));
		string rights_sql = "";
		foreach (var i in rights)
		{
			rights_sql += "" +
				"\tIF EXISTS (\r\n" +
				$"\tSELECT * FROM [Avanpost_task].[TestTaskSchema].[{RightTableSql[i.TypeRight].User}]\r\n" +
				$"\tWHERE [userId] = '{userLogin}' AND [{RightTableSql[i.TypeRight].Id}] = {i.Id})\r\n" +

				$"\t\tDELETE [Avanpost_task].[TestTaskSchema].[{RightTableSql[i.TypeRight].User}]\r\n" +
				$"\t\tWHERE [userId] = '{userLogin}' AND [{RightTableSql[i.TypeRight].Id}] = {i.Id}\r\n";
		}

		string CommandText = "" +
			"IF\r\n" +
			UserExistsSql(userLogin) +
			"\tBEGIN\r\n" +
			rights_sql +
			"\tSELECT CAST(1 AS BIT)\r\n" +
			"END;\r\n" +
			"ELSE \r\n" +
			"\tSELECT CAST(0 AS BIT)";

		bool ret = false;

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					reader.Read();
					ret = reader.GetBoolean(0);
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException, userLogin);
		}
		if (!ret) Connector.ExceptionReport(new ConnectorUserNotExistException());
		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
	}

	public IEnumerable<string> GetUserPermissions(ref string userLogin)
	{
		Connector.Logger.Debug(StartFunctionForLog(GetCallerName() + "(" + userLogin + ")"));
		string CommandText = "" +
			"SELECT [name]\r\n" +
			"\tFROM [Avanpost_task].[TestTaskSchema].[User]\r\n" +
			"\tJOIN [Avanpost_task].[TestTaskSchema].[UserRequestRight] ON [login] = [userId]\r\n" +
			"\tJOIN [Avanpost_task].[TestTaskSchema].[RequestRight] ON [rightId] = [id]\r\n" +
			$"\tWHERE [login] = '{userLogin}'\r\n" +
			"UNION\r\n" +
			"SELECT [name]\r\n" +
			"\tFROM [Avanpost_task].[TestTaskSchema].[User]\r\n" +
			"\tJOIN [Avanpost_task].[TestTaskSchema].[UserItRole] ON [login] = [userId]\r\n" +
			"\tJOIN [Avanpost_task].[TestTaskSchema].[ItRole] ON [roleId] = [id]\r\n" +
			$"\tWHERE [login] = '{userLogin}'";
		List<string> list = new List<string>();

		int try_count = 0;
		while (++try_count < try_count_close)
		{
			try
			{
				using (SqlDataReader reader = new SqlCommand(CommandText, Connection).ExecuteReader())
				{
					while (reader.Read())
						list.Add(reader.GetString(0));
				}
				break;
			}
			catch (Exception ex)
			{
				LastException = ex;
				Connector.Logger.Warn(ex.ToString());
			}
		}
		if (try_count == try_count_close)
		{
			TurnOff();
			Connector.ExceptionReport(LastException, userLogin);
		}

		Connector.Logger.Debug(EndFunctionForLog(GetCallerName()));
		return list;
	}

	string GetConnectionState() { return Connection.ToString() + "=" + Connection.State.ToString(); }

	string StartFunctionForLog(string function) { return "Start " + function; }

	string EndFunctionForLog(string function) { return "End " + function; }

	string BasePropertiesRulesSql()
	{
		return
			"FROM INFORMATION_SCHEMA.COLUMNS\r\n" +
			"WHERE [TABLE_CATALOG] = 'Avanpost_task' AND\r\n" +
			"\t[TABLE_SCHEMA] = 'TestTaskSchema' AND\r\n" +
			"\t(([TABLE_NAME] = 'User' AND [COLUMN_NAME] != 'login')\r\n";
	}

	string UserExistsSql(string userLogin)
	{
		return
			"EXISTS(\r\n" +
			"\t\tSELECT * FROM [Avanpost_task].[TestTaskSchema].[User]\r\n" +
			$"\t\t\tWHERE [login] = '{userLogin}')\r\n";
	}

	static string GetCallerName([CallerMemberName] string name = "") { return name; }

	Exception LastException { get; set; }
	SqlConnection Connection { get; set; }
	ConnectorDb Connector {  get; set; }
}