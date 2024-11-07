using Task.Connector.Interfaces;

namespace Task.Connector.Implementation
{
    public class ITRoleRepository : Repository<Task.Integration.Data.DbCommon.DbModels.ITRole>, IITRoleRepository
    {
        public ITRoleRepository(Integration.Data.DbCommon.DataContext context) : base(context)
        {

        }
    }
}
