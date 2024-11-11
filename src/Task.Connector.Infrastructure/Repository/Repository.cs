using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Task.Connector.Domain;
using Tasks = System.Threading.Tasks;

namespace Task.Connector.Infrastructure.Repository;

public class Repository<TEntity, TContext> : IRepository<TEntity, TContext>
    where TEntity : EntityBase
    where TContext : DbContext
{
    protected TContext DbContext;
    protected DbSet<TEntity> DbSet;

    public Repository(TContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public async Tasks.Task CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity?> FindAsync(object id, CancellationToken cancellationToken)
    {
        return await DbSet.FindAsync(id, cancellationToken);
    }

    public IQueryable<TEntity> GetAll()
    {
        return DbSet.AsQueryable();
    }

    public IQueryable<TEntity> GetByPredicate(Expression<Func<TEntity, bool>> predicate)
    {
        return DbSet.Where(predicate).AsQueryable();
    }

    public async Tasks.Task RemoveAsync(object id, CancellationToken cancellationToken)
    {
        if (await DbSet.FindAsync(id, cancellationToken) is TEntity entity)
        {
            DbSet.Remove(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Tasks.Task RemoveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Remove(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Tasks.Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Update(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
