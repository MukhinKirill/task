using Base.Interfaces.DataAccess;
using Connector.Core.Interfaces.DataAccess.Repositories;

namespace Connector.Core.Interfaces.DataAccess
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        public IRequestRepository RequestRepository { get; }

        public IRoleRepository RoleRepository { get; }

        public IUserRepository UserRepository { get; }
    }
}
