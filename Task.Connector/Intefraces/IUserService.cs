using Task.Integration.Data.Models.Models;

namespace Task.Connector.Intefraces;

internal interface IUserService
{
	bool IsUserExists(string userLogin);
	void CreateUser(UserToCreate user);
}
