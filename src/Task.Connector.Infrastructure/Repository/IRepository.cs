using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Task.Connector.Domain;
using Tasks = System.Threading.Tasks;

namespace Task.Connector.Infrastructure.Repository;

public interface IRepository<TEntity, TContext>
    where TEntity : EntityBase
    where TContext : DbContext
{
    Tasks.Task CreateAsync(TEntity entity, CancellationToken cancellationToken);

    Task<TEntity?> FindAsync(object id, CancellationToken cancellationToken);

    IQueryable<TEntity> GetAll();

    IQueryable<TEntity> GetByPredicate(Expression<Func<TEntity, bool>> predicate);

    Tasks.Task RemoveAsync(object id, CancellationToken cancellationToken);

    Tasks.Task RemoveAsync(TEntity entity, CancellationToken cancellationToken);

    Tasks.Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
}
