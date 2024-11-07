namespace Task.Connector.Interfaces
{
    public interface IUserRepository : IRepository<Task.Integration.Data.DbCommon.DbModels.User>
    {
        Task.Integration.Data.DbCommon.DbModels.User GetById(string id);
    }
}
