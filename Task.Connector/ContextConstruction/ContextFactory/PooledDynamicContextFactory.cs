using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Task.Connector.ContextConstruction.ContextFactory
{
    // Предоставляет возможность генерации DbContext через PooledDbContextFactory.

    // Так как (насколько уточняется в спецификации Microsoft)
    // OnConfiguring вызывается лишь один раз для всей фабрики,
    // то создание модели должно тоже проходить всего один раз

    // В теории, если данный код используется синхронно,
    // то poolSize можно указать равным 1, но я решил не рисковать
    public class PooledDynamicContextFactory<T> : IDynamicContextFactory<T> where T : DbContext
    {
        private readonly IDbContextFactory<T> _contextFactory;

        public PooledDynamicContextFactory(IModelGenerator<T> modelGenerator, DbContextOptionsBuilder<T> optionsBuilder, string schemaName, int poolSize = 1024)
        {
            var model = modelGenerator.GenerateModel(schemaName);
            optionsBuilder.UseModel(model);
            _contextFactory = new PooledDbContextFactory<T>(optionsBuilder.Options, poolSize);
        }

        public T CreateContext()
        {
            return _contextFactory.CreateDbContext();
        }
    }
}
