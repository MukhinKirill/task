namespace Task.Common.EntityFrameWork;

public interface IRepository<TEntity, TKey> where TEntity : Entity<TKey>, IAggregateRoot
{
}