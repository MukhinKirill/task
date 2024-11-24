using Task.Connector.Models;

namespace Task.Connector.Strategies.Permission;

internal interface IPermissionsStrategy
{
    string Name { get; }
    public void AddUserPermission(string userLogin, SpecifiedPermission permission);

    public void RemoveUserPermission(string userLogin, SpecifiedPermission permission);
}