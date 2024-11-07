namespace Task.Connector.Interfaces
{
    public interface IRequestRightRepository : IRepository<Task.Integration.Data.DbCommon.DbModels.RequestRight>
    {
        List<string> GetRequestRightsNames(List<Task.Integration.Data.DbCommon.DbModels.UserRequestRight> userRequestRights);
    }
}
