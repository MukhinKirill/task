using Task.Connector.Repositories;
using Task.Connector.Repositories.Interfaces;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Services;

public class SequrityService : ISequrityService
{
    private ISequrityRepository _sequrityRepository;

    public SequrityService(string connectionString)
    {
        _sequrityRepository = new SequrityRepository(connectionString);
    }
    
    public void CreateSequrity(string login, string password)
    {
        var newSequrity = new Sequrity
        {
            UserId = login,
            Password = password
        };
        
        _sequrityRepository.CreateSequrity(newSequrity);
    }
}