using Microsoft.EntityFrameworkCore;
using Task.Connector.Interfaces;

namespace Task.Connector.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public Repository(Integration.Data.DbCommon.DataContext context)
        {
            Context = context;
            ObjectSet = context.Set<T>();
        }
        public void Add(T entity)
        {
            ObjectSet.Add(entity);
        }

        public void Remove(T entity)
        {
            var entry = Context.Entry(entity);
            if (entry != null && entry.State != EntityState.Detached)
                ObjectSet.Remove(entity);

        }

        public IQueryable<T> GetAll()
        {
            return ObjectSet;
        }
        protected DbSet<T> ObjectSet;
        protected Integration.Data.DbCommon.DataContext Context;
    }
}
