using Task.Connector.Interfaces;

namespace Task.Connector.Implementation
{
    public class UserRepository : Repository<Task.Integration.Data.DbCommon.DbModels.User>, IUserRepository
    {
        public UserRepository(Integration.Data.DbCommon.DataContext context) : base(context)
        {
        }

        public Task.Integration.Data.DbCommon.DbModels.User GetById(string id) => ObjectSet.FirstOrDefault(x => x.Login == id);
    }
}
