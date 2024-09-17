using System.Text.RegularExpressions;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

public class ConnectorDb : IConnector
{
	public ILogger Logger { get; set; }

	public void StartUp(string connectionString)
    {
        var settings = ParseConnectionString(connectionString);

        var connectionStr = settings["ConnectionString"];
    }

    public void CreateUser(UserToCreate user)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Property> GetAllProperties()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        throw new NotImplementedException();
    }

    public bool IsUserExists(string userLogin)
    {
        throw new NotImplementedException();
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        throw new NotImplementedException();
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        throw new NotImplementedException();
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        throw new NotImplementedException();
    }



	private static Dictionary<string, string> ParseConnectionString(string connectionString)
	{
		var result = new Dictionary<string, string>();

		// (\w+) - >= 1 буквенно-цифровых символов (ключ)
		// '([^']+)' - >= 1 символов в одинарных кавычках кроме самих кавычек (значение)
		var pattern = @"(\w+)='([^']+)'";

		var matches = Regex.Matches(connectionString, pattern);

		foreach (Match match in matches)
		{
			var key = match.Groups[1].Value;
			var value = match.Groups[2].Value;

			result[key] = value;
		}

		return result;
	}
}