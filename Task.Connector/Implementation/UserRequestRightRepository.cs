using Task.Connector.Interfaces;

namespace Task.Connector.Implementation
{
    public class UserRequestRightRepository : Repository<Task.Integration.Data.DbCommon.DbModels.UserRequestRight>, IUserRequestRightRepository
    {
        public UserRequestRightRepository(Integration.Data.DbCommon.DataContext context) : base(context)
        {

        }

        public void AddUserPermissions(string userId, string rightId)
        {
            if (int.TryParse(rightId, out int i))
                ObjectSet.Add(new Task.Integration.Data.DbCommon.DbModels.UserRequestRight() { UserId = userId, RightId = i });
        }

        public void RemoveUserPermissions(string userId, IEnumerable<string> rightIds)
        {
            foreach (var id in rightIds)
            {
                if (int.TryParse(id.Split(":", StringSplitOptions.RemoveEmptyEntries)[1], out int i))
                {
                    var userPermission = ObjectSet.FirstOrDefault(x => x.UserId == userId && x.RightId == i);
                    if (userPermission != null)
                        ObjectSet.Remove(userPermission);
                }
            }
        }
    }
}
