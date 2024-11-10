using Microsoft.EntityFrameworkCore;
using Task.Connector.Domain;

namespace Task.Connector.Infrastructure.Repository;

public class Repository<TEntity, TContext> : IRepository<TEntity, TContext>
    where TEntity : EntityBase
    where TContext : DbContext
{
    protected TContext DbContext;
    protected DbSet<TEntity> DbSet;

    public Repository(TContext dBContext)
    {
        DbContext = dBContext;
        DbSet = DbContext.Set<TEntity>();
    }
}
