using Task.Connector.Repositories.Interfaces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories;

public class SequrityRepository : ISequrityRepository
{
    private readonly DataContext _dbContext; 
    
    public SequrityRepository(string connectionString)
    {
        var dbContextFactory = new DbContextFactory(connectionString);
        _dbContext = dbContextFactory.GetContext("POSTGRE");
    }
    public void CreateSequrity(Sequrity newSequrity)
    {
        _dbContext.Passwords.Add(newSequrity);
        _dbContext.SaveChanges();
    }
}