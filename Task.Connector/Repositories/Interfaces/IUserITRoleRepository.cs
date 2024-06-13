using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories.Interfaces;

public interface IUserITRoleRepository
{
    void AddUserITRole(List<UserITRole> userITRoles);
    void RemoveUserITRole(List<UserITRole> userITRoles);
    
    IEnumerable<UserITRole> GetAlluserITRole();
    IQueryable<UserITRole> GetUserITRolesByLogin(string login);
}