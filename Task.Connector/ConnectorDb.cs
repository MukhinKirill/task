using Task.Connector.ConnectorVerified;
using Task.Connector.ConnectorVerified.MSSqlServer2019Connector;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;
public class Right
{
	public enum Type
	{
		RequestRight,
		ItRole,
		None
	}

	public Right() => new Right(Type.None, "");

	public Right(Type typeRight, string id)
	{
		Id = id;
		TypeRight = typeRight;
	}

	public Right(string typeRight, string id)
	{
		Id = id;
		switch (typeRight)
		{
			case "Role":
				TypeRight = Type.ItRole; break;
			case "Request":
				TypeRight = Type.RequestRight; break;
			default:
				TypeRight = Type.None; break;
		}
	}

	public Type TypeRight { get; set; }
	public string Id { get; set; }
}

public partial class ConnectorDb : IConnector
{
	const string
		spliter = ";",
		equal = "=",
		no_splt = "\'",
		delimeter = ":";

	//Просто заглушка, что бы программа не падала, если не указать провайдера для логов.
	class EmptyLogs : ILogger
	{
		public void Debug(string message) { }
		public void Warn(string message) { }
		public void Error(string message) { }
	}

	public void StartUp(string connectionString)
	{
		if (Logger == null) Logger = new EmptyLogs();
		FillParam(connectionString);

		const string provider_mark = "Provider";
		const string connection_mark = "ConnectionString";

		string provider = param[provider_mark];
		if (provider.IndexOf(Providers.providers[Providers.ProvidersSupported.SqlServer2019]) != -1)
		{
			ConnectorVerified = new MSSqlServer2019Connector();
		}
		else ExceptionReport(new ConnectorVerificationException(), connectionString);

		string connection = param[connection_mark];
		ConnectorVerified.StartUp(ref connection, this);
	}

	public void CreateUser(UserToCreate user)
	{
		ChekVonnectorVerified();
		ConnectorVerified.CreateUser(ref user);
	}

	public IEnumerable<Property> GetAllProperties()
	{
		ChekVonnectorVerified();
		return ConnectorVerified.GetAllProperties();
	}

	public IEnumerable<UserProperty> GetUserProperties(string userLogin)
	{
		ChekVonnectorVerified();
		ChekUserLogin(userLogin);
		return ConnectorVerified.GetUserProperties(ref userLogin);
	}

	public bool IsUserExists(string userLogin)
	{
		ChekVonnectorVerified();
		ChekUserLogin(userLogin);
		return ConnectorVerified.IsUserExists(ref userLogin);
	}

	public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
	{
		ChekVonnectorVerified();
		ChekUserLogin(userLogin);
		ConnectorVerified.UpdateUserProperties(ref properties, ref userLogin);
	}

	public IEnumerable<Permission> GetAllPermissions()
	{
		ChekVonnectorVerified();
		return ConnectorVerified.GetAllPermissions();
	}

	public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		ChekVonnectorVerified();
		ChekUserLogin(userLogin);
		List<Right> rights = new List<Right>();
		foreach (var i in rightIds)
			rights.Add(new Right(i.Substring(0, i.IndexOf(delimeter)), i.Substring(i.IndexOf(delimeter) + delimeter.Length)));

		ConnectorVerified.AddUserPermissions(ref userLogin, ref rights);
	}

	public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		ChekVonnectorVerified();
		ChekUserLogin(userLogin);
		List<Right> rights = new List<Right>();
		foreach (var i in rightIds)
			rights.Add(new Right(i.Substring(0, i.IndexOf(delimeter)), i.Substring(i.IndexOf(delimeter) + delimeter.Length)));

		ConnectorVerified.RemoveUserPermissions(ref userLogin, ref rights);
	}

	public IEnumerable<string> GetUserPermissions(string userLogin)
	{
		ChekVonnectorVerified();
		ChekUserLogin(userLogin);
		return ConnectorVerified.GetUserPermissions(ref userLogin);
	}


	void ChekVonnectorVerified()
	{
		if (ConnectorVerified == null) ExceptionReport(new ConnectorNotVerifiedException());
	}

	void ChekUserLogin(string userLogin)
	{
		if (userLogin.Trim() == "") ExceptionReport(new Exception("User_login" + "=" + userLogin));
	}

	public void ExceptionReport(Exception exception, string comment = "")
	{
		Logger.Error("Comment: " + comment + "\n" + exception.Message);
		throw exception;
	}

	string GetNextParam(ref string paramList, string paramEnd)
	{
		string ret;
		int index, delete_count = 0;
		if (paramList.StartsWith(no_splt))
		{
			delete_count += no_splt.Length;
			paramList = paramList.Substring(no_splt.Length);
			index = paramList.IndexOf(no_splt);
		}
		else index = paramList.IndexOf(paramEnd);

		delete_count += index + paramEnd.Length;
		if (index == -1 || delete_count > paramList.Length) return "";
		ret = paramList.Substring(0, index);
		paramList = paramList.Substring(delete_count);
		return ret;
	}
	void FillParam(in string paramListIn)
	{
		string paramList = paramListIn.Trim();
		if (!paramList.EndsWith(spliter))
			paramList = paramList + spliter;

		while (paramList.Length != 0)
		{
			string Key = GetNextParam(ref paramList, equal);
			if (Key == "") ExceptionReport(new Exception(), paramListIn);
			string Value = GetNextParam(ref paramList, spliter);
			if (Value == "") ExceptionReport(new Exception(), paramListIn);

			param.Add(Key, Value);
		}
	}

	Dictionary<string, string> param = new Dictionary<string, string>();
	IConnectorVerified ConnectorVerified { get; set; }
	public ILogger Logger { get; set; }
}