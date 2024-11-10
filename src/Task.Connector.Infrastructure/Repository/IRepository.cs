using Microsoft.EntityFrameworkCore;
using Task.Connector.Domain;

namespace Task.Connector.Infrastructure.Repository;

public interface IRepository<TEntity, TContext>
    where TEntity : EntityBase
    where TContext : DbContext
{
}
