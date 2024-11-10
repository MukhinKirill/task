using System.Data;
using Task.Integration.Data.Models;

namespace Task.Connector.Repositories;

public interface IRepositoryRegistry
{
    public IUserRepository UserRepository { get; }
    public IPermissionRepository PermissionRepository { get; }
}

public class RepositoryRegistry : IRepositoryRegistry
{
    public IUserRepository UserRepository { get; }
    public IPermissionRepository PermissionRepository { get; }

    public RepositoryRegistry(ILogger? logger, IDbConnection connection, string schemaName)
    {
        UserRepository = new UserRepository(
            logger,
            connection,
            schemaName);
        
        PermissionRepository = new PermissionRepository(
            logger,
            connection,
            schemaName);
    }
}