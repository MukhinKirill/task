using Task.Connector.Interfaces;

namespace Task.Connector.Implementation
{
    public class ConnectorUoW : IUnitOfWork
    {
        private readonly Integration.Data.DbCommon.DataContext _context;        
        public ConnectorUoW(Integration.Data.DbCommon.DataContext context)
        {
            _context = context;
            UserRepository = new UserRepository(_context);
            RequestRightRepository = new RequestRightRepository(_context);
            UserRequestRightRepository = new UserRequestRightRepository(_context);
            ITRoleRepository = new ITRoleRepository(_context);
            PasswordRepository = new PasswordRepository(_context);
            UserITRoleRepository = new UserITRoleRepository(_context);
        }
        public IUserRepository UserRepository { get; }
        public IRequestRightRepository RequestRightRepository { get; }
        public IUserRequestRightRepository UserRequestRightRepository { get; }
        public IITRoleRepository ITRoleRepository { get; }
        public IPasswordRepository PasswordRepository { get; }
        public IUserITRoleRepository UserITRoleRepository { get; }
        public int Commit()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
