using Task.Connector.Interfaces;

namespace Task.Connector.Implementation
{
    public class RequestRightRepository : Repository<Task.Integration.Data.DbCommon.DbModels.RequestRight>, IRequestRightRepository
    {
        public RequestRightRepository(Integration.Data.DbCommon.DataContext context) : base(context)
        {
            
        }

        public List<string> GetRequestRightsNames(List<Task.Integration.Data.DbCommon.DbModels.UserRequestRight> userRequestRights)
        {
            List<string> requestRights = new List<string>();
            foreach (var userRequestRight in userRequestRights)
            {
                var requestRightName = ObjectSet.FirstOrDefault(x => x.Id == userRequestRight.RightId)?.Name;
                if (requestRightName != null) 
                    requestRights.Add(requestRightName);
            }
            return requestRights;
        }
    }
}
