using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories.Interfaces;

public interface IITRoleRepository
{
    IEnumerable<ITRole> GetAllITRoles();
}