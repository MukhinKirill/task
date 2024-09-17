using Task.Connector.Intefraces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services;

internal class UserService : IUserService
{
	private DbContextFactory _contextFactory;
	private string _provider;

	internal UserService(DbContextFactory contextFactory, string provider)
	{
		_contextFactory = contextFactory;
		_provider = provider;
	}

	public bool IsUserExists(string userLogin)
	{
		using var context = _contextFactory.GetContext(_provider);
		throw new NotImplementedException();
	}

	public void CreateUser(UserToCreate user)
	{
		using var context = _contextFactory.GetContext(_provider);
		throw new NotImplementedException();
	}
}
