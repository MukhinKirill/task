using Task.Connector.Intefraces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services;

internal class UserPropertyService : IUserPropertyService
{
	private DbContextFactory _contextFactory;
	private string _provider;

	internal UserPropertyService(DbContextFactory contextFactory, string provider)
	{
		_contextFactory = contextFactory;
		_provider = provider;
	}

	public IEnumerable<UserProperty> GetUserProperties(string userLogin)
	{
		using var context = _contextFactory.GetContext(_provider);
		throw new NotImplementedException();
	}

	public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
	{
		using var context = _contextFactory.GetContext(_provider);
		throw new NotImplementedException();
	}
}
