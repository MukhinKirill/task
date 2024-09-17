using System.Text.RegularExpressions;
using Task.Connector.Intefraces;
using Task.Connector.Services;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

public class ConnectorDb : IConnector
{
	private IPermissionService _permissionService;
	private IUserPermissionService _userPermissionService;
	private IUserPropertyService _userPropertyService;
	private IUserService _userService;

	private string _provider = "POSTGRE";

	public ILogger Logger { get; set; }

	public void StartUp(string connectionString)
	{
		var settings = ParseConnectionString(connectionString);

		var contextFactory = new DbContextFactory(settings["ConnectionString"]);

		_permissionService = new PermissionService(contextFactory, _provider);
		_userPermissionService = new UserPermissionService(contextFactory, _provider);
		_userPropertyService = new UserPropertyService(contextFactory, _provider);
		_userService = new UserService(contextFactory, _provider);
	}

	public void CreateUser(UserToCreate user)
	{
		_userService.CreateUser(user);
	}

	public IEnumerable<Property> GetAllProperties()
	{
		Property[] properties =
		{
			new("lastName", "Фамилия"),
			new("firstName", "Имя"),
			new("middleName", "Отчество"),
			new("telephoneNumber", "Номер телефона"),
			new("isLead", "Руководитель"),
			new("password", "Пароль")
		};

		return properties;
	}

	public IEnumerable<UserProperty> GetUserProperties(string userLogin)
	{
		return _userPropertyService.GetUserProperties(userLogin);
	}

	public bool IsUserExists(string userLogin)
	{
		return _userService.IsUserExists(userLogin);
	}

	public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
	{
		_userPropertyService.UpdateUserProperties(properties, userLogin);
	}

	public IEnumerable<Permission> GetAllPermissions()
	{
		return _permissionService.GetAllPermissions();
	}

	public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		_userPermissionService.AddUserPermissions(userLogin, rightIds);
	}

	public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		_userPermissionService.RemoveUserPermissions(userLogin, rightIds);
	}

	public IEnumerable<string> GetUserPermissions(string userLogin)
	{
		return _userPermissionService.GetUserPermissions(userLogin);
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