using Task.Connector.Repositories.Interfaces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories;

public class ITRoleRepository : IITRoleRepository
{
    private readonly DataContext _dbContext; 
    
    public ITRoleRepository(string connectionString)
    {
        var dbContextFactory = new DbContextFactory(connectionString);
        _dbContext = dbContextFactory.GetContext("POSTGRE");
    }

    public IEnumerable<ITRole> GetAllITRoles()
    {
        return _dbContext.ITRoles;
    }
}