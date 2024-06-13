using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories.Interfaces;

public interface ISequrityRepository
{
    void CreateSequrity(Sequrity newSequrity);
}