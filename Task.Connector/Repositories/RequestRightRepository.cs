using Task.Connector.Repositories.Interfaces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories;

public class RequestRightRepository : IRequestRightRepository
{
    private readonly DataContext _dbContext; 
    
    public RequestRightRepository(string connectionString)
    {
        var dbContextFactory = new DbContextFactory(connectionString);
        _dbContext = dbContextFactory.GetContext("POSTGRE");
    }

    public IEnumerable<RequestRight> GetAllRequestRights()
    {
        return _dbContext.RequestRights;
    }
}