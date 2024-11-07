using Task.Connector.Interfaces;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Implementation
{
    public class PasswordRepository : Repository<Sequrity>, IPasswordRepository
    {
        public PasswordRepository(Integration.Data.DbCommon.DataContext context) : base(context)
        {
            
        }
    }
}
