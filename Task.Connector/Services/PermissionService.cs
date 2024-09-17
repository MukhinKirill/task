using Task.Connector.Intefraces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services;

internal class PermissionService : IPermissionService
{
	private DbContextFactory _contextFactory;
	private string _provider;

	internal PermissionService(DbContextFactory contextFactory, string provider)
	{
		_contextFactory = contextFactory;
		_provider = provider;
	}

	public IEnumerable<Permission> GetAllPermissions()
	{
		using var context = _contextFactory.GetContext(_provider);

		throw new NotImplementedException();
	}
}
