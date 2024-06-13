using Task.Connector.Repositories.Interfaces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories;

public class UserRequestRightRepository : IUserRequestRightRepository
{
    private readonly DataContext _dbContext; 
    
    public UserRequestRightRepository(string connectionString)
    {
        var dbContextFactory = new DbContextFactory(connectionString);
        _dbContext = dbContextFactory.GetContext("POSTGRE");
    }
    
    public IEnumerable<UserRequestRight> GetAllUserRequestRight()
    {
        return _dbContext.UserRequestRights;
    }

    public IQueryable<UserRequestRight> GetUserRequestsRightsByLogin(string login)
    {
        return _dbContext.UserRequestRights
            .Where(urr => urr.UserId == login);
    }

    public void AddUserRequestRight(List<UserRequestRight> userRequestRights)
    {
        _dbContext.UserRequestRights.AddRange(userRequestRights);
        _dbContext.SaveChanges();
    }

    public void RemoveUserRequestRights(List<UserRequestRight> userRequestRights)
    {
        _dbContext.UserRequestRights.RemoveRange(userRequestRights);
        _dbContext.SaveChanges();
    }
}