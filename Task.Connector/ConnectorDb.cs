using Microsoft.EntityFrameworkCore;
using Task.Connector;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

public class ConnectorDb : IConnector
{
	private DataContext _context;
	private const char RightIdDelimeter = ':';
	private const string RequestRightGroupName = "Request";
	private const string RoleRightGroupName = "Role";

	public const string PostgreSqlConnectionString = "Host=localhost;Port=5555;Database=Avanpost;Username=postgres;Password=v1nkoredb";

	public ILogger Logger { get; set; }

	public ConnectorDb() { }

	private string GetContextProviderName(string connectionString)
	{
		if (connectionString.Contains("Server"))
		{
			return "MSSQL";
		}

		return "POSTGRE";
	}

	public void StartUp(string connectionString)
	{
		if (connectionString.Contains("Host="))
		{
			connectionString = PostgreSqlConnectionString;
		}
		var dbContextFactory = new DbContextFactory(connectionString);
		_context = dbContextFactory.GetContext(GetContextProviderName(connectionString));
	}

	public void CreateUser(UserToCreate user)
	{
		var newUser = new User()
		{
			Login = user.Login,
		};

		UserPropertyHelper.SetProperties(newUser, user.Properties);
		_context.Users.Add(newUser);

		_context.Passwords.Add(new Sequrity()
		{
			UserId = user.Login,
			Password = user.HashPassword,
		});

		_context.SaveChanges();

		Logger.Debug("User created");
	}

	public bool IsUserExists(string userLogin)
	{
		return _context.Users.Any(u => u.Login == userLogin);
	}

	public IEnumerable<Property> GetAllProperties()
		=> UserPropertyHelper.GetProperties().Select(up => new Property(up.Name, string.Empty));

	public IEnumerable<UserProperty> GetUserProperties(string userLogin)
	{
		var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Login == userLogin);
		if (user is null)
		{
			Logger.Error("User with the specified login was not found");

			return Enumerable.Empty<UserProperty>();
		}

		return UserPropertyHelper.GetProperties(user);
	}

	public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
	{
		var user = _context.Users.FirstOrDefault(u => u.Login == userLogin);
		if (user is null)
		{
			Logger.Error("User with the specified login was not found");

			return;
		}

		UserPropertyHelper.SetProperties(user, properties);

		_context.Update(user);
		_context.SaveChanges();

		Logger.Debug("User properties successfully updated");
	}

	public IEnumerable<Permission> GetAllPermissions()
	{
		var requestRights = _context.RequestRights.AsNoTracking().ToArray();
		var itRoles = _context.ITRoles.AsNoTracking().ToArray();

		var permissions = new List<Permission>();
		permissions.AddRange(requestRights.Select(rr => new Permission(rr.Id.Value.ToString(), rr.Name, string.Empty)));
		permissions.AddRange(itRoles.Select(r => new Permission(r.Id.Value.ToString(), r.Name, r.CorporatePhoneNumber)));

		return permissions;
	}

	public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		if (!IsUserExists(userLogin))
		{
			Logger.Error("User with the specified login was not found");
			return;
		}
		foreach (var right in rightIds)
		{
			var groupName = new string(right.TakeWhile(c => c != RightIdDelimeter).ToArray());
			var id = Convert.ToInt32(new string(right.Skip(groupName.Length + RightIdDelimeter.ToString().Length).ToArray()));
			if (groupName == RequestRightGroupName)
			{
				if (!_context.UserRequestRights.Any(rr => rr.RightId == id))
				{
					_context.UserRequestRights.Add(new UserRequestRight { RightId = id, UserId = userLogin });
				}

				continue;
			}
			if (!_context.UserITRoles.Any(r => r.RoleId == id))
			{
				_context.UserITRoles.Add(new UserITRole { RoleId = id, UserId = userLogin });
			}
		}

		_context.SaveChanges();

		Logger.Debug("Added new permissions for user");
	}

	public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		if (!IsUserExists(userLogin))
		{
			Logger.Error("User with the specified login was not found");

			return;
		}
		foreach (var right in rightIds)
		{
			var groupName = new string(right.TakeWhile(c => c != RightIdDelimeter).ToArray());
			var id = Convert.ToInt32(new string(right.Skip(groupName.Length + RightIdDelimeter.ToString().Length).ToArray()));
			if (groupName == RequestRightGroupName)
			{
				_context.UserRequestRights.Remove(new UserRequestRight { RightId = id, UserId = userLogin });

				continue;
			}
			_context.UserITRoles.Remove(new UserITRole { RoleId = id, UserId = userLogin });
		}

		_context.SaveChanges();

		Logger.Debug("Removed permissions from user");
	}

	public IEnumerable<string> GetUserPermissions(string userLogin)
	{
		return _context.UserRequestRights
			.Where(rr => rr.UserId == userLogin).Select(rr => rr.RightId.ToString())
			.Union(_context.UserITRoles.Where(r => r.UserId == userLogin).Select(r => r.RoleId.ToString()))
			.ToList();
	}
}