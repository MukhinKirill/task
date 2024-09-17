using Task.Connector.Intefraces;
using Task.Integration.Data.DbCommon;

namespace Task.Connector.Services;

internal class UserPermissionService : IUserPermissionService
{
	private DbContextFactory _contextFactory;
	private string _provider;

	internal UserPermissionService(DbContextFactory contextFactory, string provider)
	{
		_contextFactory = contextFactory;
		_provider = provider;
	}

	public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		using var context = _contextFactory.GetContext(_provider);
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetUserPermissions(string userLogin)
	{
		using var context = _contextFactory.GetContext(_provider);
		throw new NotImplementedException();
	}

	public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
	{
		using var context = _contextFactory.GetContext(_provider);
		throw new NotImplementedException();
	}
}
