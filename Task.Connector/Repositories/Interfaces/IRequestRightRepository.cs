using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Repositories.Interfaces;

public interface IRequestRightRepository
{
    IEnumerable<RequestRight> GetAllRequestRights();
}