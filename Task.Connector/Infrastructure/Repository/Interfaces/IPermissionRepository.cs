using Task.Connector.Domain.Models;
using Task.Connector.Infrastructure.DataModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure.Repository.Interfaces;

public interface IPermissionRepository
{
    public IEnumerable<Permission> GetAllPermissions();
    public IEnumerable<PermissionDataModel> GetUserPermissions(string login);
    public void AddRolePermissions(IEnumerable<UserItRole> permissions);
    public void AddRequestPermissions(IEnumerable<UserRequestRight> permissions);
    public void RemoveRequestPermissions(IEnumerable<UserRequestRight> permissions);
    public void RemoveRolePermissions(IEnumerable<UserItRole> permissions);
}