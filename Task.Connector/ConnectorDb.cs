using Task.Connector.Services.PermissionService;
using Task.Connector.Services.UserPermissionService;
using Task.Connector.Services.UserPropertyService;
using Task.Connector.Services.UserService;
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
		var settings = connectionString.ParseConnectionString();
		var contextFactory = new DbContextFactory(settings["ConnectionString"]);

		_permissionService = new PermissionService(contextFactory, _provider);
		_userPermissionService = new UserPermissionService(contextFactory, _provider);
		_userPropertyService = new UserPropertyService(contextFactory, _provider);
		_userService = new UserService(contextFactory, _provider);
	}

	public void CreateUser(UserToCreate user)
	{
		try
		{
			Logger.Debug($"Добавление пользователя. Логин: {user.Login}.");

			_userService.CreateUser(user);

			Logger.Debug($"Пользователь {user.Login} успешно создан.");
		}
		catch (Exception ex)
		{
			Logger.Error($"Ошибка при создании пользователя {user.Login}: " + ex.Message);
			throw;
		}
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
		try
		{
			Logger.Debug($"Получение свойств пользователя. Логин: {userLogin}");

			var properties = _userPropertyService.GetUserProperties(userLogin);

			Logger.Debug($"Свойства пользователя {userLogin} успешно получены." +
				$" Количество свойств: {properties.Count()}");

			return properties;
		}
		catch (Exception ex)
		{
			Logger.Error($"Ошибка при получении свойств пользователя {userLogin}: " + ex.Message);
			throw;
		}
	}

	public bool IsUserExists(string userLogin)
	{
		try
		{
			Logger.Debug($"Проверка существования пользователя. Логин: {userLogin}");

			var exists = _userService.IsUserExists(userLogin);

			Logger.Debug($"Проверка существования пользователя {userLogin} завершена." +
				$" Результат: {exists}");

			return exists;
		}
		catch (Exception ex)
		{
			Logger.Error($"Ошибка при проверке существования пользователя {userLogin}: " + ex.Message);
			throw;
		}
	}

	public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
	{
		try
		{
			Logger.Debug($"Обновление свойств пользователя. Логин: {userLogin}." +
				$" Количество свойств: {properties.Count()}");

			_userPropertyService.UpdateUserProperties(properties, userLogin);

			Logger.Debug($"Свойства пользователя {userLogin} успешно обновлены.");
		}
		catch (Exception ex)
		{
			Logger.Error($"Ошибка при обновлении свойств пользователя {userLogin}: " + ex.Message);
			throw;
		}
	}

	public IEnumerable<Permission> GetAllPermissions()
	{
		try
		{
			Logger.Debug("Получение всех прав доступа.");

			var permissions = _permissionService.GetAllPermissions();

			Logger.Debug($"Права доступа успешно получены. Количество прав: {permissions.Count()}");

			return permissions;
		}
		catch (Exception ex)
		{
			Logger.Error("Ошибка при получении всех прав доступа: " + ex.Message);
			throw;
		}
	}

	public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		try
		{
			Logger.Debug($"Добавление прав пользователю. Логин: {userLogin}." +
				$" Количество прав: {rightIds.Count()}");

			_userPermissionService.AddUserPermissions(userLogin, rightIds);

			Logger.Debug($"Права пользователя {userLogin} успешно добавлены.");
		}
		catch (Exception ex)
		{
			Logger.Error($"Ошибка при добавлении прав пользователю {userLogin}: " + ex.Message);
			throw;
		}
	}

	public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		try
		{
			Logger.Debug($"Удаление прав пользователя. Логин: {userLogin}." +
				$" Количество удаляемых прав: {rightIds.Count()}");

			_userPermissionService.RemoveUserPermissions(userLogin, rightIds);

			Logger.Debug($"Права пользователя {userLogin} успешно удалены.");
		}
		catch (Exception ex)
		{
			Logger.Error($"Ошибка при удалении прав пользователя {userLogin}: " + ex.Message);
			throw;
		}
	}

	public IEnumerable<string> GetUserPermissions(string userLogin)
	{
		try
		{
			Logger.Debug($"Получение прав пользователя. Логин: {userLogin}");

			var permissions = _userPermissionService.GetUserPermissions(userLogin);

			Logger.Debug($"Права пользователя {userLogin} успешно получены." +
				$" Количество прав: {permissions.Count()}");

			return permissions;
		}
		catch (Exception ex)
		{
			Logger.Error($"Ошибка при получении прав пользователя {userLogin}: " + ex.Message);
			throw;
		}
	}
}