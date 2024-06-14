using Task.Connector.Repositories.Interfaces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories;

public class UserITRoleRepository : IUserITRoleRepository
{
    private readonly DataContext _dbContext; 
    
    public UserITRoleRepository(string connectionString)
    {
        var dbContextFactory = new DbContextFactory(connectionString);
        _dbContext = dbContextFactory.GetContext("POSTGRE");
    }
    
    public void AddUserITRole(List<UserITRole> userITRoles)
    {
        _dbContext.UserITRoles.AddRange(userITRoles);
        _dbContext.SaveChanges();
    }

    public void RemoveUserITRole(List<UserITRole> userITRoles)
    {
        _dbContext.UserITRoles.RemoveRange(userITRoles);
        _dbContext.SaveChanges();
    }

    public IEnumerable<UserITRole> GetAlluserITRole()
    {
        return _dbContext.UserITRoles;
    }

    public IQueryable<UserITRole> GetUserITRolesByLogin(string login)
    {
        return _dbContext.UserITRoles
            .Where(urr => urr.UserId == login);
    }
}