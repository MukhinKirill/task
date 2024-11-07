namespace Task.Connector.Interfaces
{
    public interface IRepository<T>where T : class
    {
        IQueryable<T> GetAll();
        void Add(T entity);
        void Remove(T entity);
    }
}
