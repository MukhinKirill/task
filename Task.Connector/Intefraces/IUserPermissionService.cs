namespace Task.Connector.Intefraces;

internal interface IUserPermissionService
{
	IEnumerable<string> GetUserPermissions(string userLogin);
	void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
	void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
}
